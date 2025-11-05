using HarmonyLib;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(TerrainObstacleTrigger))]
public static class TerrainObstacleTriggerPatches
{
    [HarmonyPatch("OnTriggerEnter")]
    [HarmonyPrefix]
    static bool OnTriggerEnter_Prefix(TerrainObstacleTrigger __instance, Collider other, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        CruiserXLController controller = other.GetComponent<CruiserXLController>();
        if (controller == null)
            return true;

        if (controller.IsOwner && controller.averageVelocity.magnitude > 5f) //&& Vector3.Angle(controller.averageVelocity, __instance.transform.position - controller.mainRigidbody.position) < 80f
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