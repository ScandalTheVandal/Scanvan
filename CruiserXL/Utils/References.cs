using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.Audio;

namespace CruiserXL.Utils;

public static class References
{
    // weather refs
    internal static ParticleSystem rainParticles = null!;
    internal static ParticleSystem rainHitParticles = null!;

    internal static ParticleSystem stormyRainParticles = null!;
    internal static ParticleSystem stormyRainHitParticles = null!;

    internal static ParticleSystem.TriggerModule rainParticlesTrigger = default!;
    internal static ParticleSystem.TriggerModule rainHitParticlesTrigger = default!;

    internal static ParticleSystem.TriggerModule stormyRainParticlesTrigger = default!;
    internal static ParticleSystem.TriggerModule stormyRainHitParticlesTrigger = default!;

    // optimisation
    internal static ItemDropship itemShip = null!;
    internal static CruiserXLController truckController = null!;

    // fixes
    internal static PlayerControllerB lastDriver = null!;
    internal static AudioMixerGroup diageticSFXGroup = null!;

    // custom animations
    internal static RuntimeAnimatorController truckPlayerAnimator = null!;
    internal static RuntimeAnimatorController truckOtherPlayerAnimator = null!;
}
