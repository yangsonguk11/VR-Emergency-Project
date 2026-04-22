using System.Collections.Generic;
using UnityEngine;

public class ExtParticleController : MonoBehaviour
{
    [SerializeField] ParticleSystem PS;
    private void Awake()
    {
        // 1. 씬에서 FireController가 붙은 오브젝트를 모두 찾습니다.
        FireController[] targets = FindObjectsByType<FireController>(FindObjectsSortMode.None);

        // 2. 파티클 시스템의 트리거 모듈을 가져옵니다.
        var triggerModule = PS.trigger;

        // 3. 찾은 오브젝트들의 Collider를 트리거 리스트에 등록합니다.
        for (int i = 0; i < targets.Length; i++)
        {
            Collider col = targets[i].GetComponent<Collider>();
            if (col != null)
            {
                // 인덱스 i 위치에 콜라이더를 할당합니다.
                triggerModule.SetCollider(i, col);
            }
        }
    }
    public void OnParticle()
    {
        gameObject.SetActive(true);
    }
    public void OffParticle()
    {
        gameObject.SetActive(false);
    }
    void OnParticleTrigger()
    {
        List<ParticleSystem.Particle> enterParticles = new List<ParticleSystem.Particle>();
        // 트리거 모듈에 등록된 모든 콜라이더를 순회
        int numTrigger = PS.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enterParticles, out var deltaData);

        for (int i = 0; i < deltaData.GetColliderCount(0); i++)
        {
            Component target = deltaData.GetCollider(0, i);
            // 대상 오브젝트의 특정 인터페이스나 컴포넌트를 호출
            if (target.TryGetComponent(out FireController responder))
            {
                responder.ParticleCollide();
            }
        }
    }

}
