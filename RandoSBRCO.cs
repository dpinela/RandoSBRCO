using System.Collections.Generic;
using System.Linq;
using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.Placements;
using ItemChanger.Locations.SpecialLocations;
using Modding;
using RandomizerCore.Logic;
using RandomizerMod.RandomizerData;
using RandomizerMod.IC;
using RandomizerMod.RC;

namespace RandoSBRCO;

public class RandoSBRCO : Mod, IGlobalSettings<SBRCOSettings>
{
    public static RandoSBRCO Instance;
    public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

    public SBRCOSettings GS { get; private set; } = new SBRCOSettings();
    public void OnLoadGlobal(SBRCOSettings s) => GS = s;
    public SBRCOSettings OnSaveGlobal() => GS;
    
    public RandoSBRCO()
    {
        Instance = this;
    }

    public override void Initialize()
    {
        var randoExists = ModHooks.GetMod("Randomizer 4") is Mod;
        if (!randoExists)
        {
            Log("Cannot find rando, bye");
            return;
        }
        RequestBuilder.OnUpdate.Subscribe(0.1f, EditCosts);
        RequestBuilder.OnUpdate.Subscribe(20.2f, DupeGrubs);
        Finder.GetLocationOverride += MakeNMGShiny;
        MenuHolder.Hook();
    }

    private static Cost NCharmCost(int n) =>
        new PDIntCost(n, nameof(PlayerData.charmsOwned), $"{n} previous charms required.");
    
