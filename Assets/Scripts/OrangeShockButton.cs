using UnityEngine;

public class OrangeShockButton : MonoBehaviour
{
    [Header("AED Checker")]
    public AEDChecker aedChecker;

    [Header("Effects")]
    public AudioSource shockAudioSource;
    public AudioClip shockClip;

    public ParticleSystem shockEffect;

    private bool isPressed = false;

    public void PressShockButton()
    {
        Debug.Log("PressShockButton 함수 진입");

        if (isPressed)
        {
            Debug.Log("이미 버튼 눌림 상태");
            return;
        }

        isPressed = true;

        Debug.Log("제세동 버튼 클릭 성공");

        // AED 진행 상태 알림
        if (aedChecker != null)
        {
            Debug.Log("AEDChecker 연결 확인됨");

            aedChecker.NotifyShockButtonPressed();
        }
        else
        {
            Debug.Log("AEDChecker 연결 안됨");
        }

        // 효과음
        if (shockAudioSource != null)
        {
            Debug.Log("Shock AudioSource 있음");

            if (shockClip != null)
            {
                Debug.Log("Shock Clip 재생");

                shockAudioSource.PlayOneShot(shockClip);
            }
            else
            {
                Debug.Log("Shock Clip 없음");
            }
        }
        else
        {
            Debug.Log("Shock AudioSource 없음");
        }

        // 파티클
        if (shockEffect != null)
        {
            Debug.Log("Shock Effect 실행");

            shockEffect.Play();
        }
        else
        {
            Debug.Log("Shock Effect 없음");
        }
    }
}