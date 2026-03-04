using UnityEngine;

public class RetryButton : MonoBehaviour
{
    public void RestartLevel()
    {
        AudioManager.instance.PlayPersistentSoundClip(AudioManager.instance.restart, 1f, false, false);
        LevelManager.instance?.RestartLevel();
    }
    public void NextLevel()
    {
        AudioManager.instance.PlayPersistentSoundClip(AudioManager.instance.click, 1f, false, false);
        LevelManager.instance?.NextLevel();
    }
}
