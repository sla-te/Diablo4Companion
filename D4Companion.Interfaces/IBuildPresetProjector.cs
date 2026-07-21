using D4Companion.Entities;
using D4Companion.Entities.Canonical;

namespace D4Companion.Interfaces
{
    public interface IBuildPresetProjector
    {
        /// <summary>
        /// Projects one canonical variant into an AffixPreset. This is the only place
        /// canonical data becomes preset data; importers must not build presets directly.
        /// </summary>
        AffixPreset Project(CanonicalVariant variant, string presetName);
    }
}
