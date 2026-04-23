using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.Audio;

namespace ScanVan.Utils;

public static class References
{
    // optimisation
    internal static ItemDropship itemShip = null!;
    internal static CruiserXLController truckController = null!;
    internal static CadaverGrowthAI cadaverGrowthAI = null!;

    // fixes
    internal static PlayerControllerB lastDriver = null!;
    internal static AudioMixerGroup diageticSFXGroup = null!;

    // custom animations
    internal static RuntimeAnimatorController truckPlayerAnimator = null!;

    // misc
    internal static bool justDepartedIntoOrbit;
    internal static bool isDespawningProps;
}
