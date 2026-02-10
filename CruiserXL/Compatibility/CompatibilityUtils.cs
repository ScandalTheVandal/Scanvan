using BepInEx.Bootstrap;
using CruiserXL.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using VoxxWeatherPlugin.Patches;

namespace CruiserXL.Compatibility;

/// <summary>
///  Available from BrutalCompanyMinus, licensed under MIT licence.
///  Source: https://github.com/Sylkadi/BrutalCompanyMinus

///  Available from BrutalCompanyMinusExtraReborn, licensed under GNU General Public License.
///  Source: https://github.com/TheSoftDiamond/BrutalCompanyMinusExtraReborn
/// </summary>

[HarmonyPatch]
public class CompatibilityUtils
{
    internal static bool lethalElementsPresent = false;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PreInitSceneScript), "Awake")]
    private static void OnGameLoad()
    {   
        lethalElementsPresent = IsModInstalled("voxx.LethalElementsPlugin", "Lethal Elements detected!");
    }

    public static bool IsModInstalled(string name, string logMessage = "")
    {
        bool isPresent = Chainloader.PluginInfos.ContainsKey(name);
        if (isPresent)
        {
            Plugin.Logger.LogDebug($"{name} is installed. {logMessage}");
        }
        return isPresent;
    }
}
