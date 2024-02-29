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
        var rawBytes = Convert.FromBase64String(SBRCode);
        // It is not sufficient to reverse the rawBytes; we must add
        // a zero at the end so that the BigInteger constructor is
        // guaranteed to produce a positive number.
        var bytes = new byte[rawBytes.Length + 1];
        for (var i = 0; i < rawBytes.Length; i++)
        {
            bytes[rawBytes.Length - 1 - i] = rawBytes[i];
        }
        return Data.KthPermutation(allCharms.Length, new BigInteger(bytes))
            .ToList();
    }
}