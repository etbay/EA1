using UnityEngine;
using UnityEngine.UI;

public class PauseScreen : MonoBehaviour
{
    [SerializeField] GameObject display;
    private void Start()
    {
        display.SetActive(false);
    }
    public void Enable()
    {
        Debug.Log("Enabling pause screen");
        display.SetActive(true);
    }
    public void Disable()
    {
        display.SetActive(false);
    }
    public void UpdateMusicVol(float newVal)
    {
        AudioManager.instance.mixer.SetFloat("MusicVolume", newVal);
    }
    public void UpdateMasterVol(float newVal)
    {
        AudioManager.instance.mixer.SetFloat("MasterVolume", newVal);
    }
    public void UpdateSFXVol(float newVal)
    {
        AudioManager.instance.mixer.SetFloat("SFXVolume", newVal);
    }
    public void ResumeGame()
    {
        AudioManager.instance.PlayPersistentSoundClip(AudioManager.instance.click, 1f, false, false);
        LevelManager.instance?.ResumeGame();
    }
    public void GoToMenu()
    {
        AudioManager.instance.PlayPersistentSoundClip(AudioManager.instance.click, 1f, false, false);
        LevelManager.instance?.GoToMenu();
    }
}
