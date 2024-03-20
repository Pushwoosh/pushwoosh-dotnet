namespace PushwooshSDK.DotNet.Geozones;

// All the code in this file is included in all platforms.
public abstract class LocationManager
{
    static LocationManager manager = null;
    public static LocationManager Instance
    {
        get
        {
            return manager;
        }
        set
        {
            manager = value;
        }
    }

    public abstract void StartLocationTracking();

    public abstract void StopLocationTracking();
}

