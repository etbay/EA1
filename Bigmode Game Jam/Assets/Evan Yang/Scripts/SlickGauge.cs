using UnityEngine;

public class SlickGauge : MonoBehaviour
{
    [SerializeField] private float maxSlick;
    [SerializeField] private Vector3 rotationFull;
    [SerializeField] private Vector3 rotationEmpty;
    [SerializeField] private Transform needle;

    private void Update()
    {
        needle.localEulerAngles = Vector3.Lerp(rotationEmpty, rotationFull, Player.SlickValue / maxSlick);
    }
}
