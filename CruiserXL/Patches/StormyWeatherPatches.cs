using CruiserXL.Utils;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(StormyWeather))]
public static class StormyWeatherPatches
{
    [HarmonyPatch(nameof(StormyWeather.PlayThunderEffects))]
    [HarmonyPostfix]
    private static void PlayThunderEffects_Postfix(StormyWeather __instance, Vector3 strikePosition, AudioSource audio)
    {
        if (!NetworkManager.Singleton.IsHost)
            return;

        if (References.truckController == null)
            return;
        CruiserXLController controller = References.truckController;

        if (!controller.IsSpawned ||
            !controller.hasBeenSpawned ||
            controller.alarmDebounce ||
            controller.ignitionStarted)
            return;

        if (Vector3.Distance(controller.transform.position, strikePosition) > 5f)
            return;

        if ((float)UnityEngine.Random.Range(9, 85) > 17f)
            return;

        controller.alarmDebounce = true;
        controller.TryBeginAlarm();
    }
}
