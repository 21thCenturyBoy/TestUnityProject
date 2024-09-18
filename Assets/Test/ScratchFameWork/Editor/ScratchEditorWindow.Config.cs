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