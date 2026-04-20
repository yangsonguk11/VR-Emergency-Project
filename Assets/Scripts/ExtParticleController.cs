using UnityEngine;

public class ExtParticleController : MonoBehaviour
{
    [SerializeField] ParticleSystem PS;

    public void OnParticle()
    {
        gameObject.SetActive(true);
    }
    public void OffParticle()
    {
        gameObject.SetActive(false);
    }
}
