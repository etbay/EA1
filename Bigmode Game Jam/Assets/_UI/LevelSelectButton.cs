using UnityEngine;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] private GameObject popup;
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(TogglePopup);
        popup.SetActive(false);
    }
    void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
    private void TogglePopup()
    {
        AudioManager.instance.PlayPersistentSoundClip(AudioManager.instance.click, 1f, false, false);
        popup.SetActive(!popup.activeInHierarchy);
    }
}
