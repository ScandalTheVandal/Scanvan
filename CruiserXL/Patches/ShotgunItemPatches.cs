using CruiserXL.Utils;
using HarmonyLib;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(ShotgunItem))]
public class ShotgunItemPatches
{
    [HarmonyPatch("ShootGun")]
    [HarmonyPrefix]
    private static void ShootGun_Postfix(ShotgunItem __instance, Vector3 shotgunPosition, Vector3 shotgunForward)
    {
        //RaycastHit[] vehicleColliders = new RaycastHit[10];
        //Ray ray = new Ray(shotgunPosition - shotgunForward * 10f, shotgunForward);

        //int sphereCast = Physics.SphereCastNonAlloc(ray, 5f, vehicleColliders, 15f, StartOfRound.Instance.collidersAndRoomMaskAndDefault);

        //for (int i = 0; i < sphereCast; i++)
        //{
        //    if (__instance.playerHeldBy != null && vehicleColliders[i].transform.TryGetComponent<CruiserXLController>(out CruiserXLController vehicle))
        //    {
        //        if (vehicle.currentDriver == __instance.playerHeldBy || vehicle.currentMiddlePassenger == __instance.playerHeldBy || vehicle.currentPassenger == __instance.playerHeldBy)
        //        {
        //            continue;
        //        }
        //    }
        //    if (!Physics.Linecast(shotgunPosition, vehicleColliders[i].point, out RaycastHit hitInfo, StartOfRound.Instance.collidersAndRoomMaskAndDefault) && 
        //        vehicleColliders[i].collider.TryGetComponent<CruiserXLController>(out CruiserXLController cdc))
        //    {
        //        float dist = Vector3.Distance(shotgunPosition, vehicleColliders[i].point);
        //        int force = ((dist < 3.7f) ? 5 : ((!(dist < 6f)) ? 2 : 3));

        //        cdc.DealPermanentDamage(force);
        //        cdc.PushTruckServerRpc(cdc.transform.position - (shotgunForward * 5), shotgunForward);
        //    }
        //}
    }
}
