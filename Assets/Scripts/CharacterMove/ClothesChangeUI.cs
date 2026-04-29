using UnityEngine;

public class ClothesChangeUI : MonoBehaviour
{
    public GameObject askCanvas;

    public GameObject suitBody;      // Male_Suit_Belt
    public GameObject noneBody;      // Male_None_Belt

    public Transform playerCamera;
    public float showDistance = 5f;

    private bool isFallen = false;
    private bool changed = false;

    void Update()
    {
        if (!isFallen || changed)
        {
            Debug.Log("캐릭터 안쓰러짐");
            return;
        }

        float distance = Vector3.Distance(playerCamera.position, transform.position);

        if (distance <= showDistance)
        {
            askCanvas.SetActive(true);
            Debug.Log("캔버스 활성화");

            Vector3 dir = askCanvas.transform.position - playerCamera.position;
            dir.y = 0f;

            if (dir != Vector3.zero)
                askCanvas.transform.rotation = Quaternion.LookRotation(dir);
        }
        else
        {
            askCanvas.SetActive(false);
        }
    }

    public void SetFallen(bool value)
    {
        isFallen = value;
    }

    public void YesChangeClothes()
    {
        suitBody.SetActive(false);
        noneBody.SetActive(true);

        changed = true;
        askCanvas.SetActive(false);
    }

    public void NoClose()
    {
        askCanvas.SetActive(false);
    }
}