//using LethalCompanyInputUtils.Api;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using UnityEngine.InputSystem;

//namespace ScanVan.Utils
//{
//    public class UserVehicleControls : LcInputActions
//    {
//        public static readonly UserVehicleControls Instance = new();

//        public InputAction SteerVehicle => Asset["steerVehicle"];

//        public override void CreateInputActions(in InputActionMapBuilder builder)
//        {
//            var steerAction = new InputAction("steerVehicle", InputActionType.Value);
//            steerAction.AddCompositeBinding("1DAxis") 
//                .With("Positive", "<Keyboard>/D") 
//                .With("Negative", "<Keyboard>/A");
//            steerAction.AddBinding("<Gamepad>/leftStick/x");
//            builder.WithAction(steerAction);
//        }
//    }
//}
