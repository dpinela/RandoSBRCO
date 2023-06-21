using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using RandomizerCore.Extensions;
using UnityEngine;

namespace RandoSBRCO;

public static class Data
{
    public static readonly string[] VanillaCharmOrder = new []
    {
        "Gathering_Swarm",
        "Wayward_Compass",
        "Grubsong",
        "Stalwart_Shell",
        "Baldur_Shell",
        "Fury_of_the_Fallen",
        "Quick_Focus",
        "Lifeblood_Heart",
        "Lifeblood_Core",
        "Defender's_Crest",
        "Flukenest",
        "Thorns_of_Agony",
        "Mark_of_Pride",
        "Steady_Body",
        "Heavy_Blow",
        "Sharp_Shadow",
        "Spore_Shroom",
        "Longnail",
        "Shaman_Stone",
        "Soul_Catcher",
        "Soul_Eater",
        "Glowing_Womb",
        "Fragile_Heart",
        "Fragile_Greed",
        "Fragile_Strength",
        "Nailmaster's_Glory",
        "Joni's_Blessing",
        "Shape_of_Unn",
        "Hiveblood",
        "Dream_Wielder",
        "Dashmaster",
        "Quick_Slash",
        "Spell_Twister",
        "Deep_Focus",
        "Grubberfly's_Elegy",
        "KINGSOUL",
        "Sprintmaster",
        "Dreamshield",
        "Weaversong",
        "Grimmchild",
        "Bluemoth_Wings",
        "Antigravity_Amulet",
        "Lemm's_Strength",
        "Florist's_Blessing",
        "Snail_Slash",
        "Snail_Soul",
        "Shaman_Amp",
        "Nitro_Crystal",
        "Crystalmaster",
        "Disinfectant_Flask",
        "Millibelle's_Blessing",
        "Greedsong",
        "Marissa's_Audience",
        "Chaos_Orb",
        "Vespa's_Vengeance"
    };

    public static IEnumerable<int> KthPermutation(int n, BigInteger k)
    {
        var nFact = Enumerable.Range(1, n).Select(i => new BigInteger(i)).Aggregate(BigInteger.One, (x, y) => x * y);
        var pool = Enumerable.Range(0, n).ToList();
        for (var i = 0; i < n; i++)
        {
            nFact /=  n - i;
            var iDigit = k / nFact;
            k -= iDigit * nFact;

            yield return pool.Pop((int) iDigit);
        }
    }
}
