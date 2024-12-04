using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.IMGUI.Controls;

namespace ScratchFramework.Editor
{
    public partial class ScratchEditorWindow
    {
        public class LogicTree : MenuTreeWindow<LogicTree>
        {
            class LogicTreeView : TreeView
            {
                public LogicTreeView(TreeViewState treeViewState) : base(treeViewState)
                {
                    Reload();
                }

                private int Add(List<TreeViewItem> allItems, int index, bool isGlobal = false)
                {
                    Dictionary<int, IEngineBlockBaseData> rootDict = isGlobal ? ScratchEngine.Instance.CurrentGroup.GlobalCanvas.RootBlock : ScratchEngine.Instance.Current.RootBlock;
                    foreach (KeyValuePair<int, IEngineBlockBaseData> valuePair in rootDict)
                    {
                        ScratchUtils.GetBlockDataTree(valuePair.Value.Guid, out var tree, null, false);

                        tree.TraverseTree((deep, bNode) =>
                        {
                            index++;
                            IEngineBlockBaseData baseData =  ScratchEngine.Instance.Current[bNode.Value];

                            string icon = null;
                            string name = $"[{bNode.Value}]";
                            if (baseData != null)
                            {
                                name += baseData.Type.ToString();
                                switch (baseData.FucType)
                                {
                                    case FucType.Undefined:
                                        icon = "sv_icon_dot8_pix16_gizmo";
                                        break;
                                    case FucType.Event:
                                        icon = "sv_icon_dot12_pix16_gizmo";
                                        break;
                                    case FucType.Action:
                                        icon = "sv_icon_dot9_pix16_gizmo";
                                        break;
                                    case FucType.Control:
                                        icon = "sv_icon_dot15_pix16_gizmo";
                                        break;
                                    case FucType.Condition:
                                        icon = "sv_icon_dot10_pix16_gizmo";
                                        break;
                                    case FucType.GetValue:
                                        icon = "sv_icon_dot14_pix16_gizmo";
                                        break;
                                    case FucType.Variable:
                                        icon = "sv_icon_dot11_pix16_gizmo";
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }
                            else
                            {
                                icon = "sv_icon_dot8_pix16_gizmo";
                            }

                            var item = new TreeViewItem
                            {
                                id = index,
                                depth = isGlobal ? deep + 1 : deep,
                                displayName = name,
                                icon = EditorGUIUtility.FindTexture(icon)
                            };
                            allItems.Add(item);

                            return true;
                        });
                    }

                    return index;
                }

                protected override TreeViewItem BuildRoot()
                {
                    var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

                    List<BlockTree> trees = new List<BlockTree>();
                    var allItems = new List<TreeViewItem>();
                    if (Application.isPlaying)
                    {
                        if (ScratchEngine.Instance.Current != null)
                        {
                            int id = 1;
                            var global = new TreeViewItem { id = id, depth = 0, displayName = "[Global]" };
                            allItems.Add(global);
                            id = Add(allItems, id, true);
                            if (!ScratchEngine.Instance.CurrentIsGlobal)
                            {
                                id = Add(allItems, id);
                            }
                        }
                    }

                    // Utility method that initializes the TreeViewItem.children and -parent for all items.
                    SetupParentsAndChildrenFromDepths(root, allItems);

                    // Return root of the tree
                    return root;
                }
            }

            TreeViewState m_TreeViewState;

            LogicTreeView m_TreeView;
            SearchField m_SearchField;

            public bool Inited = false;

            public void Init()
            {
                m_TreeViewState = new TreeViewState();
                // EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

                m_TreeView = new LogicTreeView(m_TreeViewState);
                m_SearchField = new SearchField();
                m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;

                Inited = true;
            }

            public override string GetMenuPath() => "Engine/LogicTree";

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