//using UnityEditor.SettingsManagement;
using UnityEngine;
using UnityEngine.UI;

public class SettingsButton : MonoBehaviour
{
    [SerializeField] private GameObject settingsPopup;
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ToggleSettings);
        settingsPopup.SetActive(false);
    }
    void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
    private void ToggleSettings()
    {
        AudioManager.instance.PlayPersistentSoundClip(AudioManager.instance.click, 1f, false, false);
        settingsPopup.SetActive(!settingsPopup.activeInHierarchy);
    }
}
