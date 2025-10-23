//using System.IO;
//using HarmonyLib;
//using LCVR.Assets;
//using LCVR.Networking;
//using LCVR.Patches;
//using LCVR.Player;
//using UnityEngine;
//using LCVR.Physics;
//using Object = UnityEngine.Object;

//namespace CruiserXL.Compatibility.SCVR.Physics.Interactions.Scanvan;

//public class ScanvanShifter : MonoBehaviour, VRInteractable
//{
//    private static readonly int CarAnimation = Animator.StringToHash("SA_CarAnim");
//    public InteractableFlags Flags => InteractableFlags.BothHands | InteractableFlags.NotWhileHeld;

//    public CruiserXLController vehicle;
//    public Channel channel;
//    public Transform container;

//    public const float PARK_POSITION = 1.7551f;
//    public const float REVERSE_POSITION = 1.66f;
//    public const float NEUTRAL_POSITION = 1.5463f;
//    public const float DRIVE_POSITION = 1.4611f;

//    private const int HANDS_ON_LEGS = 0;
//    private const int HANDS_ON_WHEEL = 1;
//    private const int HAND_NEAR_GEAR = 4;
//    private const int HAND_ON_GEAR = 5;

//    public Vector3 handSnapOffset = new Vector3(0f, -0.02f, 1.1f);
//    public Vector3 handSnapToPos = new Vector3(0f, 0f, 0f);
//    public Vector3 rightHandRotation = new Vector3(200f, 180f, 0f);
//    public Vector3 leftHandRotation = new Vector3(-150f, 180f, 0f);

//    public Vector3 nonLocalRightHandSnapToPos = new Vector3(-0.0001f, 0.001f, 0.002f);

//    public Vector3 nonLocalLeftHandSnapToPos = new Vector3(-0.01f, -0.006f, 0f);

//    public Vector3 nonLocalRightHandRotation = new Vector3(200f, 180f, 0f);

//    public Vector3 nonLocalLeftHandRotation = new Vector3(-25f, 45f, 120f);

//    private bool isHeld;
//    private bool isHeldByLocal;
//    private Transform localHand;

//    public void Awake()
//    {
//        vehicle = GetComponentInParent<CruiserXLController>();
//        channel = NetworkSystem.Instance.CreateChannel(ChannelType.VehicleGearStick, vehicle.NetworkObjectId);

//        container = transform.parent.parent.parent.parent;

//        channel.OnPacketReceived += OnPacketReceived;
//    }

//    public void Update()
//    {
//        if (!isHeldByLocal || !localHand)
//            return;

//        var localPosition = container.InverseTransformPoint(localHand.position).z + 0.1f;

//        Debug.LogError(localPosition);

//        if (vehicle.autoGear != TruckGearShift.Park && Mathf.Abs(localPosition - PARK_POSITION) < 0.05f)
//        {
//            vehicle.ShiftToGearAndSync((int)TruckGearShift.Park);
//        }
//        else if (vehicle.autoGear != TruckGearShift.Reverse && Mathf.Abs(localPosition - REVERSE_POSITION) < 0.05f)
//        {
//            vehicle.ShiftToGearAndSync((int)TruckGearShift.Reverse);
//        }
//        else if (vehicle.autoGear != TruckGearShift.Neutral && Mathf.Abs(localPosition - NEUTRAL_POSITION) < 0.05f)
//        {
//            vehicle.ShiftToGearAndSync((int)TruckGearShift.Neutral);
//        }
//        else if (vehicle.autoGear != TruckGearShift.Drive && Mathf.Abs(localPosition - DRIVE_POSITION) < 0.05f)
//        {
//            vehicle.ShiftToGearAndSync((int)TruckGearShift.Drive);
//        }
//    }

//    public void OnDestroy()
//    {
//        channel.Dispose();
//    }

//    public void OnColliderEnter(VRInteractor interactor)
//    {
//        if (vehicle.currentDriver != VRSession.Instance.LocalPlayer.PlayerController)
//            return;

//        vehicle.currentDriver.playerBodyAnimator.SetInteger(CarAnimation, HAND_NEAR_GEAR);
//    }

//    public void OnColliderExit(VRInteractor interactor)
//    {
//        if (vehicle.currentDriver != VRSession.Instance.LocalPlayer.PlayerController)
//            return;

//        vehicle.currentDriver.playerBodyAnimator.SetInteger(CarAnimation,
//            vehicle.ignitionStarted ? HANDS_ON_WHEEL : HANDS_ON_LEGS);
//    }

