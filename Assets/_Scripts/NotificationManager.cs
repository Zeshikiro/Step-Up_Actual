using UnityEngine;
using System;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

public class NotificationManager : MonoBehaviour
{
    private string channelId = "motivational_reminders";

    // A pool of inspirational texts to surprise the player with
    private string[] motivationalQuotes = new string[]
    {
        "Time to get your steps in! Every step counts towards a healthier you. 🚶‍♂️",
        "Consistency is key! Let's smash that daily target today. 🎯",
        "Small progress is still progress. Put on those sneakers! 👟",
        "Believe you can and you're halfway there. Let's take a quick walk! ✨",
        "Your body can stand almost anything. Go give your avatar a boost! 💪"
    };

    private void Start()
    {
        #if UNITY_ANDROID
        InitializeAndroidChannels();
        // Automatically check and refresh the schedule when the app opens
        RefreshNotificationSchedule();
        #endif
    }

    #if UNITY_ANDROID
    private void InitializeAndroidChannels()
    {
        // Registering a notification channel is required for Android 8.0+
        var channel = new AndroidNotificationChannel()
        {
            Id = channelId,
            Name = "Daily Motivation",
            Importance = Importance.Default,
            Description = "Inspirational step-tracking reminders."
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }
    #endif

    public void RefreshNotificationSchedule()
    {
        #if UNITY_ANDROID
        // 1. Clear out old scheduled ones so we don't accidentally stack up duplicates
        AndroidNotificationCenter.CancelAllScheduledNotifications();

        // 2. Read the preference from the toggle we wired up earlier!
        bool notificationsAllowed = PlayerPrefs.GetInt("NotificationsEnabled", 1) == 1;

        if (!notificationsAllowed)
        {
            Debug.Log("Notifications disabled by user. Cleared all scheduled texts.");
            return; 
        }

        // 3. Pick a random phrase from our pool
        string chosenText = motivationalQuotes[UnityEngine.Random.Range(0, motivationalQuotes.Length)];

        // 4. Set up the notification structure
        var notification = new AndroidNotification();
        notification.Title = "Step Up! 🚀";
        notification.Text = chosenText;
        
        // --- CHOOSE YOUR TIMING METHOD FOR TESTING ---
        
        // Production: Send it 24 hours from right now
        notification.FireTime = DateTime.Now.AddHours(24);

        // ADD THIS LINE FOR TRUE DAILY REPEATING:
        notification.RepeatInterval = TimeSpan.FromDays(1);

        // Testing: UNCOMMENT the line below to make it fire 15 seconds after closing the app!
        //notification.FireTime = DateTime.Now.AddSeconds(15);

        // 5. Send it off to the mobile scheduling queue
        AndroidNotificationCenter.SendNotification(notification, channelId);
        Debug.Log("Successfully scheduled notification: " + chosenText);
        #endif
    }
}