using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScratchFramework.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ScratchFramework
{
    public class ScratchEditorWindow : BasicMenuEditorWindow
    {
        private static ScratchEditorWindow m_Instance;
        public static ScratchEditorWindow Instance => m_Instance;


        public static string[] ScratchEditorMenuType = new[]
        {
            "Runtime Database"
        };


        [MenuItem("Tools/Scratch/EditorWindow")]
        static void window()
        {
            if (m_Instance == null)
            {
                m_Instance = GetWindow<ScratchEditorWindow>();
            }

            Instance.Show();
        }


        protected override void OnRightGUI(CustomMenuTreeViewItem _selectedItem)
        {
            if (_selectedItem.displayName == ScratchEditorMenuType[0])
            {
                GUILayout.BeginVertical();
                foreach (var vmData in BlockCanvasManager.Instance.BlockDict.Values)
                {
                    GUILayout.Label($"{vmData}");
                }

                GUILayout.EndVertical();
                // RuntimeDatabaseOnGUI();
            }
        }

        private void RuntimeDatabaseOnGUI()
        {
            if (!EditorApplication.isPlaying) return;

            GUILayout.BeginVertical();

            foreach (ScratchVMData vmData in ScratchDataManager.Instance.DataDict.Values)
            {
                GUILayout.Label($"[{vmData.IdPtr}]{vmData}");
            }

            GUILayout.Label($"--------------------------");

            var inputs = GameObject.FindObjectsOfType<BlockHeaderItem_Input>();

            foreach (var input in inputs)
            {
                GUILayout.Label($"[{input.ContextData.IdPtr}][{input.ContextData.ChildOperation}]");
            }

            GUILayout.EndVertical();
        }

        protected override CustomMenuTreeView BuildMenuTree(TreeViewState _treeViewState)
        {
            CustomMenuTreeView menuTree = new CustomMenuTreeView(_treeViewState);
            menuTree.AddMenuItem(ScratchEditorMenuType[0], new CustomMenuTreeViewItem()
            {
                // userData = Resources.FindObjectsOfTypeAll<PlayerSettings>().FirstOrDefault()
            });
            return menuTree;
        }
    }
}