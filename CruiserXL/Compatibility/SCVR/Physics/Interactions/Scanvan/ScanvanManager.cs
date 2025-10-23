//using System.Collections.Generic;

//namespace CruiserXL.Compatibility.SCVR.Physics.Interactions.Scanvan;

//public class ScanvanManager
//{
//    private readonly Dictionary<CruiserXLController, ScanvanSteeringWheel> wheelCache = [];

//    public ScanvanSteeringWheel FindWheelForVehicle(CruiserXLController vehicle)
//    {
//        if (wheelCache.TryGetValue(vehicle, out var wheel))
//            return wheel;

//        wheel = vehicle.GetComponentInChildren<ScanvanSteeringWheel>();

//        if (!wheel)
//            return null;

//        wheelCache[vehicle] = wheel;
//        return wheel;
//    }

//    public void OnCarDestroyed(CruiserXLController vehicle)
//    {
//        wheelCache.Remove(vehicle);
//    }
//}