    private void EditCosts(RequestBuilder rb)
    {
        if (!GS.LogicEnabled) return;

        var charms = GS.GetCharmOrder();

        rb.CostConverters.Subscribe(1, (LogicCost lc, out Cost c) =>
        {
            if (!(lc is SimpleCost sc))
            {
                c = default;
                return false;
            }
            
            if (sc.term.Name == "WHITEFRAGMENT" && sc.threshold == 2)
            {
                c = NCharmCost(charms.IndexOf(35) + 1);
                Log($"Converting {lc} to {c}");
                return true;
            }
            else if (sc.threshold == 1)
            {
                var i = charms.FindIndex(j => Data.Charm(j) == sc.term.Name);
                if (i != -1)
                {
                    c = NCharmCost(i + 1);
                    Log($"Converting {lc} to {c}");
                    return true;
                }
            }
            c = default;
            return false;
        });

        var vanillaShopCharms = new (string, DefaultShopItems) []
        {
            (LocationNames.Sly, DefaultShopItems.SlyCharms),
            (LocationNames.Sly_Key, DefaultShopItems.SlyKeyCharms),
            (LocationNames.Iselda, DefaultShopItems.IseldaCharms),
            (LocationNames.Salubra, DefaultShopItems.SalubraCharms),
            (LocationNames.Leg_Eater, DefaultShopItems.LegEaterCharms)
        };

        if (charms.Count != 40 && !rb.gs.PoolSettings.CharmNotches)
        {
            rb.AddToPreplaced(new VanillaDef(ItemNames.Salubras_Blessing, LocationNames.Salubra, new CostDef[]
            {
                new("GEO", 801),
                new("CHARMS", charms.Count)
            }));
            // Remove the original Salubra's Blessing (which costs 40 charms).
            rb.RemoveFromVanilla(ItemNames.Salubras_Blessing);
            vanillaShopCharms[3] = (LocationNames.Salubra, DefaultShopItems.SalubraCharms | DefaultShopItems.SalubraBlessing);
        }

        // Rando is not aware of the vanilla costs of shop charms, so we must restore them
        // ourselves.
        // Normally, every item placed at a shop gets a -1 geo cost set, which is replaced
        // during onRandomizerFinish with a randomized cost. However, preplaced items do not
        // receive the randomized cost and keep the -1 instead. This is a bug in rando,
        // which allows us to compensate for the -1 by increasing all of these costs by 1.
        // When the bug is fixed, we can instead override onRandomizerFinish on these items
        // and replace the -1 with the actual cost of the charms.
        var vanillaCharmCosts = new Dictionary<string, int>()
        {
            {ItemNames.Gathering_Swarm, 301},
            {ItemNames.Stalwart_Shell, 201},
            {ItemNames.Heavy_Blow, 351},
            {ItemNames.Sprintmaster, 401},
            {ItemNames.Wayward_Compass, 221},
            {ItemNames.Lifeblood_Heart, 251},
            {ItemNames.Longnail, 301},
            {ItemNames.Steady_Body, 121},
            {ItemNames.Shaman_Stone, 221},
            {ItemNames.Quick_Focus, 801},
            {ItemNames.Fragile_Heart, 351},
            {ItemNames.Fragile_Greed, 251},
            {ItemNames.Fragile_Strength, 601}
        };

        var vanillaGrubCosts = new Dictionary<string, int>()
        {
            {ItemNames.Grubsong, 10},
            {ItemNames.Grubberflys_Elegy, 46}
        };

        foreach (var (shop, shopCharms) in vanillaShopCharms)
        {
            rb.EditLocationRequest(shop, info =>
            {
                var orig = info.customPlacementFetch;
                info.customPlacementFetch = (factory, placement) =>
                {
                    var p = orig(factory, placement);
                    if (p is ShopPlacement sp)
                    {
                        sp.defaultShopItems &= ~shopCharms;
                    }
                    else
                    {
                        LogWarn($"placement for shop {shop} isn't a ShopPlacement; can't remove vanilla charms");
                    }
                    return p;
                };
            });
        }

        rb.EditLocationRequest(LocationNames.Grubfather, info =>
        {
            var orig = info.customPlacementFetch;
            info.customPlacementFetch = (factory, placement) =>
            {
                var p = orig(factory, placement);
                var t = p.GetOrAddTag<DestroyGrubRewardTag>();
                t.destroyRewards |= GrubfatherRewards.Grubsong | GrubfatherRewards.GrubberflysElegy;
                return p;
            };
            // Rando adds its own randomized cost to all Grubfather and Seer locations, even for items
            // that already have grub/essence costs of their own already, and offers no way to prevent
            // this at the source. This is the next best thing, but it means the extraneous costs
            // still affect logic.
            info.customAddToPlacement = (factory, randoPlacement, icPlacement, item) =>
            {
                if (randoPlacement.Location is RandoModLocation rl && rl.costs != null)
                {
                    if (vanillaGrubCosts.TryGetValue(item.name, out var g))
                    {
                        rl.costs.RemoveAll(c => c is SimpleCost sc && sc.term.Name == "GRUBS" && sc.threshold != g);
                    }
                    CostConversion.HandleCosts(factory, rl.costs, item, icPlacement);
                }
                icPlacement.Add(item);
            };
        });

        rb.EditLocationRequest(LocationNames.Seer, info =>
        {
            var orig = info.customPlacementFetch;
            info.customPlacementFetch = (factory, placement) =>
            {
                var p = orig(factory, placement);
                var t = p.GetOrAddTag<DestroySeerRewardTag>();
                t.destroyRewards |= SeerRewards.DreamWielder;
                return p;
            };
            info.customAddToPlacement = (factory, randoPlacement, icPlacement, item) =>
            {
                if (randoPlacement.Location is RandoModLocation rl && rl.costs != null)
                {
                    if (item.name == ItemNames.Dream_Wielder)
                    {
                        rl.costs.RemoveAll(c => c is SimpleCost sc && sc.term.Name == "ESSENCE" && sc.threshold != 500);
                    }
                    CostConversion.HandleCosts(factory, rl.costs, item, icPlacement);
                }
                icPlacement.Add(item);
            };
        });

        for (var i = 1; i < charms.Count; i++)
        {
            var previousCharm = Data.Charm(charms[i-1]);
            var nextCharm = Data.Charm(charms[i]);

            List<VanillaDef> oldVDs;

            if (charms[i] >= Data.VanillaCharmOrder.Length)
            {
                oldVDs = rb.Preplaced.Values.SelectMany(vds => vds.Where(vd => vd.Item == nextCharm || vd.Location == nextCharm)).ToList();

                foreach (var oldVD in oldVDs)
                {
                    rb.RemoveFromPreplaced(oldVD);
                    Log($"Amending {oldVD.Item} at {oldVD.Location} to require {previousCharm}");
                    var newCost = previousCharm == "KINGSOUL" ? new CostDef("WHITEFRAGMENT", 2) : new CostDef(previousCharm, 1);
                    var newCosts = oldVD.Costs is { } oldCosts
                        ? oldCosts.Append(newCost).ToArray()
                        : new[] { newCost };
                    rb.AddToPreplaced(new VanillaDef(oldVD.Item, oldVD.Location, newCosts));
                }
            }
            else
            {
                if (nextCharm == "KINGSOUL")
                {
                    oldVDs = rb.Vanilla.Values.SelectMany(vds =>
                        vds.Where(vd => vd.Item is "King_Fragment" or "Queen_Fragment")).ToList();
                }
                else
                {
                    oldVDs = rb.Vanilla.Values.SelectMany(vds => vds.Where(vd => vd.Item == nextCharm || vd.Location == nextCharm)).ToList();
                }

                foreach (var oldVD in oldVDs)
                {
                    rb.RemoveFromVanilla(oldVD);
                    Log($"Amending {oldVD.Item} at {oldVD.Location} to require {previousCharm}");
                    var newCost = previousCharm == "KINGSOUL" ? new CostDef("WHITEFRAGMENT", 2) : new CostDef(previousCharm, 1);
                    var newCosts = new List<CostDef>();
                    if (oldVD.Costs != null)
                    {
                        newCosts.AddRange(oldVD.Costs);
                    }
                    newCosts.Add(newCost);
                    if (vanillaCharmCosts.TryGetValue(oldVD.Item, out var geo))
                    {
                        newCosts.Add(new("GEO", geo));
                    }
                    rb.AddToPreplaced(new VanillaDef(oldVD.Item, oldVD.Location, newCosts.ToArray()));
                }
            }
        }
    }

    private void DupeGrubs(RequestBuilder rb)
    {
        for (var i = 0; i < GS.DupeGrubs; i++) rb.AddItemByName(ItemNames.Grub);
    }

    private void MakeNMGShiny(GetLocationEventArgs args)
    {
        // The Nailmaster's Glory location normally does not support costs of any kind.
        // Turn it into a shiny to rectify this.
        if (GS.LogicEnabled && args.LocationName == LocationNames.Nailmasters_Glory)
        {
            args.Current = new NailmastersGloryObjectLocation()
            {
                name = LocationNames.Nailmasters_Glory,
                sceneName = SceneNames.Room_Sly_Storeroom,
                objectName = "Sly Basement NPC",
                elevation = 0
            };
        }
    }
}
