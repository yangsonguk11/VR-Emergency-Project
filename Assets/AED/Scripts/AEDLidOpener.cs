using UnityEngine;

public class AEDLidOpener : MonoBehaviour
{
    public Transform lidObject;
    public Vector3 openedRotation = new Vector3(-100f, 0f, 0f);
    public float openSpeed = 2f;

    private bool isOpening = false;
    private Quaternion targetRotation;

    private void Start()
    {
        if (lidObject != null)
            targetRotation = Quaternion.Euler(openedRotation);
    }

    private void Update()
    {
        if (!isOpening || lidObject == null)
            return;

        lidObject.localRotation = Quaternion.Slerp(
            lidObject.localRotation,
            targetRotation,
            Time.deltaTime * openSpeed
        );
    }

    public void OpenLid()
    {
        isOpening = true;
    }
}