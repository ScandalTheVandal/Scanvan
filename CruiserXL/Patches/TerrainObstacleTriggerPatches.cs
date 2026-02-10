using CruiserXL.Utils;
using HarmonyLib;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(TerrainObstacleTrigger))]
public static class TerrainObstacleTriggerPatches
{
    [HarmonyPatch(nameof(TerrainObstacleTrigger.OnTriggerEnter))]
    [HarmonyPrefix]
    static bool OnTriggerEnter_Prefix(TerrainObstacleTrigger __instance, Collider other, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (References.truckController == null)
            return true;

        CruiserXLController controller = other.GetComponent<CruiserXLController>();
        if (controller == null)
            return true;

        // restore functionality for trees to damage the truck, while accounting for snowmen,
        // since they're breakable as of v70, but we do not want them to damage the truck as vanilla never accounts for this

        // may add the angle check back in for consistency
        if (controller.IsOwner && controller.averageVelocity.magnitude >= 5f) //&& Vector3.Angle(controller.averageVelocity, __instance.transform.position - controller.mainRigidbody.position) < 80f
        {
            RoundManager.Instance.DestroyTreeOnLocalClient(__instance.transform.position);
            bool isObjectATree = __instance.transform.parent != null &&
                __instance.transform.parent.CompareTag("Wood");
            controller.CarReactToObstacle(
                controller.mainRigidbody.position - __instance.transform.position,
                __instance.transform.position,
                Vector3.zero,
                CarObstacleType.Object,
                1f,
                null!,
                isObjectATree);
        }
        return false;
    }
}