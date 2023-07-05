using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using MenuChanger.Attributes;

namespace RandoSBRCO;

public class SBRCOSettings
{
    public bool LogicEnabled;
    public int DupeGrubs;
    
    [MenuIgnore]
    public string SBRCode { get; set; }

    [MenuIgnore]
    public List<int> GetCharmOrder()
    {
        var allCharms = SBRCode.Length switch
        {
            28 => Data.VanillaCharmOrder,
            44 => Data.VanillaCharmOrder.Concat(Data.TranscendenceCharmOrder).ToArray(),
            _ => throw new ArgumentException($"Invalid SBRCode length; is {SBRCode.Length}, should be either 28 or 44")
        };
        return Data.KthPermutation(allCharms.Length, new BigInteger(Convert.FromBase64String(SBRCode).Reverse().ToArray()))
            .ToList();
    }
}