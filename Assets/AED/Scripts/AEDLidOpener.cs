using UnityEngine;

public class AEDLidOpener : MonoBehaviour
{
    public Transform lidObject;   // ¶Ñ²± ¿ÀºêÁ§Æ® (SM_AEDPart02)
    public float openAngle = 110f;
    public float speed = 120f;

    private bool isOpening = false;
    private float currentAngle = 0f;

    public void OpenLid()
    {
        Debug.Log("OpenLid È£ÃâµÊ");
        isOpening = true;
    }

    void Update()
    {
        if (isOpening)
        {
            Debug.Log("¶Ñ²± ¿©´Â Áß");

            if (currentAngle < openAngle)
            {
                float delta = speed * Time.deltaTime;
                currentAngle += delta;

                lidObject.localRotation = Quaternion.Euler(-currentAngle, 0f, 0f);
            }
        }
    }
}