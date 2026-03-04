using UnityEngine;

public class voiceover : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clickSound;

    public void PlaySound()
    {
        audioSource.PlayOneShot(clickSound);
        gameObject.SetActive(false);
    }
}