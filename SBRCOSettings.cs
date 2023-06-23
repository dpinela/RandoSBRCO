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
        var allCharms = Data.VanillaCharmOrder.Concat(Data.TranscendenceCharmOrder).ToList();
        return Data.KthPermutation(55, new BigInteger(Convert.FromBase64String(SBRCode).Reverse().ToArray()))
            .ToList();
    }   
}