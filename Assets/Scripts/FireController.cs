using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireController : MonoBehaviour
{
    float fireIntensity;
    public float FireIntensity { get { return fireIntensity; } set { fireIntensity = value; UpdateFire(); } }

    [SerializeField] List<ParticleSystem> fireParticles;
    List<int> fireParticlesorginalemission;
    

    private void Start()
    {
        fireIntensity = 1f;
        fireParticlesorginalemission = new List<int>();
        foreach (ParticleSystem p in fireParticles)
        {
            fireParticlesorginalemission.Add(Mathf.RoundToInt(p.emission.rateOverTime.constant));
        }
    }
    void UpdateFire()
    {
        if (fireIntensity <= 0)
            Destroy(gameObject);
        for(int i = 0; i < fireParticles.Count; i++)
        {
            var emission = fireParticles[i].emission;
            emission.rateOverTime = fireParticlesorginalemission[i] * FireIntensity;
        }
        foreach(ParticleSystem p in fireParticles)
        {
            Debug.Log(p.emission.rateOverTime);
        }
    }

    IEnumerator FireCor()
    {
        float t = 0f;
        
        while(t < 8)
        {
            FireIntensity = 1f - t / 8f;
            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    public void ParticleCollide()
    {
        FireIntensity -= 0.005f;
    }
}
