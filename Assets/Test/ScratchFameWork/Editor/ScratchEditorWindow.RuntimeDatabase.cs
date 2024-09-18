using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScratchFramework.Editor
{
    public partial class ScratchEditorWindow
    {
        public class Database : MenuTreeWindow<Database>
        {
            public override string GetMenuPath() => "Runtime/Database";

            public override void ShowGUI()
            {
                if (!EditorApplication.isPlaying) return;

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
            }
        }
    }
}