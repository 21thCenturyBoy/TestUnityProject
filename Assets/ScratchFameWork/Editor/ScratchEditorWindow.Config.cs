using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScratchFramework.Editor
{
    public partial class ScratchEditorWindow
    {
        public class Config : MenuTreeWindow<Config>
        {
            private UnityEditor.Editor _editor;

            private static readonly Vector2 MIN_SIZE = new Vector2(400, 400);

            private const string path = "Assets/Test/ScratchFameWork/Resources/ScratchConfig.asset";

            public void Init()
            {
                var asset = AssetDatabase.LoadAssetAtPath<ScratchConfig>(path);
                
                if (null == asset)
                {
                    string[] guids = AssetDatabase.FindAssets("t:" + nameof(ScratchConfig));
                    if (guids.Length>0)
                    {
                        asset = AssetDatabase.LoadAssetAtPath<ScratchConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
                        Debug.LogWarning("存在多个ScratchConfig资源，已选择第一个");
                    }
                    else
                    {
                        asset = ScriptableObject.CreateInstance<ScratchConfig>();
                        AssetDatabase.CreateAsset(asset, path);
                        AssetDatabase.SaveAssets();
                    }
                }
                _editor = UnityEditor.Editor.CreateEditor(asset);
            }

            public override string GetMenuPath() => "Config";
            
            public override void ShowGUI()
            {
    
                if (null != _editor)
                {
                    _editor.OnInspectorGUI();
                }
                else
                {
                    Init();
                }
            }
        }
    }
}