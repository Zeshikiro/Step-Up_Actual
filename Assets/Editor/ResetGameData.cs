using UnityEngine;
using UnityEditor;

public class ResetGameData : EditorWindow
{
    [MenuItem("Step Up Tools/💥 FACTORY RESET (Wipe All Saved Data)")]
    public static void ClearAllData()
    {
        // 1. Erase all PlayerPrefs (BMI setup, Onboarding status, Coins, Steps, etc)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // 2. Clear out Firebase Auth cache if they are signed in inside the Editor
        try
        {
            if (Firebase.Auth.FirebaseAuth.DefaultInstance != null)
            {
                Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Firebase auth wipe skipped (not initialized yet).");
        }

        Debug.Log("<color=green><b>SUCCESS:</b></color> All local saved data has been completely erased! The next time you hit Play, you will start as a brand new user.");
    }
}
