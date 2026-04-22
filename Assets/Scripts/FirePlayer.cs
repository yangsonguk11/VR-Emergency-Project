using UnityEngine;

public class FirePlayer : MonoBehaviour
{
    public int SmokeGauge;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("fdsa");
        if (other.gameObject.CompareTag("Smoke"))
            SmokeGauge += 1;
    }
}
