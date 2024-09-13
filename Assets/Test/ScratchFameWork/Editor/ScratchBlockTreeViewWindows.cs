using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ScratchFramework.Editor
{
    class SimpleTreeView : TreeView
    {
        public SimpleTreeView(TreeViewState treeViewState) : base(treeViewState)
        {
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

            List<BlockTree> trees = new List<BlockTree>();
            var allItems = new List<TreeViewItem>();
            if (Application.isPlaying)
            {
                // BlockCanvasManager canvasManager = GameObject.FindObjectOfType<BlockCanvasManager>();
                //
                // if (canvasManager != null && canvasManager.Inited)
                // {
                //     trees = GameObject.FindObjectOfType<BlockCanvasManager>().GetBlockTree();
                //     int idIndex = 1;
                //     int depthVal = 0;
                //
                //
                //     for (int i = 0; i < trees.Count; i++)
                //     {
                //         var item = new TreeViewItem
                //         {
                //             id = idIndex,
                //             depth = depthVal,
                //             displayName = trees[i].DisplayName
                //         };
                //         idIndex++;
                //         allItems.Add(item);
                //     }
                // }
            }

            // var allItems = new List<TreeViewItem>
            // {
            //     new TreeViewItem { id = 1, depth = 0, displayName = "Animals" },
            //     new TreeViewItem { id = 2, depth = 1, displayName = "Mammals" },
            //     new TreeViewItem { id = 3, depth = 2, displayName = "Tiger" },
            //     new TreeViewItem { id = 4, depth = 2, displayName = "Elephant" },
            //     new TreeViewItem { id = 5, depth = 2, displayName = "Okapi" },
            //     new TreeViewItem { id = 6, depth = 2, displayName = "Armadillo" },
            //     new TreeViewItem { id = 7, depth = 1, displayName = "Reptiles" },
            //     new TreeViewItem { id = 8, depth = 2, displayName = "Crocodile" },
            //     new TreeViewItem { id = 9, depth = 2, displayName = "Lizard" },
            // };

            // Utility method that initializes the TreeViewItem.children and -parent for all items.
            SetupParentsAndChildrenFromDepths(root, allItems);

            // Return root of the tree
            return root;
        }
    }

    public class ScratchBlockTreeViewWindows : EditorWindow
    {
        TreeViewState m_TreeViewState;

        SimpleTreeView m_TreeView;
        SearchField m_SearchField;

        private bool m_Inited = false;

        private void OnEnable()
        {
        }

        // private void OnPlayModeStateChanged(PlayModeStateChange change)
        // {
        //     m_TreeView.MointerCanvas = null;
        // }
        //
        // private void OnDisable()
        // {
        //     EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        // }

        public void OnGUI()
        {
            if (!m_Inited) return;

            DoToolbar();
            DoTreeView();
        }
        

        private void OnInspectorUpdate()
        {
            if (Application.isPlaying)
            {
                if (!m_Inited)
                {
                    m_TreeViewState = new TreeViewState();

                    // EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

                    m_TreeView = new SimpleTreeView(m_TreeViewState);
                    m_SearchField = new SearchField();
                    m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;

                    m_Inited = true;
                }

                m_TreeView.Reload();
            }
            else
            {
                m_Inited = false;
            }
        }

        void DoToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(100);
            GUILayout.FlexibleSpace();
            m_TreeView.searchString = m_SearchField.OnToolbarGUI(m_TreeView.searchString);
            GUILayout.EndHorizontal();
        }

        void DoTreeView()
        {
            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);
            m_TreeView.OnGUI(rect);
        }

        [MenuItem("Tools/Scratch/Tree Window")]
        static void ShowWindow()
        {
            // Get existing open window or if none, make a new one:
            var window = GetWindow<ScratchBlockTreeViewWindows>();
            window.titleContent = new GUIContent("ScratchBlockTreeViewWindows");
            window.Show();
        }
    }
}