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


        public class EditorGUISplitView
        {
            public enum Direction
            {
                Horizontal,
                Vertical
            }

            Direction splitDirection;
            float splitNormalizedPosition;
            bool resize;
            public Vector2 scrollPosition;
            Rect availableRect;


            public EditorGUISplitView(Direction splitDirection)
            {
                splitNormalizedPosition = 0.5f;
                this.splitDirection = splitDirection;
            }

            public Rect BeginSplitView()
            {
                Rect tempRect;

                if (splitDirection == Direction.Horizontal)
                    tempRect = EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                else
                    tempRect = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));

                if (tempRect.width > 0.0f)
                {
                    availableRect = tempRect;
                }

                if (splitDirection == Direction.Horizontal)
                {
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(availableRect.width * splitNormalizedPosition));
                    return new Rect(0f, 0f, availableRect.width * splitNormalizedPosition, tempRect.height);
                }
                else
                {
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(availableRect.height * splitNormalizedPosition));
                    return new Rect(0f, 0f, tempRect.width, availableRect.height * splitNormalizedPosition);
                }
            }

            public Rect Split()
            {
                GUILayout.EndScrollView();
                return ResizeSplitFirstView();
            }

            public void EndSplitView()
            {
                if (splitDirection == Direction.Horizontal)
                    EditorGUILayout.EndHorizontal();
                else
                    EditorGUILayout.EndVertical();
            }

            private Rect ResizeSplitFirstView()
            {
                Rect resizeHandleRect;

                if (splitDirection == Direction.Horizontal)
                    resizeHandleRect = new Rect(availableRect.width * splitNormalizedPosition, availableRect.y, 2f, availableRect.height);
                else
                    resizeHandleRect = new Rect(availableRect.x, availableRect.height * splitNormalizedPosition, availableRect.width, 2f);

                GUI.DrawTexture(resizeHandleRect, EditorGUIUtility.whiteTexture);

                if (splitDirection == Direction.Horizontal)
                    EditorGUIUtility.AddCursorRect(resizeHandleRect, MouseCursor.ResizeHorizontal);
                else
                    EditorGUIUtility.AddCursorRect(resizeHandleRect, MouseCursor.ResizeVertical);

                if (Event.current.type == EventType.MouseDown && resizeHandleRect.Contains(Event.current.mousePosition))
                {
                    resize = true;
                }

                if (resize)
                {
                    if (splitDirection == Direction.Horizontal)
                        splitNormalizedPosition = Event.current.mousePosition.x / availableRect.width;
                    else
                        splitNormalizedPosition = Event.current.mousePosition.y / availableRect.height;
                }

                if (Event.current.type == EventType.MouseUp)
                    resize = false;
                return resizeHandleRect;
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
                Rect rect = GUILayoutUtility.GetRect(0, 100, 0, 100);
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