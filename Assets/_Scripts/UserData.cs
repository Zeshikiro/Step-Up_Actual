using System;

[Serializable] // This allows Unity and Firebase to "read" the data
public class UserData
{
    public string username;
    public int totalSteps;
    public int currentPoints;

    public UserData(string name, int steps, int points)
    {
        this.username = name;
        this.totalSteps = steps;
        this.currentPoints = points;
    }
}