using UnityEngine;

public class ClothesChangeUI : MonoBehaviour
{
    public GameObject askCanvas;

    public GameObject suitBody;
    public GameObject noneBody;

    public Transform playerCamera;
    public float showDistance = 5f;

    private bool isFallen = false;
    private bool changed = false;
    private bool canvasShown = false;

    void Start()
    {
        askCanvas.SetActive(false);
        suitBody.SetActive(true);
        noneBody.SetActive(false);
    }

    void Update()
    {
        if (!isFallen || changed)
            return;

        float distance = Vector3.Distance(playerCamera.position, transform.position);

        if (distance <= showDistance)
        {
            if (!canvasShown)
            {
                askCanvas.SetActive(true);
                FacePlayerOnce();
                canvasShown = true;
            }
        }
        else
        {
            askCanvas.SetActive(false);
            canvasShown = false;
        }
    }

    void FacePlayerOnce()
    {
        Vector3 dir = askCanvas.transform.position - playerCamera.position;
        dir.y = 0f;

        if (dir != Vector3.zero)
            askCanvas.transform.rotation = Quaternion.LookRotation(dir);
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
        changed = true;
        askCanvas.SetActive(false);
    }
}