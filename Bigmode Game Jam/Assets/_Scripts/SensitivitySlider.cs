using UnityEngine;
using UnityEngine.UI;

public class SensitivitySlider : MonoBehaviour
{
    public static float mouseSensitivity = 0.05f;
    [SerializeField] private Slider slider;

    private void Awake()
    {
        slider.onValueChanged.AddListener(SetSens);
    }
    private void Start()
    {
        float val = PlayerPrefs.GetFloat("MouseSensitivity", 0.05f);
        SetSens(val);
        slider.value = val;
    }
    private void SetSens(float value)
    {
        mouseSensitivity = value;
        PlayerPrefs.SetFloat("MouseSensitivity", slider.value);
        PlayerPrefs.Save();
    }

    void OnDestroy()
    {
        slider.onValueChanged.RemoveAllListeners();
    }
}
