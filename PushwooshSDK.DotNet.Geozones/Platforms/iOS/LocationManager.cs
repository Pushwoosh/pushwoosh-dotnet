using Com.Pushwoosh.iOS;

namespace PushwooshSDK.DotNet.Geozones.iOS;

// All the code in this file is only included on iOS.
public class LocationManager : PushwooshSDK.DotNet.Geozones.LocationManager
{
    public static new LocationManager Instance => global::PushwooshSDK.DotNet.Geozones.LocationManager.Instance as LocationManager;

    PWGeozonesManager nativeManager;

    public static void Init()
    {
        global::PushwooshSDK.DotNet.Geozones.LocationManager.Instance = new LocationManager();
    }

    public LocationManager()
    {
        nativeManager = PWGeozonesManager.SharedManager;
    }

    public override void StartLocationTracking()
    {
        nativeManager.StartLocationTracking();
    }

    public override void StopLocationTracking()
    {
        nativeManager.StopLocationTracking();
    }
}

