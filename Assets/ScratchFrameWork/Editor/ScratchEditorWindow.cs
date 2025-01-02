using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ScratchFramework.Editor
{
    public partial class ScratchEditorWindow : BasicMenuEditorWindow
    {
        private static ScratchEditorWindow m_Instance;
        public static ScratchEditorWindow Instance => m_Instance;

        private static SerializedObject serializedObject;

        public static List<IMenuTreeWindow> ScratchEditorMenuList = new List<IMenuTreeWindow>()
        {
            Database.Instance,
            ScratchBlockTreeViewWindows.Instance,
            Config.Instance,
            Variable.Instance,
            BlockBrowser.Instance,
            LogicTree.Instance,
            CanvasData.Instance,
        };

        [MenuItem("Tools/Scratch/EditorWindow")]
        public static void OpenWindow()
        {
            if (m_Instance == null)
            {
                m_Instance = GetWindow<ScratchEditorWindow>();
            }

            Instance.Show();
            serializedObject = new SerializedObject(m_Instance);
        }

        private void Update()
        {
            if (m_Instance != this)
            {
                OpenWindow();
            }

            IList<int> selection = menuTreeView.GetSelection();
            if (selection.Count > 0)
            {
                CustomMenuTreeViewItem treeViewItem = menuTreeView.Find(selection[0]) as CustomMenuTreeViewItem;
                if (treeViewItem != null)
                {
                    if (treeViewItem.CustomWindow == null) return;

                    if (treeViewItem.CustomWindow.IsFrameUpdate())
                    {
                        Repaint();
                    }
                }
            }
        }

        protected override void OnRightGUI(CustomMenuTreeViewItem _selectedItem)
        {
            _selectedItem?.CustomWindow?.ShowGUI();
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
            bool IsFrameUpdate();
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
            public virtual bool IsFrameUpdate() => false;
        }
    }
}