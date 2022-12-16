using System.Collections.Generic;
using System.Linq;
using ItemChanger;
using Modding;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;

namespace RandoSBRCO;

public class RandoSBRCO : Mod, IGlobalSettings<SBRCOSettings>
{
    public static RandoSBRCO Instance;

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

    private void DupeGrubs(RequestBuilder rb)
    {
        for (var i = 0; i < GS.DupeGrubs; i++) rb.AddItemByName(ItemNames.Grub);
    }
    
    private void EditCosts(RequestBuilder rb)
    {
        if (!GS.LogicEnabled) return;
        var charms = GS.GetCharmOrder();

        for (var i = 1; i < charms.Count; i++)
        {
            var previousCharm = charms[i-1];
            var nextCharm = charms[i];

            List<VanillaDef> oldVDs;
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
                rb.AddToVanilla(new VanillaDef(oldVD.Item, oldVD.Location, newCosts));
            }
        }
    }
}
