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

    // VRRayClicker에서 Ray가 버튼을 맞췄을 때 호출됨
    public void Interact()
    {
        PressShockButton();
    }

    private void PressShockButton()
    {
        if (isPressed)
        {
            Debug.Log("이미 Shock 버튼이 눌린 상태");
            return;
        }

        isPressed = true;

        Debug.Log("주황색 Shock 버튼 Ray 클릭 성공");

        if (aedChecker != null)
        {
            aedChecker.NotifyShockButtonPressed();
        }
        else
        {
            Debug.LogError("AEDChecker 연결 안됨");
        }

        if (shockAudioSource != null && shockClip != null)
        {
            shockAudioSource.PlayOneShot(shockClip);
        }

        if (shockEffect != null)
        {
            shockEffect.Play();
        }
    }
}