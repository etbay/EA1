using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.SceneTemplate;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTemplatePipeline : ISceneTemplatePipeline
{
    public virtual bool IsValidTemplateForInstantiation(SceneTemplateAsset sceneTemplateAsset)
    {
        return true;
    }

    public virtual void BeforeTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset, bool isAdditive, string sceneName)
    {

    }

    // public virtual void AfterTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset, Scene scene, bool isAdditive, string sceneName)
    // {
    //     string assetPath = $"Assets/_Scriptable_Objects/{sceneName}_LevelData.asset";
    //     assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

    //     LevelData levelData = ScriptableObject.CreateInstance<LevelData>();
    //     AssetDatabase.CreateAsset(levelData, assetPath);
    //     AssetDatabase.SaveAssets();

    //     foreach(var root in scene.GetRootGameObjects())
    //     {
    //         LevelManager levelManager = root.GetComponentInChildren<LevelManager>();
    //         if(levelManager != null)
    //         {
    //             levelManager.data = levelData;
    //             EditorUtility.SetDirty(levelManager);
    //             Debug.Log($"[SceneTemplatePipeline] Created LevelData at {assetPath} and assigned to LevelManager in scene {sceneName}");
    //             return;
    //         }
    //     }
    // }

    public virtual void AfterTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset, Scene scene, bool isAdditive, string sceneName)
    {
        // Open popup to ask user for level name
        LevelNamePopup.Show((enteredName) =>
        {
            if (string.IsNullOrEmpty(enteredName))
                enteredName = "NewLevel";

            CreateLevelData(scene, enteredName);
            SaveSceneWithName(scene, enteredName);
        });
    }

    private void CreateLevelData(Scene scene, string levelName)
    {
        // Ensure folder exists
        string folderPath = "Assets/_LevelData";
        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets", "_LevelData");

        // Create unique asset path
        string assetPath = Path.Combine(folderPath, $"{levelName}_LevelData.asset");
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        // Create the LevelData asset
        LevelData levelData = ScriptableObject.CreateInstance<LevelData>();
        AssetDatabase.CreateAsset(levelData, assetPath);
        AssetDatabase.SaveAssets();

        // Assign to LevelManager(s) in the scene
        foreach (var rootGO in scene.GetRootGameObjects())
        {
            LevelManager manager = rootGO.GetComponentInChildren<LevelManager>();
            if (manager != null)
            {
                manager.data = levelData;
                EditorUtility.SetDirty(manager);
            }
        }

        Debug.Log($"[SceneTemplatePipeline] Created LevelData '{assetPath}' and assigned to LevelManager(s) in scene {scene.name}");
    }

    private void SaveSceneWithName(Scene scene, string levelName)
    {
        // Ensure Scenes folder exists
        string scenesFolder = "Assets/_Scenes/Levels";
        if (!AssetDatabase.IsValidFolder(scenesFolder))
        {
            AssetDatabase.CreateFolder("Assets", "_Scenes");
        }
        if (!AssetDatabase.IsValidFolder(scenesFolder))
        {
            AssetDatabase.CreateFolder("Assets/_Scenes", "Levels");
        }

        // Build unique scene path
        string scenePath = Path.Combine(scenesFolder, levelName + ".unity");
        scenePath = AssetDatabase.GenerateUniqueAssetPath(scenePath);

        // Save the scene
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[SceneTemplatePipeline] Saved scene as '{scenePath}'");
    }
}
