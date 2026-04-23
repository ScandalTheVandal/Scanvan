using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.InputSystem;

namespace ScanVan.Utils;

/// <summary>
///  Available from BetterVehicleControls, licensed under MIT License.
///  Source: https://github.com/1A3Dev/LC-BetterVehicleControls/blob/master/source/Plugin.cs
/// </summary>

internal class VehicleControls : LcInputActions
{
    [InputAction(KeyboardControl.W, Name = "Gas Pedal", GamepadControl = GamepadControl.Unbound, ActionType = InputActionType.Value)]
    public InputAction GasPedalKey { get; set; } = null!;


    [InputAction(KeyboardControl.Unbound, Name = "(Arcade) Reverse Pedal", GamepadControl = GamepadControl.Unbound, ActionType = InputActionType.Value)]
    public InputAction ReversePedalKey { get; set; } = null!;


    [InputAction(KeyboardControl.A, Name = "Steer Left", GamepadControl = GamepadControl.Unbound, ActionType = InputActionType.Value)]
    public InputAction SteerLeftKey { get; set; } = null!;


    [InputAction(KeyboardControl.S, Name = "Brake Pedal", GamepadControl = GamepadControl.Unbound, ActionType = InputActionType.Value)]
    public InputAction BrakePedalKey { get; set; } = null!;


    [InputAction(KeyboardControl.D, Name = "Steer Right", GamepadControl = GamepadControl.Unbound, ActionType = InputActionType.Value)]
    public InputAction SteerRightKey { get; set; } = null!;


    [InputAction(KeyboardControl.LeftShift, Name = "Clutch Pedal", GamepadControl = GamepadControl.Unbound, ActionType = InputActionType.Value)]
    public InputAction ClutchPedalKey { get; set; } = null!;


    [InputAction(MouseControl.LeftButton, Name = "Switch Ignition Action", GamepadControl = GamepadControl.Unbound)]
    public InputAction SwitchIgnitionKey { get; set; } = null!;


    [InputAction(KeyboardControl.Space, Name = "Jump", GamepadControl = GamepadControl.Unbound)]
    public InputAction JumpKey { get; set; } = null!;


    [InputAction(MouseControl.ScrollUp, Name = "Shift Gear Up", GamepadControl = GamepadControl.Unbound)]
    public InputAction GearShiftForwardKey { get; set; } = null!;


    [InputAction(MouseControl.ScrollDown, Name = "Shift Gear Down", GamepadControl = GamepadControl.Unbound)]
    public InputAction GearShiftBackwardKey { get; set; } = null!;


    [InputAction(KeyboardControl.F, Name = "Headlamps", GamepadControl = GamepadControl.Unbound)]
    public InputAction ToggleHeadlightsKey { get; set; } = null!;


    [InputAction(KeyboardControl.H, Name = "Horn", GamepadControl = GamepadControl.Unbound)]
    public InputAction ActivateHornKey { get; set; } = null!;


    [InputAction(KeyboardControl.None, Name = "Wipers", GamepadControl = GamepadControl.Unbound)]
    public InputAction ToggleWipersKey { get; set; } = null!;


    [InputAction(KeyboardControl.None, Name = "Magnet")]
    public InputAction ToggleMagnetKey { get; set; } = null!;
}
