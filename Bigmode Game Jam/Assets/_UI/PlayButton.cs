using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
    [SerializeField] private LevelData level1;
    private Button button;
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlayGame);
    }
    void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
    private void PlayGame()
    {
        AudioManager.instance.PlayPersistentSoundClip(AudioManager.instance.Play, 1f, false, false);
        SceneManager.LoadScene(level1.sceneName);
    }
}
