using GameNetcodeStuff;
using UnityEngine;

namespace CruiserXL.Utils;

public static class References
{
    internal static ParticleSystem rainParticles = null!;
    internal static ParticleSystem rainHitParticles = null!;

    internal static ParticleSystem stormyRainParticles = null!;
    internal static ParticleSystem stormyRainHitParticles = null!;

    internal static ParticleSystem.TriggerModule rainParticlesTrigger = default!;
    internal static ParticleSystem.TriggerModule rainHitParticlesTrigger = default!;

    internal static ParticleSystem.TriggerModule stormyRainParticlesTrigger = default!;
    internal static ParticleSystem.TriggerModule stormyRainHitParticlesTrigger = default!;

    internal static ItemDropship itemShip = null!;

    internal static CruiserXLController truckController = null!;
    internal static CruiserXLController currentVehicle = null!;

    internal static PlayerControllerB currentDriver = null!;
    internal static PlayerControllerB currentMiddlePassenger = null!;
    internal static PlayerControllerB currentPassenger = null!;

    internal static PlayerControllerB lastDriver = null!;

    internal static RuntimeAnimatorController originalPlayerAnimator = null!; // Local player
    internal static RuntimeAnimatorController truckPlayerAnimator = null!; // Local player
    internal static RuntimeAnimatorController truckOtherPlayerAnimator = null!; // Other players

    internal static bool radioFucked = false; // When shit hits the fan
}
