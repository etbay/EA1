using UnityEngine;
using UnityEngine.UI;

public class UISlickometer : MonoBehaviour
{
    private Slider slider;
    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    void Update()
    {
        slider.value = Player.SlickValue;
    }
}
