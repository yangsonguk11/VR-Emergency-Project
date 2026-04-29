using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirePlayer : MonoBehaviour
{
    [SerializeField] CanvasGroup gameovercanvasgroup;
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
        Debug.Log(other.gameObject);
        if (other.gameObject.CompareTag("Smoke"))
        {
            SmokeGauge += 1;
            if (SmokeGauge >= 1000)
            {
                GetComponent<Collider>().enabled = false;
                GameOver();
            }
        }
    }
    void GameOver()
    {
        StartCoroutine(GameOverCor());
    }
    IEnumerator GameOverCor()
    {
        float t = 0;
        while(t <= 4)
        {
            t += Time.fixedDeltaTime;
            gameovercanvasgroup.alpha = t / 2f;
            yield return new WaitForFixedUpdate();
        }
        SceneManager.LoadScene("FireScene");
    }
}
