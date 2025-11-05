using System;
using UnityEngine;

/// <summary>
///  Available from RadioFurniture, licensed under GNU General Public License.
///  Source: https://github.com/legoandmars/RadioFurniture/tree/master/RadioFurniture
/// </summary>
namespace CruiserXL.Events
{
    public static class WeatherEvents
    {
        public static event Action OnStormStarted = null!;
        public static event Action OnStormEnded = null!;

        public static void StormStart()
        {
            OnStormStarted?.Invoke();
        }

        public static void StormEnd()
        {
            OnStormEnded?.Invoke();
        }
    }
}
