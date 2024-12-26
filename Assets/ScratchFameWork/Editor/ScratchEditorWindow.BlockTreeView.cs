using System;
using System.Collections.Generic;
using ScratchFramework;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ScratchFramework.Editor
{
    public class EditorBlockTree
    {
        public Guid BlockGuid;

        public List<BlockTreeNode> BlockTreeNode = new List<BlockTreeNode>();

        public int Index { get; set; }
        public int Depth { get; set; }

        public string DisplayName
        {
            get
            {
                string rex = IsRoot ? "" : Header ? "[H]" : "[B]";
                return $"{rex}{BlockCanvasUIManager.Instance.BlockDict[BlockGuid].name}";
            }
        }

        public bool IsRoot;
        public bool Header;
    }

    public class BlockTreeNode
    {
        public string DisplayName => nameof(BlockTreeNode) + SectionIndex.ToString();
        public int SectionIndex;

        public List<EditorBlockTree> HeadBlocks = new List<EditorBlockTree>();
        public List<EditorBlockTree> BodyBlocks = new List<EditorBlockTree>();
    }

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

                List<EditorBlockTree> trees = new List<EditorBlockTree>();
                var allItems = new List<TreeViewItem>();
                if (Application.isPlaying)
                {
                    BlockCanvasUIManager canvasManager = GameObject.FindObjectOfType<BlockCanvasUIManager>();

                    if (canvasManager != null && canvasManager.Inited)
                    {
                        trees = GetBlockTree();
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

            private void GetAllTreeview(ref List<TreeViewItem> allTreeViewItems, ref int index, ref int depthVal, EditorBlockTree tree)
            {
                var block = BlockCanvasUIManager.Instance.BlockDict[tree.BlockGuid];
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

            private List<EditorBlockTree> GetBlockTree()
            {
                List<EditorBlockTree> trees = new List<EditorBlockTree>();
                BlockCanvasUIManager blockCanvasUI = FindObjectOfType<BlockCanvasUIManager>();
                if (blockCanvasUI == null) return trees;

                var transform = blockCanvasUI.transform;
                trees.Clear();
                int childCount = transform.childCount;
                int index = 0;
                int deep = 0;
                for (int i = 0; i < childCount; i++)
                {
                    Block block = transform.GetChild(i).GetComponent<Block>();

                    if (block != null && block.Active && block.Visible)
                    {
                        EditorBlockTree tree = new EditorBlockTree();
                        tree.BlockGuid = block.BlockId;
                        tree.Depth = deep;
                        tree.Index = index;
                        tree.Header = false;
                        tree.IsRoot = true;

                        GetBlockDeep(tree, deep + 1);
                        trees.Add(tree);
                        index++;
                    }
                }

                return trees;
            }

            private void GetBlockDeep(EditorBlockTree editorBlock, int deep)
            {
                var blockDict = BlockCanvasUIManager.Instance.BlockDict;
                var sections = blockDict[editorBlock.BlockGuid].Layout.SectionsArray;
                for (int j = 0; j < sections.Length; j++)
                {
                    BlockTreeNode treeNode = new BlockTreeNode();
                    treeNode.SectionIndex = j;
                    editorBlock.BlockTreeNode.Add(treeNode);
                    if (sections[j].Header != null)
                    {
                        int headChildCount = sections[j].Header.transform.childCount;
                        int index_head = 0;
                        for (int k = 0; k < headChildCount; k++)
                        {
                            Block childBlock = sections[j].Header.transform.GetChild(k).GetComponent<Block>();
                            if (childBlock != null)
                            {
                                EditorBlockTree editorBlockTreeHeadEditorBlock = new EditorBlockTree();
                                editorBlockTreeHeadEditorBlock.BlockGuid = childBlock.BlockId;
                                editorBlockTreeHeadEditorBlock.Depth = deep;
                                editorBlockTreeHeadEditorBlock.Index = index_head;
                                editorBlockTreeHeadEditorBlock.Header = true;

                                treeNode.HeadBlocks.Add(editorBlockTreeHeadEditorBlock);

                                GetBlockDeep(editorBlockTreeHeadEditorBlock, deep + 1);

                                index_head++;
                            }
                        }
                    }

                    if (sections[j].Body != null)
                    {
                        int bodyChildCount = sections[j].Body.transform.childCount;
                        int index_body = 0;

                        for (int k = 0; k < bodyChildCount; k++)
                        {
                            Block childBlock = sections[j].Header.transform.GetChild(k).GetComponent<Block>();
                            if (childBlock != null)
                            {
                                index_body++;
                                EditorBlockTree editorBlockTreeBodyEditorBlock = new EditorBlockTree();
                                editorBlockTreeBodyEditorBlock.BlockGuid = childBlock.BlockId;
                                editorBlockTreeBodyEditorBlock.Depth = deep;
                                editorBlockTreeBodyEditorBlock.Index = index_body;
                                editorBlockTreeBodyEditorBlock.Header = false;

                                treeNode.BodyBlocks.Add(editorBlockTreeBodyEditorBlock);

                                GetBlockDeep(editorBlockTreeBodyEditorBlock, deep + 1);
                            }
                        }
                    }
                }
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
                Rect rect = GUILayoutUtility.GetRect(0, 500, 0, 500);
                m_TreeView.OnGUI(rect);
            }

            public override string GetMenuPath() => "UI Runtime/BlockTreeView";

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