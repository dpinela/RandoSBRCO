using RandoSettingsManager.SettingsManagement.Versioning;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RandoSBRCO;

internal class StructuralVersioningPolicy : VersioningPolicy<Signature>
{
    public override Signature Version => new() { FeatureSet = FeatureSetForSettings(RandoSBRCO.Instance.GS) };

    private static List<string> FeatureSetForSettings(SBRCOSettings rs) =>
        SupportedFeatures.Where(f => f.feature(rs)).Select(f => f.name).ToList();

    public override bool Allow(Signature s) => s.FeatureSet.All(name => SupportedFeatures.Any(sf => sf.name == name));

    private static List<(Predicate<SBRCOSettings> feature, string name)> SupportedFeatures = new()
    {
        (rs => rs.LogicEnabled, "Logic"),
        (rs => rs.DupeGrubs > 0, "DupeGrubs")
    };
}

internal struct Signature
{
    public List<string> FeatureSet;
}
