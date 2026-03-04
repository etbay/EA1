using System.Collections;
using TMPro;
using UnityEngine;

public class TutorialDisplay : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private Coroutine showRoutine;

    private void Awake()
    {
        textMesh = this.GetComponent<TextMeshProUGUI>();
        this.gameObject.SetActive(false);
    }

    public void Show(string text)
    {
        this.textMesh.text = text;
        this.gameObject.SetActive(true);

        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
        }

        showRoutine = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(4f);
        this.gameObject.SetActive(false);
    }
}
