using UnityEngine;

public class PlayerSFXBank : MonoBehaviour
{
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip slidingSound;
    [SerializeField] private AudioClip slideFailedSound;
    [SerializeField] private AudioClip[] walkSounds;
    [SerializeField] private AudioClip airAmbience;
    [SerializeField] private AudioClip powerDown;

    public AudioClip JumpSound() { return jumpSound; }
    public AudioClip LandSound() { return landSound; }
    public AudioClip SlidingSound() { return slidingSound; }
    public AudioClip SlideFailedSound() { return slideFailedSound; }
    public AudioClip[] WalkSounds() { return walkSounds; }
    public AudioClip AirAmbience() { return airAmbience; }
    public AudioClip PowerDown() { return powerDown; }
}
