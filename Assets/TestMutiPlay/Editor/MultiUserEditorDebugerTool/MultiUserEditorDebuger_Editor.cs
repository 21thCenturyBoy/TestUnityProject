using System;
using System.Linq;
using TestMutiPlay;
using UnityEditor;

public class MultiUserEditorDebuger_Editor
{
    [UnityEditor.MenuItem("Tools/MultiUserTools/EditorRun")]
    public static void EditorRun()
    {
        //Find InitUnitySceneName
        bool search = false;
        var scenes = UnityEditor.EditorBuildSettings.scenes;
        string path = string.Empty;
        for (int i = 0; i < scenes.Length; i++)
        {
            string scene = scenes[i].path;
            search = System.IO.Path.GetFileNameWithoutExtension(scene).Equals(MultiUserEditorDebuger.InitUnitySceneName, StringComparison.OrdinalIgnoreCase);
            if (search)
            {
                path = scene;
                break;
            }
        }

        if (!search)
        {

            string[] searchScenes = UnityEditor.AssetDatabase.FindAssets("TestLogin");
            if (searchScenes.Length != 1)
            {
                UnityEngine.Debug.LogError("存在多个重名");
            }


            foreach (string scene in searchScenes)
            {
                if (System.IO.Path.GetExtension(UnityEditor.AssetDatabase.GUIDToAssetPath(scene)).Equals(".unity", StringComparison.OrdinalIgnoreCase))
                {
                    path = UnityEditor.AssetDatabase.GUIDToAssetPath(scene);
                    UnityEngine.Debug.Log(path);
                }
            }
            if (string.IsNullOrEmpty(path))
            {
                UnityEngine.Debug.LogError("未找到启动场景!");
                return;
            }

            UnityEditor.EditorBuildSettingsScene settingsScene = new UnityEditor.EditorBuildSettingsScene(path, false);
            var array = scenes.ToList();
            array.Add(settingsScene);
            UnityEditor.EditorBuildSettings.scenes = array.ToArray();

        }

        UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += OnUnityOpen;

        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);

    }

    private static void OnUnityOpen(UnityEngine.SceneManagement.Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
    {
        UnityEditor.SceneManagement.EditorSceneManager.sceneOpened -= OnUnityOpen;

        UnityEditor.EditorApplication.isPlaying = true;
    }

}
