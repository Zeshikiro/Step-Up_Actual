package com.stepup.background;

import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.os.Build;
import android.os.IBinder;

public class StepForegroundService extends Service implements SensorEventListener {
    private SensorManager sensorManager;
    private Sensor stepSensor;
    private int currentSteps = 0;
    private int initialSteps = -1;

    private static final String CHANNEL_ID = "step_tracker_foreground";
    private static final int NOTIFICATION_ID = 778;

    @Override
    public void onCreate() {
        super.onCreate();
        createNotificationChannel();
        sensorManager = (SensorManager) getSystemService(Context.SENSOR_SERVICE);
        if (sensorManager != null) {
            stepSensor = sensorManager.getDefaultSensor(Sensor.TYPE_STEP_COUNTER);
            if (stepSensor != null) {
                sensorManager.registerListener(this, stepSensor, SensorManager.SENSOR_DELAY_NORMAL);
            }
        }
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        if (intent != null && intent.hasExtra("currentSteps")) {
            currentSteps = intent.getIntExtra("currentSteps", 0);
        }
        startForeground(NOTIFICATION_ID, getNotification("Step-Up is tracking!", "You have taken " + currentSteps + " steps today. Keep going!"));
        return START_STICKY;
    }

    private Notification getNotification(String title, String text) {
        Notification.Builder builder;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            builder = new Notification.Builder(this, CHANNEL_ID);
        } else {
            builder = new Notification.Builder(this);
        }
        
        int iconId = getResources().getIdentifier("app_icon", "mipmap", getPackageName());
        if (iconId == 0) iconId = android.R.drawable.ic_menu_directions; // Fallback
        
        Intent launchIntent = getPackageManager().getLaunchIntentForPackage(getPackageName());
        PendingIntent pendingIntent = null;
        if (launchIntent != null) {
            int pendingFlags = PendingIntent.FLAG_UPDATE_CURRENT;
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                pendingFlags |= PendingIntent.FLAG_IMMUTABLE;
            }
            pendingIntent = PendingIntent.getActivity(this, 0, launchIntent, pendingFlags);
        }
        
        builder.setContentTitle(title)
               .setContentText(text)
               .setSmallIcon(iconId)
               .setOngoing(true);
               
        if (pendingIntent != null) {
            builder.setContentIntent(pendingIntent);
        }
        
        return builder.build();
    }

    private void createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel channel = new NotificationChannel(
                    CHANNEL_ID,
                    "Live Step Tracker",
                    NotificationManager.IMPORTANCE_LOW
            );
            channel.setDescription("Tracks your steps live in the background.");
            NotificationManager manager = getSystemService(NotificationManager.class);
            if (manager != null) manager.createNotificationChannel(channel);
        }
    }

    @Override
    public void onSensorChanged(SensorEvent event) {
        if (event.sensor.getType() == Sensor.TYPE_STEP_COUNTER) {
            int hardwareSteps = (int) event.values[0];
            if (initialSteps == -1) {
                initialSteps = hardwareSteps;
            }
            int stepsTakenSinceStart = hardwareSteps - initialSteps;
            int displaySteps = currentSteps + stepsTakenSinceStart;
            
            // Save the background steps to SharedPreferences so Unity can retrieve them on boot!
            android.content.SharedPreferences prefs = getSharedPreferences("StepUpPrefs", Context.MODE_PRIVATE);
            android.content.SharedPreferences.Editor editor = prefs.edit();
            editor.putInt("BackgroundSteps", stepsTakenSinceStart);
            editor.apply();
            
            NotificationManager manager = (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);
            if (manager != null) {
                manager.notify(NOTIFICATION_ID, getNotification("Step-Up is tracking!", "You have taken " + displaySteps + " steps today. Keep going!"));
            }
        }
    }

    @Override
    public void onAccuracyChanged(Sensor sensor, int accuracy) {
    }

    @Override
    public void onDestroy() {
        super.onDestroy();
        if (sensorManager != null && stepSensor != null) {
            sensorManager.unregisterListener(this);
        }
    }

    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }
}
