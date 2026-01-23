using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    [Header("Wave UI Elements")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI centerText; // Used for "Wave Incoming" AND Countdown

    public void UpdateTimer(float timeRemaining)
    {
        if (timerText != null)
        {
            // Format time as 00
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        }
    }

    public void ShowCenterText(string message)
    {
        if (centerText != null)
        {
            centerText.gameObject.SetActive(true);
            centerText.text = message;
        }
    }

    public void HideCenterText()
    {
        if (centerText != null)
        {
            centerText.gameObject.SetActive(false);
        }
    }
}