//    public bool OnButtonPress(VRInteractor interactor)
//    {
//        if (isHeld)
//            return false;

//        isHeld = true;
//        isHeldByLocal = true;
//        localHand = interactor.IsRightHand
//            ? VRSession.Instance.LocalPlayer.RightHandVRTarget
//            : VRSession.Instance.LocalPlayer.LeftHandVRTarget;

//        Vector3 rotation = interactor.IsRightHand ? rightHandRotation : leftHandRotation;
//        interactor.SnapTo(transform.parent.parent, handSnapOffset, rotation);

//        interactor.FingerCurler.ForceFist(true);

//        channel.SendPacket([(byte)GearStickCommand.GrabStick, interactor.IsRightHand ? (byte)1 : (byte)0]);

//        if (vehicle.currentDriver == VRSession.Instance.LocalPlayer.PlayerController)
//            vehicle.currentDriver.playerBodyAnimator.SetInteger(CarAnimation, HAND_ON_GEAR);

//        return true;
//    }

//    public void OnButtonRelease(VRInteractor interactor)
//    {
//        if (!isHeldByLocal)
//            return;

//        isHeld = false;
//        isHeldByLocal = false;
//        localHand = null;

//        interactor.SnapTo(null);
//        interactor.FingerCurler.ForceFist(false);

//        channel.SendPacket([(byte)GearStickCommand.ReleaseStick, interactor.IsRightHand ? (byte)1 : (byte)0]);

//        if (vehicle.currentDriver == VRSession.Instance.LocalPlayer.PlayerController)
//            vehicle.currentDriver.playerBodyAnimator.SetInteger(CarAnimation, HAND_NEAR_GEAR);
//    }

//    private void OnPacketReceived(ushort sender, BinaryReader reader)
//    {
//        switch ((GearStickCommand)reader.ReadByte())
//        {
//            case GearStickCommand.GrabStick:
//                {
//                    // Discard packet if already held
//                    if (isHeld)
//                        break;

//                    // Check if player exists
//                    if (!NetworkSystem.Instance.TryGetPlayer(sender, out var player))
//                        break;

//                    isHeld = true;


//                    var isRightHand = reader.ReadBoolean();
//                    if (isRightHand)
//                    {
//                        player.RightFingerCurler.ForceFist(true);
//                        player.SnapRightHandTo(transform.parent, nonLocalRightHandSnapToPos,
//                            nonLocalRightHandRotation);
//                    }
//                    else
//                    {
//                        player.LeftFingerCurler.ForceFist(true);
//                        player.SnapLeftHandTo(transform.parent, nonLocalLeftHandSnapToPos,
//                            nonLocalLeftHandRotation);
//                    }
//                }
//                break;

//            case GearStickCommand.ReleaseStick:
//                {
//                    // Discard packet if not held by other
//                    if (!isHeld || isHeldByLocal)
//                        break;

//                    // Check if player exists
//                    if (!NetworkSystem.Instance.TryGetPlayer(sender, out var player))
//                        break;

//                    isHeld = false;

//                    var isRightHand = reader.ReadBoolean();
//                    if (isRightHand)
//                    {
//                        player.RightFingerCurler.ForceFist(false);
//                        player.SnapRightHandTo(null);
//                    }
//                    else
//                    {
//                        player.LeftFingerCurler.ForceFist(false);
//                        player.SnapLeftHandTo(null);
//                    }
//                }
//                break;
//        }
//    }

//    private enum GearStickCommand : byte
//    {
//        GrabStick,
//        ReleaseStick
//    }
//}

//[LCVRPatch(LCVRPatchTarget.Universal)]
//[HarmonyPatch]
//internal static class ScanvanShifterPatches
//{
//    [HarmonyPatch(typeof(CruiserXLController), nameof(CruiserXLController.Awake))]
//    [HarmonyPostfix]
//    private static void OnCarCreated(CruiserXLController __instance)
//    {
//        var gearStickObj = __instance.transform.Find("Meshes/ScanvanGearStickContainer/GearStickContainer/GearStickPivot/GearStickMesh");
//        var gearStickInteractable = Object.Instantiate(AssetManager.Interactable, gearStickObj);

//        gearStickInteractable.transform.localPosition = new Vector3(0, 0, 0.0018f);
//        gearStickInteractable.transform.localScale = new Vector3(0.001f, 0.001f, 0.002f);

//        gearStickInteractable.AddComponent<ScanvanShifter>();
//    }
//}