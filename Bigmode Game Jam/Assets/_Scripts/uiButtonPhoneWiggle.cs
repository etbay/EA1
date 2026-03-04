using UnityEngine;

public class uiButtonPhoneWiggle : MonoBehaviour
{
    [Header("Wiggle")]
    public float angle = 6f;          // Rotation amount
    public float wiggleSpeed = 25f;   // How fast it shakes
    public float wiggleDuration = 0.25f;

    [Header("Pause")]
    public float pauseDuration = 1.5f;

    private RectTransform rectTransform;
    private float baseZ;
    private float timer;
    private bool isWiggling;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        baseZ = rectTransform.localEulerAngles.z;
        StartPause();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (isWiggling)
        {
            float z = baseZ + Mathf.Sin(Time.time * wiggleSpeed) * angle;
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, z);

            if (timer <= 0f)
                StartPause();
        }
        else
        {
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, baseZ);

            if (timer <= 0f)
                StartWiggle();
        }
    }

    void StartWiggle()
    {
        isWiggling = true;
        timer = wiggleDuration;
    }

    void StartPause()
    {
        isWiggling = false;
        timer = pauseDuration;
    }
}
