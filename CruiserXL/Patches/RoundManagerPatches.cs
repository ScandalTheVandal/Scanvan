using HarmonyLib;
using CruiserXL.Events;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(RoundManager))]
public class RoundManagerPatches
{
    /// <summary>
    ///  Available from RadioFurniture, licensed under GNU General Public License.
    ///  Source: https://github.com/legoandmars/RadioFurniture/tree/master/RadioFurniture
    /// </summary>
    [HarmonyPatch(nameof(RoundManager.SetToCurrentLevelWeather))]
    [HarmonyPostfix]
    static void SetToCurrentLevelWeather_Postfix(ref SelectableLevel ___currentLevel)
    {
        if (___currentLevel.currentWeather == LevelWeatherType.Stormy)
        {
            WeatherEvents.StormStart();
        }
    }
}
