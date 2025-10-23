//using HarmonyLib;
//using LCVR.Assets;
//using LCVR.Patches;
//using LCVR.Player;
//using UnityEngine;
//using LCVR.Physics;
//using Object = UnityEngine.Object;
//using LCVR.Physics.Interactions.Car;

//namespace CruiserXL.Compatibility.SCVR.Physics.Interactions.Scanvan;

//public class ScanvanHonk : MonoBehaviour, VRInteractable
//{
//    public InteractableFlags Flags => InteractableFlags.BothHands | InteractableFlags.NotWhileHeld;

//    private InteractTrigger trigger;

//    public void Awake()
//    {
//        trigger = GetComponentInParent<InteractTrigger>();
//    }

//    public void OnColliderEnter(VRInteractor interactor)
//    {
//        if (!trigger.interactable)
//            return;

//        trigger.HoldInteractNotFilled();
//    }

//    public void OnColliderExit(VRInteractor interactor)
//    {
//        trigger.StopInteraction();
//    }

//    public bool OnButtonPress(VRInteractor interactor) { return false; }
//    public void OnButtonRelease(VRInteractor interactor) { }
//}

//[LCVRPatch(LCVRPatchTarget.Universal)]
//[HarmonyPatch]
//internal static class ScanvanHonkPatches
//{
//    [HarmonyPatch(typeof(CruiserXLController), nameof(CruiserXLController.Awake))]
//    [HarmonyPostfix]
//    private static void OnCarCreated(CruiserXLController __instance)
//    {
//        if (!LCVRCompatibility.modEnabled) return;

//        var honkTrigger = __instance.transform.Find("Meshes/ScanvanSteeringWheelContainer/SteeringWheel/Triggers/HonkHorn");

//        // Make sure VR interact trigger goes away
//        honkTrigger.gameObject.name = "HonkHornInteractable";

//        var honkInteractableObject = Object.Instantiate(AssetManager.Interactable, honkTrigger);
//        honkInteractableObject.AddComponent<ScanvanHonk>();
//        honkInteractableObject.transform.localScale = Vector3.one * 0.8f;
//    }
//}