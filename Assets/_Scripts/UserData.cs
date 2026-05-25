using System;

[Serializable] // This allows Unity and Firebase to "read" the data
public class UserData
{
    public string username;
    public int totalSteps;
    public int currentPoints;
    public int currentStreak; // 👈 New variable to keep track of the daily streak

    // Constructor 1: Keeps your existing registration/auth scripts working perfectly
    public UserData(string name, int steps, int points)
    {
        this.username = name;
        this.totalSteps = steps;
        this.currentPoints = points;
        this.currentStreak = 0; // New accounts start at a 0-day streak
    }

    // Constructor 2: Allows you to pass a specific streak number when updating data
    public UserData(string name, int steps, int points, int streak)
    {
        this.username = name;
        this.totalSteps = steps;
        this.currentPoints = points;
        this.currentStreak = streak;
    }
}