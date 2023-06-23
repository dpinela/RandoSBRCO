using System.Collections.Generic;
using System.Linq;
using ItemChanger;
using Modding;
using RandomizerCore.Logic;
using RandomizerMod.RandomizerData;
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
        MenuHolder.Hook();
    }

    private static Cost NCharmCost(int n) =>
        new PDIntCost(n, nameof(PlayerData.charmsOwned), $"{n} previous charms required");
    
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
                c = NCharmCost(charms.IndexOf(35));
                Log($"Converting {lc} to {c}");
                return true;
            }
            else if (sc.threshold == 1)
            {
                var i = charms.FindIndex(j => Data.Charm(j) == sc.term.Name);
                if (i != -1)
                {
                    c = NCharmCost(i);
                    Log($"Converting {lc} to {c}");
                    return true;
                }
            }
            c = default;
            return false;
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
                    var newCost = new CostDef(previousCharm, 1);
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
                    var newCosts = oldVD.Costs is { } oldCosts
                        ? oldCosts.Append(newCost).ToArray()
                        : new[] { newCost };
                    rb.AddToPreplaced(new VanillaDef(oldVD.Item, oldVD.Location, newCosts));
                }
            }
        }
    }

    private void DupeGrubs(RequestBuilder rb)
    {
        for (var i = 0; i < GS.DupeGrubs; i++) rb.AddItemByName(ItemNames.Grub);
    }
}
