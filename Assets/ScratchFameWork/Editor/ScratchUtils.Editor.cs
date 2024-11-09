using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace ScratchFramework
{
    public static class ScratchUtils_Editor
    {
        [MenuItem("Assets/Scratch/Convert TemplateDatas", false, 80)]
        public static void Editor_Convert_Datas()
        {
            if (Selection.gameObjects == null) return;

            var dataDicts = ScratchUtils.ConvertSimpleBlock(Selection.gameObjects);
            foreach (var dataDict in dataDicts)
            {
                string path = Path.Combine(ScratchConfig.Instance.TemplateDatasPath, dataDict.Key);
                File.WriteAllBytes($"{path}.bytes", dataDict.Value);
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Scratch/Convert Prefabs", false, 80)]
        public static void Editor_Convert_Prefabs()
        {
            string[] strs = Selection.assetGUIDs;
            if (strs == null || strs.Length == 0) return;

            for (int i = 0; i < strs.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(strs[0]);
                TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                Block block = ScratchUtils.DeserializeBlock(textAsset.bytes);

                string newPath = path.Replace(".bytes", ".prefab");
                var localPath = AssetDatabase.GenerateUniqueAssetPath(newPath);

                bool prefabSuccess;
                PrefabUtility.SaveAsPrefabAsset(block.gameObject, localPath, out prefabSuccess);
                
                GameObject.DestroyImmediate(block.gameObject);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}