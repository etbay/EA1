using UnityEditor;
using UnityEngine;

public class LevelNamePopup : EditorWindow
{
    private string levelName = "";
    private System.Action<string> onConfirm;

    public static void Show(System.Action<string> onConfirmCallback)
    {
        LevelNamePopup window = ScriptableObject.CreateInstance<LevelNamePopup>();
        window.titleContent = new GUIContent("Enter Level Name");
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 300, 100);
        window.onConfirm = onConfirmCallback;
        window.ShowModal(); // pauses editor until closed
    }

    private void OnGUI()
    {
        GUILayout.Label("Enter Level Name for this scene:", EditorStyles.boldLabel);
        levelName = EditorGUILayout.TextField("Level Name", levelName);

        GUILayout.Space(10);

        if (GUILayout.Button("Create LevelData"))
        {
            if (!string.IsNullOrEmpty(levelName))
            {
                onConfirm?.Invoke(levelName);
                Close();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Level name cannot be empty", "OK");
            }
        }
    }
}
