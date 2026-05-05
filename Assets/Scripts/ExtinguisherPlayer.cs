using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExtinguisherPlayer : MonoBehaviour
{
    [SerializeField] CanvasGroup gameclearcanvasgroup;
    [SerializeField] GameObject obj;
    bool isEnd = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (obj == null && !isEnd)
        {
            GameClear();
            isEnd = true;
        }
    }
    void GameClear()
    {
        StartCoroutine(GameClearCor());
    }
    IEnumerator GameClearCor()
    {
        float t = 0;
        while (t <= 2)
        {
            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        t = 0;
        while (t <= 6)
        {
            t += Time.fixedDeltaTime;
            gameclearcanvasgroup.alpha = t / 2f;
            yield return new WaitForFixedUpdate();
        }
        SceneManager.LoadScene("MainMenu");
    }
}
