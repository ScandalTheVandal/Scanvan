//using System.Linq;
//using HarmonyLib;
//using LCVR.Assets;
//using LCVR.Patches;
//using LCVR.Player;
//using LCVR.Physics.Interactions.Car;
//using LCVR.Physics;
//using UnityEngine;
//using Object = UnityEngine.Object;

//namespace CruiserXL.Compatibility.SCVR.Physics.Interactions.Scanvan;

//public class ScanvanButton : MonoBehaviour, VRInteractable
//{
//    public InteractableFlags Flags => InteractableFlags.BothHands;

//    private InteractTrigger trigger;
//    //private AudioClip buttonPressSfx;
//    //private AudioSource audioSource;
//    private float lastTriggerTime;

//    public ScanvanButton[] otherButtons = [];

//    private bool CanInteract => trigger.interactable && Time.realtimeSinceStartup - lastTriggerTime > 0.25f;

//    public void Awake()
//    {
//        trigger = GetComponentInParent<InteractTrigger>();
//        //buttonPressSfx = ShipBuildModeManager.Instance.beginPlacementSFX;
//        //audioSource = gameObject.AddComponent<AudioSource>();
//    }

//    public void OnColliderEnter(VRInteractor interactor)
//    {
//        if (!CanInteract && otherButtons.All(btn => btn.CanInteract))
//            return;

//        lastTriggerTime = Time.realtimeSinceStartup;
//        trigger.onInteract?.Invoke(VRSession.Instance.LocalPlayer.PlayerController);
//        //audioSource.PlayOneShot(buttonPressSfx);
//    }

//    public void OnColliderExit(VRInteractor _) { }
//    public bool OnButtonPress(VRInteractor _) { return false; }
//    public void OnButtonRelease(VRInteractor _) { }
//}

//[LCVRPatch(LCVRPatchTarget.Universal)]
//[HarmonyPatch]
//internal static class ScanvanButtonPatches
//{
//    [HarmonyPatch(typeof(CruiserXLController), nameof(CruiserXLController.Awake))]
//    [HarmonyPostfix]
//    private static void OnCarCreated(CruiserXLController __instance)
//    {
//        var wipers = __instance.transform.Find("Meshes/XLBody/RightStockAnimContainer/RightStock/ToggleWiper");
//        var cabinWindow = __instance.transform.Find("Meshes/CabinWindowContainer/CabinWindowAnimContainer/CabinWindowMesh/ToggleCabWindow");
//        var headlights = __instance.transform.Find("Triggers/NonVRTriggers/Cabin/VerticalColumn/ToggleHeadlights");
//        var tune = __instance.transform.Find("Triggers/NonVRTriggers/Cabin/Radio/SeekRadioL");
//        var tune1 = __instance.transform.Find("Triggers/NonVRTriggers/Cabin/Radio/SeekRadioR");
//        var toggleRadio = __instance.transform.Find("Triggers/NonVRTriggers/Cabin/Radio/ToggleRadio");
//        var cabLight = __instance.transform.Find("Triggers/NonVRTriggers/Cabin/VerticalColumn/ToggleCabLight");
//        var turboSwitch = __instance.transform.Find("Triggers/NonVRTriggers/Cabin/VerticalColumn/ToggleOverdrive");


//        // Make sure VR interact trigger goes away
//        wipers.gameObject.name = "CarButton";
//        cabinWindow.gameObject.name = "CarButton";
//        headlights.gameObject.name = "CarButton";
//        tune.gameObject.name = "CarButton";
//        tune1.gameObject.name = "CarButton";
//        toggleRadio.gameObject.name = "CarButton";
//        cabLight.gameObject.name = "CarButton";
//        turboSwitch.gameObject.name = "CarButton";

//        var wipersInteract = Object.Instantiate(AssetManager.Interactable, wipers);
//        var cabinWindowInteract = Object.Instantiate(AssetManager.Interactable, cabinWindow);
//        var headlightsInteract = Object.Instantiate(AssetManager.Interactable, headlights);
//        var tuneInteract = Object.Instantiate(AssetManager.Interactable, tune);
//        var tune1Interact = Object.Instantiate(AssetManager.Interactable, tune1);
//        var toggleRadioInteract = Object.Instantiate(AssetManager.Interactable, toggleRadio);
//        var toggleCabLightInteract = Object.Instantiate(AssetManager.Interactable, cabLight);
//        var toggleTurboSwitchInteract = Object.Instantiate(AssetManager.Interactable, turboSwitch);


//        // Buncha transforms
//        wipersInteract.transform.localPosition = new Vector3(-0.1f, 0, 0);
//        wipersInteract.transform.localScale = Vector3.one * 0.5f;

//        cabinWindowInteract.transform.localPosition = new Vector3(-0.1f, 0, 0);
//        cabinWindowInteract.transform.localScale = Vector3.one * 0.5f;

//        headlightsInteract.transform.localPosition = new Vector3(0.2f, 0, 0);
//        headlightsInteract.transform.localScale = Vector3.one * 0.5f;

//        tuneInteract.transform.localPosition = new Vector3(0.1f, 0.2f, 0.2f);
//        tuneInteract.transform.localScale = Vector3.one * 0.5f;

//        tune1Interact.transform.localPosition = new Vector3(0.1f, 0.2f, 0.2f);
//        tune1Interact.transform.localScale = Vector3.one * 0.5f;

//        toggleRadioInteract.transform.localPosition = new Vector3(0.1f, -0.2f, 0.2f);
//        toggleRadioInteract.transform.localScale = Vector3.one * 0.5f;

//        toggleCabLightInteract.transform.localPosition = new Vector3(0.1f, 0f, 0f);
//        toggleCabLightInteract.transform.localScale = Vector3.one * 0.5f;

//        toggleTurboSwitchInteract.transform.localPosition = new Vector3(0.1f, 0f, 0f);
//        toggleTurboSwitchInteract.transform.localScale = Vector3.one * 0.5f;

//        var wipersButton = wipersInteract.AddComponent<CarButton>();
//        var cabinButton = cabinWindowInteract.AddComponent<CarButton>();
//        var tuneButton = tuneInteract.AddComponent<CarButton>();
//        var tune1Button = tune1Interact.AddComponent<CarButton>();
//        var toggleButton = toggleRadioInteract.AddComponent<CarButton>();
//        var cabLightButton = toggleCabLightInteract.AddComponent<CarButton>();
//        var turboSwitchButton = toggleTurboSwitchInteract.AddComponent<CarButton>();

//        var headlightButton = headlightsInteract.AddComponent<CarButton>();

//        wipersButton.otherButtons = [cabinButton];

//        cabinButton.otherButtons = [wipersButton];

//        tuneButton.otherButtons = [toggleButton];
//        tune1Button.otherButtons = [toggleButton];

//        toggleButton.otherButtons = [tuneButton];

//        cabLightButton.otherButtons = [turboSwitchButton];
//        turboSwitchButton.otherButtons = [cabLightButton];
//    }
//}