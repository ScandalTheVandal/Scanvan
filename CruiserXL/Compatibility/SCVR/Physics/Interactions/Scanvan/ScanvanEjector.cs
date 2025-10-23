//using HarmonyLib;
//using LCVR.Assets;
//using LCVR.Patches;
//using LCVR.Player;
//using LCVR.Physics;
//using UnityEngine;

//namespace CruiserXL.Compatibility.SCVR.Physics.Interactions.Scanvan;

//internal class ScanvanEjector : MonoBehaviour, VRInteractable
//{
//    private InteractTrigger trigger;
//    internal ScanvanEjectButtonGlass glassInteractable;
//    private float lastTriggerTime;

//    public InteractableFlags Flags => InteractableFlags.BothHands;

//    private void Awake()
//    {
//        trigger = GetComponentInParent<InteractTrigger>();
//    }

//    public void OnColliderEnter(VRInteractor _)
//    {
//        if (!glassInteractable.CanPressButton || !trigger.interactable ||
//            Time.realtimeSinceStartup - lastTriggerTime < 2f)
//            return;

//        trigger.Interact(VRSession.Instance.LocalPlayer.PlayerController.transform);
//        lastTriggerTime = Time.realtimeSinceStartup;
//    }

//    public bool OnButtonPress(VRInteractor _) { return false; }
//    public void OnButtonRelease(VRInteractor _) { }
//    public void OnColliderExit(VRInteractor _) { }
//}

//internal class ScanvanEjectButtonGlass : MonoBehaviour, VRInteractable
//{
//    private AnimatedObjectTrigger trigger;
//    private float lastTriggerTime;

//    // Check if glass is open and has been opened for at least 200ms to prevent accidental trigger
//    public bool CanPressButton => trigger.boolValue && Time.realtimeSinceStartup - lastTriggerTime > 0.2f;
//    public InteractableFlags Flags => InteractableFlags.BothHands;

//    private void Awake()
//    {
//        trigger = GetComponentInParent<AnimatedObjectTrigger>();
//    }

//    public void OnColliderEnter(VRInteractor _)
//    {
//        // Require at least 1s cooldown on glass
//        if (Time.realtimeSinceStartup - lastTriggerTime < 1)
//            return;

//        lastTriggerTime = Time.realtimeSinceStartup;
//        trigger.TriggerAnimation(VRSession.Instance.LocalPlayer.PlayerController);
//    }

//    public bool OnButtonPress(VRInteractor _) { return false; }
//    public void OnButtonRelease(VRInteractor _) { }
//    public void OnColliderExit(VRInteractor _) { }
//}

//[LCVRPatch(LCVRPatchTarget.Universal)]
//[HarmonyPatch]
//internal static class ScanvanEjectButtonPatches
//{
//    [HarmonyPatch(typeof(CruiserXLController), nameof(CruiserXLController.Awake))]
//    [HarmonyPostfix]
//    private static void OnCarCreated(CruiserXLController __instance)
//    {
//        var container = __instance.transform.Find("Triggers/NonVRTriggers/Cabin/EjectorSeat/ButtonAnimContainer");
//        var glass = container.transform.Find("ButtonGlass").gameObject;
//        var button = __instance.transform.Find("Triggers/NonVRTriggers/Cabin/EjectorSeat/ButtonAnim/RedButton").gameObject;

//        glass.name = "EjectButtonGlass";
//        button.name = "EjectRedButton";

//        var buttonInteractableObject = Object.Instantiate(AssetManager.Interactable, button.transform);
//        var glassInteractableObject = Object.Instantiate(AssetManager.Interactable, glass.transform);

//        buttonInteractableObject.transform.localPosition = new Vector3(0, -0.12f, 1.2f);
//        buttonInteractableObject.transform.localScale = new Vector3(1.1f, 1.2f, 2);

//        glassInteractableObject.transform.localPosition = new Vector3(-1, 0, 1);
//        glassInteractableObject.transform.localEulerAngles = new Vector3(0, 315, 0);
//        glassInteractableObject.transform.localScale = new Vector3(3, 1.7f, 1.5f);

//        var buttonInteractable = buttonInteractableObject.AddComponent<ScanvanEjector>();
//        var glassInteractable = glassInteractableObject.AddComponent<ScanvanEjectButtonGlass>();

//        buttonInteractable.glassInteractable = glassInteractable;
//    }
//}