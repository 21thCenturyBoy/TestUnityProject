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
                // var blockTrees = BlockCanvasManager.Instance.GetBlockTree();
                //
                // for (int i = 0; i < blockTrees.Count; i++)
                // {
                //     ShowBlcok(blockTrees[i]);
                // }

                foreach (var block in BlockCanvasManager.Instance.BlockDict.Values)
                {
                    GUILayout.Label($"{block}");
                }
                GUILayout.Label($"--------------------------");
                foreach (ScratchVMData vmData in ScratchDataManager.Instance.DataDict.Values)
                {
                    GUILayout.Label($"{vmData}");
                }
                GUILayout.Label($"--------------------------");
                var inputs = GameObject.FindObjectsOfType<BlockHeaderItem_Input>();
                foreach (var input in inputs)
                {
                    GUILayout.Label($"[{input.ContextData.IdPtr}][{input.ContextData.ChildOperation}]");
                }
                
                GUILayout.Label($"--------------------------");
                var ScratchUIBehaviours = GameObject.FindObjectsOfType<ScratchUIBehaviour>(true);
                foreach (var input in ScratchUIBehaviours)
                {
                    if (input is IBlockScratch_Head inputHead)
                    {
                        if (inputHead.DataRef() == null)
                        {
                            GUILayout.Label($"NULL[{input.gameObject.name}]");
                        }
                        else
                        {
                            GUILayout.Label($"[{input.gameObject.name}][{inputHead.DataRef().ToString()}]");
                        }
                    }
     
                }

                GUILayout.EndVertical();
                // RuntimeDatabaseOnGUI();
            }
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
            menuTree.AddMenuItem(ScratchEditorMenuType[0], new CustomMenuTreeViewItem()
            {
                // userData = Resources.FindObjectsOfTypeAll<PlayerSettings>().FirstOrDefault()
            });
            return menuTree;
        }
    }
}