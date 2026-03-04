using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] ParticleSystem Boom;
    [SerializeField] ParticleSystem Smoke;
    [SerializeField] ParticleSystem Sparks;
    [SerializeField] AudioClip[] ExplosionSoundEffects;
    public void Play()
    {
        Boom.Play();
        Smoke.Play();
        Sparks.Play();
        AudioManager.instance?.PlaySoundClipFromList(ExplosionSoundEffects, this.gameObject.transform.position, 1f, true, true);
    }
}
