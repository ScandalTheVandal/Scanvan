using CruiserXL.Utils;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(StormyWeather))]
public static class StormyWeatherPatches
{
    // A little messy, and could be optimised, not even
    // sure if this works properly, but this is meant to
    // be a rare chance where if lightning strikes near a truck
    // it will set off the vehicles alarm, immobilizing the
    // vheicle temporarily, and playing a very loud audio
    // that also attracts dogs.
    [HarmonyPatch("PlayThunderEffects")]
    [HarmonyPostfix]
    private static void PlayThunderEffects_Postfix(StormyWeather __instance, Vector3 strikePosition, AudioSource audio)
    {
        if (!NetworkManager.Singleton.IsHost) return;

        if (((float)UnityEngine.Random.Range(9, 85)) < 17f)
        {
            CruiserXLController controller = References.truckController;
            if (controller == null)
                return;

            if (!controller.IsSpawned)
                return;

            if (Vector3.Distance(controller.transform.position, strikePosition) < 5f)
                return;

            if (controller.alarmDebounce)
                return;

            if (controller.ignitionStarted)
                return;

            controller.alarmDebounce = true;
            controller.TryBeginAlarm();
        }
    }
}
