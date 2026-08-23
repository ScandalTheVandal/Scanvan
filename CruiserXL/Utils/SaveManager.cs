namespace ScanVan.Utils;
public static class SaveManager
{
    internal const string SavePrefix = "VanData.";

    // data to save
    internal const string SavedVanRotation = "AttachedVanRotation";
    internal const string SavedVanPosition = "AttachedVanPosition";
    internal const string SavedVanTurbo = "AttachedVanTurbo";
    internal const string SavedVanVariant = "AttachedVanVariant";
    internal const string SavedVanIgnition = "AttachedVanIgnition";
    internal const string SavedVanSteering = "AttachedVanSteeringRotation";
    internal const string SavedVanHealth = "AttachedVanHealth";
    internal const string SavedVanServiceAlert = "AttachedVanServiceAlert";
    internal const string SavedVanAltAlerts = "AttachedVanAltAlerts";
    internal const string SavedVanMileage = "AttachedVanMileage";

    public static void Save<T>(string key, T data)
    {
        ES3.Save(SavePrefix + key, data, GameNetworkManager.Instance.currentSaveFileName);
    }

    public static bool TryLoad<T>(string key, out T data)
    {
        if (!ES3.KeyExists(SavePrefix + key, GameNetworkManager.Instance.currentSaveFileName))
        {
            data = default!;
            return false;
        }
        data = ES3.Load<T>(SavePrefix + key, GameNetworkManager.Instance.currentSaveFileName);
        return true;
    }

    public static void Delete(string key)
    {
        ES3.DeleteKey(SavePrefix + key, GameNetworkManager.Instance.currentSaveFileName);
    }
}