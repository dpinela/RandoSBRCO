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
    public List<string> GetCharmOrder() =>
        Data.KthPermutation(40, new BigInteger(Convert.FromBase64String(SBRCode).Reverse().ToArray()))
            .Select(i => Data.VanillaCharmOrder[i])
            .ToList();
}