using Com.Pushwoosh.Location;

namespace PushwooshSDK.DotNet.Geozones.Android;

// All the code in this file is only included on Android.
public class LocationManager : PushwooshSDK.DotNet.Geozones.LocationManager
{
    public static new LocationManager Instance => global::PushwooshSDK.DotNet.Geozones.LocationManager.Instance as LocationManager;

    public static void Init()
    {
        global::PushwooshSDK.DotNet.Geozones.LocationManager.Instance = new LocationManager();
    }

    public override void StartLocationTracking()
    {
        PushwooshLocation.StartLocationTracking();
    }

    public override void StopLocationTracking()
    {
        PushwooshLocation.StopLocationTracking();
    }
}
