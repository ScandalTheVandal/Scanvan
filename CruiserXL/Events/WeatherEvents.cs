using System;
using UnityEngine;

namespace CruiserXL.Events
{
    public static class WeatherEvents
    {
        public static event Action OnStormStarted = null!;
        public static event Action OnStormEnded = null!;

        public static void StormStart()
        {
            OnStormStarted?.Invoke();
            Plugin.Logger.LogMessage("Storm started!");
        }

        public static void StormEnd()
        {
            Plugin.Logger.LogMessage("Storm ended!");
            OnStormEnded?.Invoke();
        }
    }
}
