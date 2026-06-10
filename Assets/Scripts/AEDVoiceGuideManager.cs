using System.Collections;
using UnityEngine;

public class AEDVoiceGuideManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Voice Clips")]
    public AudioClip attachPadClip;       // 1. 환자 가슴에 패드를 부착하세요
    public AudioClip analyzingClip;       // 2. 분석 진행 중입니다
    public AudioClip needShockClip;       // 3. 재세동 해야합니다
    public AudioClip stayAwayClip;        // 4. 환자에게서 떨어지세요
    public AudioClip pressOrangeClip;     // 5. 주황색 버튼을 클릭하세요
    public AudioClip shockDoneClip;       // 6. 제세동 실시되었습니다

    [Header("CPR Character")]
    public Animator cprAnimator;
    public string idleTriggerName = "Idle";
    public string cprTriggerName = "CPR";

    private bool isStarted = false;

    // AED 뚜껑이 열렸을 때 호출
    public void OnLidOpened()
    {
        if (isStarted) return;

        isStarted = true;
        StartCoroutine(StartFirstVoice());
    }

    // 뚜껑 열리고 1초 후 첫 음성 출력
    private IEnumerator StartFirstVoice()
    {
        yield return new WaitForSeconds(1f);
        PlayVoice(attachPadClip);
    }

    // 1번 완료: 패드 부착 완료
    public void OnPadsAttached()
    {
        PlayVoice(analyzingClip);
        StartCoroutine(AfterAnalyze());
    }

    // 분석 음성 후 다음 단계로 진행
    private IEnumerator AfterAnalyze()
    {
        yield return new WaitForSeconds(analyzingClip.length + 2f);

        PlayVoice(needShockClip);
        yield return new WaitForSeconds(needShockClip.length + 2f);

        SetCPRIdle();

        PlayVoice(stayAwayClip);
        yield return new WaitForSeconds(stayAwayClip.length + 2f);

        PlayVoice(pressOrangeClip);
    }

    // 주황색 버튼 클릭 완료
    public void OnShockButtonPressed()
    {
        PlayVoice(shockDoneClip);
    }

    private void PlayVoice(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    private void SetCPRIdle()
    {
        if (cprAnimator == null)
        {
            Debug.LogWarning("CPR Animator가 연결되지 않았습니다.");
            return;
        }

        cprAnimator.ResetTrigger(cprTriggerName);
        cprAnimator.SetTrigger(idleTriggerName);

        Debug.Log("접촉금지 음성 출력 → CPR 캐릭터 Idle 전환");
    }
}