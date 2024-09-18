using System.Collections.Generic;
using ScratchFramework;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ScratchFramework.Editor
{
    public partial class ScratchEditorWindow
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
                    BlockCanvasManager canvasManager = GameObject.FindObjectOfType<BlockCanvasManager>();

                    if (canvasManager != null && canvasManager.Inited)
                    {
                        trees = GameObject.FindObjectOfType<BlockCanvasManager>().GetBlockTree();
                        int idIndex = 0;
                        int depthVal = 0;

                        for (int i = 0; i < trees.Count; i++)
                        {
                            GetAllTreeview(ref allItems, ref idIndex, ref depthVal, trees[i]);
                        }
                    }
                }

                // Utility method that initializes the TreeViewItem.children and -parent for all items.
                SetupParentsAndChildrenFromDepths(root, allItems);

                // Return root of the tree
                return root;
            }

            private void GetAllTreeview(ref List<TreeViewItem> allTreeViewItems, ref int index, ref int depthVal, BlockTree tree)
            {
                var block = BlockCanvasManager.Instance.BlockDict[tree.BlockGuid];
                index++;
                var item = new TreeViewItem
                {
                    id = index,
                    depth = depthVal,
                    displayName = tree.DisplayName
                };
                allTreeViewItems.Add(item);
                depthVal++;
                for (int i = 0; i < tree.BlockTreeNode.Count; i++)
                {
                    var treeNode = tree.BlockTreeNode[i];
                    index++;
                    var treeNodeItem = new TreeViewItem
                    {
                        id = index,
                        depth = depthVal,
                        displayName = treeNode.DisplayName
                    };
                    allTreeViewItems.Add(treeNodeItem);
                    depthVal++;
                    for (int j = 0; j < treeNode.HeadBlocks.Count; j++)
                    {
                        GetAllTreeview(ref allTreeViewItems, ref index, ref depthVal, treeNode.HeadBlocks[j]);
                    }
                    depthVal--;
                    
                    depthVal++;
                    for (int j = 0; j < treeNode.BodyBlocks.Count; j++)
                    {
                        GetAllTreeview(ref allTreeViewItems, ref index, ref depthVal, treeNode.BodyBlocks[j]);
                    }
                    depthVal--;
                }
                depthVal--;
                
            }
        }

        public class ScratchBlockTreeViewWindows : MenuTreeWindow<ScratchBlockTreeViewWindows>
        {
            TreeViewState m_TreeViewState;

            SimpleTreeView m_TreeView;
            SearchField m_SearchField;

            public bool Inited = false;

            public void Init()
            {
                m_TreeViewState = new TreeViewState();
                // EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

                m_TreeView = new SimpleTreeView(m_TreeViewState);
                m_SearchField = new SearchField();
                m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;

                Inited = true;
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

            public override string GetMenuPath() => "Runtime/BlockTreeView";

            public override void ShowGUI()
            {
                if (EditorApplication.isPlaying)
                {
                    if (!Inited)
                    {
                        Init();
                    }
                }
                else
                {
                    return;
                }

                DoToolbar();
                DoTreeView();

                m_TreeView.Reload();
            }
        }
    }
}