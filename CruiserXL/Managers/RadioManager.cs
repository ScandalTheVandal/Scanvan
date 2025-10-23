using RadioBrowser.Models;
using RadioBrowser;
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Http;
using UnityEngine.Experimental.GlobalIllumination;

/// <summary>
///  Available from RadioFurniture, licensed under GNU General Public License.
///  Source: https://github.com/legoandmars/RadioFurniture/tree/master/RadioFurniture
/// </summary>
namespace CruiserXL.Managers
{
    public static class RadioManager
    {
        static List<StationInfo> _stations = new();
        public static void PreloadStations()
        {
            GetRadioStations().Forget();
        }

        private static async UniTask GetRadioStations()
        {
            Plugin.Logger.LogMessage("Searching Radio API for stations!");
            // Initialization
            var apiUrl = await GetRadioBrowserApiUrl();
            var radioBrowser = new RadioBrowserClient(apiUrl);
            //List<StationInfo> stationResults = await radioBrowser.Stations.GetByVotesAsync(2000);
            List<StationInfo> stationResults = await radioBrowser.Search.AdvancedAsync(new AdvancedSearchOptions
            {
                Language = "english"
            });
            _stations = stationResults.Where(x => x != null && x.Codec != null && x.Codec == "MP3" && x.UrlResolved != null && x.UrlResolved.Scheme == "https").ToList();
            Plugin.Logger.LogMessage("Finished searching radio API for stations.");
            Plugin.Logger.LogMessage($"Found {_stations.Count}");
        }

        public static StationInfo? GetRandomRadioStation()
        {
            if (_stations.Count == 0) return null;
            var station = _stations[UnityEngine.Random.Range(0, _stations.Count)];
            Plugin.Logger.LogMessage(station.Name);
            Plugin.Logger.LogMessage(station.StationUuid);
            return station;
        }

        public static StationInfo? GetRadioStationByGuid(Guid guid)
        {
            return _stations.FirstOrDefault(x => x.StationUuid == guid);
        }

        // huge kudos to zaggy for helping me diagnose, and fix this!
        private static async Task<string> GetRadioBrowserApiUrl()
        {
            // get fastest ip of dns
            const string baseUrl = @"all.api.radio-browser.info";
            var ips = Dns.GetHostAddresses(baseUrl);
            var lastRoundTripTime = long.MaxValue;
            var searchUrl = @"de1.api.radio-browser.info";
            HttpClient client = new HttpClient();

            foreach (var ipAddress in ips)
                try
                {
                    var reply = new Ping().Send(ipAddress);
                    if (reply == null || reply.RoundtripTime >= lastRoundTripTime) continue;
                    var hostName = (await Dns.GetHostEntryAsync(ipAddress)).HostName;
                    if (string.IsNullOrEmpty(hostName)) continue;
                    var result = await client.GetAsync($"https://{hostName}/");
                    Plugin.Logger.LogError($"{hostName}, is Success? {result.IsSuccessStatusCode}, ip? {ipAddress}");
                    if (!result.IsSuccessStatusCode) continue;
                    lastRoundTripTime = reply.RoundtripTime;
                    searchUrl = hostName;
                }
                catch (SocketException)
                {
                    Plugin.Logger.LogError("Cannot ping socket");
                }
            return searchUrl;
        }
    }
}
