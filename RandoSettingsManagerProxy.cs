using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;

namespace RandoSBRCO;

#nullable enable

internal class RandoSettingsManagerProxy : RandoSettingsProxy<SBRCOSettings, Signature>
{
    public override string ModKey => nameof(RandoSBRCO);

    public override VersioningPolicy<Signature> VersioningPolicy => new StructuralVersioningPolicy();

    public override bool TryProvideSettings(out SBRCOSettings? sent)
    {
        sent = RandoSBRCO.Instance.GS;
        return sent.Enabled();
    }

    public override void ReceiveSettings(SBRCOSettings? received)
    {
        MenuHolder.Instance.Apply(received ?? new());
    }
}