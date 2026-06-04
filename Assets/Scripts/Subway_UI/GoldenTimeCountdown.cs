using UnityEngine;
using TMPro;

public class GoldenTimeCountdown : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    public float totalTime = 300f; // 5Ка

    private float currentTime;
    private bool isCounting = false;

    void Start()
    {
        currentTime = totalTime;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isCounting) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isCounting = false;
        }

        int minute = Mathf.FloorToInt(currentTime / 60f);
        int second = Mathf.FloorToInt(currentTime % 60f);

        timeText.text = minute.ToString("00") + ":" + second.ToString("00");
    }

    public void StartCountdown()
    {
        currentTime = totalTime;
        isCounting = true;
        gameObject.SetActive(true);
    }
}