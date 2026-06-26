using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ScanVan.Utils;

public static class References
{
    // optimisation
    internal static ItemDropship itemShip = null!;
    internal static CruiserXLController vanController = null!;
    internal static CadaverGrowthAI cadaverGrowthAI = null!;
    internal static HashSet<GrabbableObject> itemsInTruck = new HashSet<GrabbableObject>();

    // fixes
    internal static PlayerControllerB lastDriver = null!;
}
