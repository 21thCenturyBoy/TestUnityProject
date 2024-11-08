using System;
using System.Collections.Generic;
using ScratchFramework.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ScratchFramework.Editor
{
    public partial class ScratchEditorWindow : BasicMenuEditorWindow
    {
        private static ScratchEditorWindow m_Instance;
        public static ScratchEditorWindow Instance => m_Instance;

        public static List<IMenuTreeWindow> ScratchEditorMenuList = new List<IMenuTreeWindow>()
        {
            Database.Instance,
            ScratchBlockTreeViewWindows.Instance,
            Config.Instance,
        };

        [MenuItem("Tools/Scratch/EditorWindow")]
        public static void window()
        {
            if (m_Instance == null)
            {
                m_Instance = GetWindow<ScratchEditorWindow>();
            }

            Instance.Show();
        }


        private void OnInspectorUpdate()
        {
            Repaint();
        }

        protected override void OnRightGUI(CustomMenuTreeViewItem _selectedItem)
        {
            _selectedItem?.CustomWindow?.ShowGUI();
        }

        private void ShowBlcok(BlockTree tree)
        {
            int orginDep = EditorGUI.indentLevel;
            EditorGUI.indentLevel = tree.Depth;
            GUILayout.Label($"{tree}");
            for (int j = 0; j < tree.BlockTreeNode.Count; j++)
            {
                EditorGUI.indentLevel++;
                GUILayout.Label($"{tree.BlockTreeNode[j]}");
                for (int k = 0; k < tree.BlockTreeNode[j].HeadBlocks.Count; k++)
                {
                    EditorGUI.indentLevel++;
                    ShowBlcok(tree.BlockTreeNode[j].HeadBlocks[k]);
                    EditorGUI.indentLevel--;
                }

                for (int k = 0; k < tree.BlockTreeNode[j].BodyBlocks.Count; k++)
                {
                    EditorGUI.indentLevel++;
                    ShowBlcok(tree.BlockTreeNode[j].BodyBlocks[k]);
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel = orginDep;
        }

        protected override CustomMenuTreeView BuildMenuTree(TreeViewState _treeViewState)
        {
            CustomMenuTreeView menuTree = new CustomMenuTreeView(_treeViewState);

            for (int i = 0; i < ScratchEditorMenuList.Count; i++)
            {
                var menuTreeWindow = ScratchEditorMenuList[i];

                menuTree.AddMenuItem(menuTreeWindow.GetMenuPath(), new CustomMenuTreeViewItem()
                {
                    CustomWindow = menuTreeWindow
                });
            }

            return menuTree;
        }


        public interface IMenuTreeWindow
        {
            string GetMenuPath();
            void ShowGUI();
        }

        public abstract class MenuTreeWindow<T> : IMenuTreeWindow where T : MenuTreeWindow<T>, new()
        {
            private static T _instance;

            protected MenuTreeWindow()
            {
            }

            public static T Instance
            {
                get { return _instance ??= new T(); }
            }

            public static T FindInstance() => _instance;
            public abstract string GetMenuPath();
            public abstract void ShowGUI();
        }
    }
}