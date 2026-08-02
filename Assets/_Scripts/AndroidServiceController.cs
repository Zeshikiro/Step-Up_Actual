using UnityEngine;

public class AndroidServiceController : MonoBehaviour
{
    private const string ServiceClassName = "com.stepup.background.StepForegroundService";

    public static void StartForegroundService(int currentSteps)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", activity, new AndroidJavaClass(ServiceClassName));
                intent.Call<AndroidJavaObject>("putExtra", "currentSteps", currentSteps);
                
                // For Android 8.0+
                if (GetAndroidSDKVersion() >= 26)
                {
                    activity.Call<AndroidJavaObject>("startForegroundService", intent);
                }
                else
                {
                    activity.Call<AndroidJavaObject>("startService", intent);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to start foreground service: " + e.Message);
        }
#endif
    }

    public static void StopForegroundService()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", activity, new AndroidJavaClass(ServiceClassName));
                activity.Call<bool>("stopService", intent);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to stop foreground service: " + e.Message);
        }
#endif
    }

    public static int GetAndClearBackgroundSteps()
    {
        int backgroundSteps = 0;
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                if (activity != null)
                {
                    AndroidJavaObject prefs = activity.Call<AndroidJavaObject>("getSharedPreferences", "StepUpPrefs", 0);
                    backgroundSteps = prefs.Call<int>("getInt", "BackgroundSteps", 0);
                    
                    if (backgroundSteps > 0)
                    {
                        AndroidJavaObject editor = prefs.Call<AndroidJavaObject>("edit");
                        editor.Call<AndroidJavaObject>("putInt", "BackgroundSteps", 0);
                        editor.Call("apply");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to get background steps: " + e.Message);
        }
#endif
        return backgroundSteps;
    }

    private static int GetAndroidSDKVersion()
    {
        try
        {
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return version.GetStatic<int>("SDK_INT");
            }
        }
        catch
        {
            return 0;
        }
    }
}
