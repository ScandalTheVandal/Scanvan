//using HarmonyLib;
//using LCVR.Assets;
//using LCVR.Patches;
//using LCVR.Player;
//using UnityEngine;
//using LCVR.Physics;
//using Object = UnityEngine.Object;

//namespace CruiserXL.Compatibility.SCVR.Physics.Interactions.Scanvan;

//public class StartScanvanIgnition : MonoBehaviour, VRInteractable
//{
//    internal CruiserXLController vehicle;
//    public Transform snapPoint;

//    public InteractableFlags Flags => InteractableFlags.RightHand | InteractableFlags.NotWhileHeld;
//    public float LastInteractedWith { get; private set; }

//    public void Awake()
//    {
//        //snapPoint = transform.parent.Find("CarKeyTurnedPos");
//    }

//    public void OnColliderEnter(VRInteractor interactor)
//    {
//        if (!vehicle.localPlayerInControl || vehicle.ignitionStarted)
//            return;

//        interactor.FingerCurler.Enabled = false;

//        if (vehicle.keyIsInIgnition)
//            return;

//        vehicle.keyIsInDriverHand = true;
//    }

//    public void OnColliderExit(VRInteractor interactor)
//    {
//        interactor.FingerCurler.Enabled = true;
//        vehicle.keyIsInDriverHand = false;
//    }

//    public bool OnButtonPress(VRInteractor interactor)
//    {
//        if (!vehicle.localPlayerInControl || vehicle.ignitionStarted)
//            return false;

//        interactor.SnapTo(snapPoint, new Vector3(1, 1, 0), new Vector3(0, 0, 140));

//        vehicle.StartTryCarIgnition();
//        LastInteractedWith = Time.realtimeSinceStartup;

//        return true;
//    }

//    public void OnButtonRelease(VRInteractor interactor)
//    {
//        interactor.SnapTo(null);

//        vehicle.CancelTryCarIgnition();
//    }
//}

//public class StopScanvanIgnition : MonoBehaviour, VRInteractable
//{
//    public InteractableFlags Flags => InteractableFlags.RightHand | InteractableFlags.NotWhileHeld;

//    internal StartScanvanIgnition startIgnition;
//    internal CruiserXLController vehicle;

//    public bool OnButtonPress(VRInteractor interactor)
//    {
//        if (!vehicle.localPlayerInControl)
//            return false;

//        if (Time.realtimeSinceStartup - startIgnition.LastInteractedWith < 1f)
//            return true;

//        vehicle.RemoveKeyFromIgnition();

//        return true;
//    }

//    public void OnColliderEnter(VRInteractor interactor) { }
//    public void OnColliderExit(VRInteractor interactor) { }
//    public void OnButtonRelease(VRInteractor interactor) { }
//}

//[LCVRPatch(LCVRPatchTarget.Universal)]
//[HarmonyPatch]
//internal static class ScanvanIgnitionPatches
//{
//    [HarmonyPatch(typeof(CruiserXLController), nameof(CruiserXLController.Awake))]
//    [HarmonyPostfix]
//    private static void OnCarCreated(CruiserXLController __instance)
//    {
//        var startIgnitionObj = __instance.transform.Find("Meshes/ScanvanKeyContainer/NonVRInteractions/StartIgnition");
//        var stopIgnitionObj = __instance.transform.Find("Meshes/ScanvanKeyContainer/NonVRInteractions/StopIgnition");
//        var startIgnitionInteractableObject = Object.Instantiate(AssetManager.Interactable, startIgnitionObj);
//        var stopIgnitionInteractableObject = Object.Instantiate(AssetManager.Interactable, stopIgnitionObj);

//        startIgnitionInteractableObject.transform.localScale = Vector3.one * 0.5f;
//        stopIgnitionInteractableObject.transform.localScale = Vector3.one * 0.5f;

//        var startTruckIgnition = startIgnitionInteractableObject.AddComponent<StartScanvanIgnition>();
//        var stopTruckIgnition = stopIgnitionInteractableObject.AddComponent<StopScanvanIgnition>();

//        startTruckIgnition.vehicle = __instance;
//        stopTruckIgnition.vehicle = __instance;
//        stopTruckIgnition.startIgnition = startTruckIgnition;
//        startTruckIgnition.snapPoint = __instance.ignitionTurnedPosition;
//    }
//}