using UnityEngine;
using TMPro;

public class LeaderboardRowDisplay : MonoBehaviour
{
    [Header("Row Component Links")]
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI stepCountText;

    // A helper method to easily populate data with your chosen font styles
    public void SetupRowDisplay(int rank, string username, int totalSteps)
    {
        if (rankText != null) rankText.text = "#" + rank;
        if (usernameText != null) usernameText.text = username;
        if (stepCountText != null) stepCountText.text = totalSteps.ToString("N0") + " Steps";
    }
}