using UnityEngine;
using UnityEngine.UI;

public class QuitButton : MonoBehaviour
{
    private Button button;
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(QuitGame);
    }
    void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
    private void QuitGame()
    {
        AudioManager.instance.PlayPersistentSoundClip(AudioManager.instance.click, 1f, false, false);
        Application.Quit();
    }
}
