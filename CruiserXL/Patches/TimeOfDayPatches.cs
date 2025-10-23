using CruiserXL.Utils;
using HarmonyLib;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(TimeOfDay))]
public static class TimeOfDayPatches
{
    // This is pretty messy, but all this is for is to allow
    // us to make our "rain colliders" on our truck kill rain,
    // effectively stopping rain from visually leaking into the
    // trucks cab or rear compartment, which was worse while the
    // truck was in motion.
    [HarmonyPatch("Awake")]
    [HarmonyPrefix]
    private static void Awake_Prefix(TimeOfDay __instance)
    {
        References.rainParticles = __instance.transform.Find("RainParticleContainer/Particle System").GetComponent<ParticleSystem>();
        References.rainHitParticles = __instance.transform.Find("RainParticleContainer/Particle System/RainHitParticle").GetComponent<ParticleSystem>();
        References.stormyRainParticles = __instance.transform.Find("StormyRainParticleContainer/Particle System").GetComponent<ParticleSystem>();
        References.stormyRainHitParticles = __instance.transform.Find("StormyRainParticleContainer/Particle System/RainHitParticle").GetComponent<ParticleSystem>();

        if (References.rainParticles == null || References.rainHitParticles == null || References.stormyRainParticles == null || References.stormyRainHitParticles == null)
            return;

        var rainParticleTrigger = References.rainParticles.trigger;
        var rainHitParticleTrigger = References.rainHitParticles.trigger;
        var stormyRainParticleTrigger = References.stormyRainParticles.trigger;
        var stormyRainHitParticleTrigger = References.stormyRainHitParticles.trigger;

        rainParticleTrigger.enabled = true;
        rainHitParticleTrigger.enabled = true;
        stormyRainParticleTrigger.enabled = true;
        stormyRainHitParticleTrigger.enabled = true;

        rainParticleTrigger.colliderQueryMode = ParticleSystemColliderQueryMode.One;
        rainHitParticleTrigger.colliderQueryMode = ParticleSystemColliderQueryMode.One;
        stormyRainParticleTrigger.colliderQueryMode = ParticleSystemColliderQueryMode.One;
        stormyRainHitParticleTrigger.colliderQueryMode = ParticleSystemColliderQueryMode.One;

        rainParticleTrigger.enter = ParticleSystemOverlapAction.Kill;
        rainParticleTrigger.exit = ParticleSystemOverlapAction.Kill;
        rainParticleTrigger.inside = ParticleSystemOverlapAction.Kill;

        rainHitParticleTrigger.enter = ParticleSystemOverlapAction.Kill;
        rainHitParticleTrigger.exit = ParticleSystemOverlapAction.Kill;
        rainHitParticleTrigger.inside = ParticleSystemOverlapAction.Kill;

        stormyRainParticleTrigger.enter = ParticleSystemOverlapAction.Kill;
        stormyRainParticleTrigger.exit = ParticleSystemOverlapAction.Kill;
        stormyRainParticleTrigger.inside = ParticleSystemOverlapAction.Kill;

        stormyRainHitParticleTrigger.enter = ParticleSystemOverlapAction.Kill;
        stormyRainHitParticleTrigger.exit = ParticleSystemOverlapAction.Kill;
        stormyRainHitParticleTrigger.inside = ParticleSystemOverlapAction.Kill;
    }
}