using HarmonyLib;
using CruiserXL.Events;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(RoundManager))]
public class RoundManagerPatches
{
    // Used for the radio static effect.
    [HarmonyPatch("SetToCurrentLevelWeather")]
    [HarmonyPostfix]
    static void SetToCurrentLevelWeather(ref SelectableLevel ___currentLevel)
    {
        if (___currentLevel.currentWeather == LevelWeatherType.Stormy)
        {
            WeatherEvents.StormStart();
        }
    }
}
