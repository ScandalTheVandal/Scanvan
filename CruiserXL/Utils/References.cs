using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.Audio;

namespace CruiserXL.Utils;

public static class References
{
    // optimisation
    internal static ItemDropship itemShip = null!;
    internal static CruiserXLController truckController = null!;
    internal static BushWolfEnemy kidnapperFox = null!;

    // fixes
    internal static PlayerControllerB lastDriver = null!;
    internal static AudioMixerGroup diageticSFXGroup = null!;

    // custom animations
    internal static RuntimeAnimatorController truckPlayerAnimator = null!;
    internal static RuntimeAnimatorController truckOtherPlayerAnimator = null!;
}
