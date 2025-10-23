using HarmonyLib;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(VehicleController))]
public static class VehicleControllerPatches
{
    // Thank you MattyMatty, and DiFFoZ over at the Lethal
    // Company Modding Discord for helping me with this,
    // these are basically "re-direct" functions, since
    // we cannot override these functions as they're not
    // marked as virtual, this sort of mimics that by 
    // cancelling out the original function, and if it's
    // our vehicle, we call the function we want to call
    // on that instead, pretty simple stuff!
    [HarmonyPatch("AddEngineOil")]
    [HarmonyPrefix]
    static bool AddEngineOil_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            //somebody else has redirected the function ignore the call
            return false;

        if (__instance is not CruiserXLController vehicle)
            //not us run the original code
            return true;

        //our class run our code, and skip original.
        vehicle.AddEngineOil();
        return false;
    }

    [HarmonyPatch("AddTurboBoost")]
    [HarmonyPrefix]
    static bool AddTurboBoost_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.AddTurboBoost();
        return false;
    }

    [HarmonyPatch("StartMagneting")]
    [HarmonyPrefix]
    static bool StartMagneting_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.StartMagneting();
        return false;
    }

    [HarmonyPatch("CollectItemsInTruck")]
    [HarmonyPrefix]
    static bool CollectItemsInTruck_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.CollectItemsInTruck();
        return false;
    }


    [HarmonyPatch("DestroyCar")]
    [HarmonyPrefix]
    static bool DestroyCar_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.DestroyCar();
        return false;
    }

    /// <summary>
    ///  Available from EnemySoundFixes, licensed under GNU General Public License.
    ///  Source: https://github.com/ButteryStancakes/EnemySoundFixes/blob/master/Patches/CruiserPatches.cs
    /// </summary>
    [HarmonyPatch("SetVehicleAudioProperties")]
    [HarmonyPrefix]
    static void SetVehicleAudioProperties_Prefix(VehicleController __instance, AudioSource audio, ref bool audioActive)
    {
        if (audioActive && ((audio == __instance.extremeStressAudio && __instance.magnetedToShip) || ((audio == __instance.rollingAudio || audio == __instance.skiddingAudio) && (__instance.magnetedToShip || (!__instance.FrontLeftWheel.isGrounded && !__instance.FrontRightWheel.isGrounded && !__instance.BackLeftWheel.isGrounded && !__instance.BackRightWheel.isGrounded)))))
            audioActive = false;
    }

    // Mods such as BioDiversity that add the Ogopogo 
    // lake monster, have specific code to grab players
    // out of vehicles seats, however, we need to call
    // our function to leave the vehicle seat, or else
    // shit breaks, like, really badly.
    [HarmonyPatch("ExitDriverSideSeat")]
    [HarmonyPrefix]
    static bool ExitDriverSideSeat_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.ExitDriverSideSeat();
        return false;
    }

    [HarmonyPatch("ExitPassengerSideSeat")]
    [HarmonyPrefix]
    static bool ExitPassengerSideSeat_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.ExitPassengerSideSeat();
        return false;
    }
}