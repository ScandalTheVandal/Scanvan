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
using ScanVan;

/// <summary>
///  Available from RadioFurniture, licensed under GNU General Public License.
///  Source: https://github.com/legoandmars/RadioFurniture/tree/master/RadioFurniture
/// </summary>
namespace ScanVan.Managers;

public static class RadioManager
{
    public static List<StationInfo> _stations = new();
    public static void PreloadStations()
    {
        GetRadioStations().Forget();
    }
        
    public static async UniTask GetRadioStations()
    {
        try
        {
            Plugin.Logger.LogDebug("Searching Radio API for stations!");

            var apiUrl = await GetRadioBrowserApiUrl();
            var radioBrowser = new RadioBrowserClient(apiUrl);
            List<StationInfo> stationResults = await radioBrowser.Stations.GetByVotesAsync(7500);

            _stations = stationResults.Where(
                x => x != null
                && x.Codec != null
                && x.Codec.Equals("mp3", StringComparison.OrdinalIgnoreCase)
                && x.UrlResolved != null &&
                (x.UrlResolved.Scheme == Uri.UriSchemeHttp || x.UrlResolved.Scheme == Uri.UriSchemeHttps))
                .ToList();

            Plugin.Logger.LogDebug("Finished searching radio API for stations.");
            Plugin.Logger.LogDebug($"Found {_stations.Count}");
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"Error finding radio stations: {ex.Message}");
        }
    }

    public static StationInfo? GetRandomRadioStation()
    {
        if (_stations.Count == 0) return null;
        var station = _stations[UnityEngine.Random.Range(0, _stations.Count)];
        Plugin.Logger.LogDebug(station.Name);
        Plugin.Logger.LogDebug(station.StationUuid);
        return station;
    }

    public static StationInfo? GetRadioStationByGuid(Guid guid)
    {
        return _stations.FirstOrDefault(x => x.StationUuid != null && x.StationUuid == guid);
    }

    // kudos to Zaggy for being my teacher through-out this! /p2
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
                Plugin.Logger.LogDebug($"{hostName}, Is success? {result.IsSuccessStatusCode}, IP? {ipAddress}");
                if (!result.IsSuccessStatusCode) continue;
                lastRoundTripTime = reply.RoundtripTime;
                searchUrl = hostName;
            }
            catch (SocketException)
            {
                Plugin.Logger.LogWarning("Cannot ping socket!");
            }
        return searchUrl;
    }
}
