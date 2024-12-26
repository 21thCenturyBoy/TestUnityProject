using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ScratchFramework.Editor
{
    public partial class ScratchEditorWindow
    {
        public class Database : MenuTreeWindow<Database>
        {
            public override string GetMenuPath() => "UI Runtime/Database";


            public enum EditorTabType
            {
                CanvasBlockDict,
                ScratchDataDict,
                HeaderItemInput,
                ScratchUI,
            }

            private EditorGUILayoutExtensions.EditorTab tab;
            protected int TabIndex;
            private EditorTabType[] items;
            private string[] itemNames;

            private void ShowTab()
            {
                if (tab == null)
                {
                    List<EditorTabType> tabTypes = new List<EditorTabType>();
                    foreach (var value in Enum.GetValues(typeof(EditorTabType)))
                    {
                        tabTypes.Add((EditorTabType)value);
                    }

                    items = tabTypes.ToArray();
                    itemNames = items.Select(i => i.ToString()).ToArray();

                    tab = new EditorGUILayoutExtensions.EditorTab(itemNames);
                }

                if (tab.TabItems == null)
                {
                    tab = new EditorGUILayoutExtensions.EditorTab(itemNames);
                }

                TabIndex = EditorGUILayoutExtensions.BeginSelectGrouping(tab);

                string itemName = tab.TabItems[TabIndex].Name;

                var tabType = Enum.Parse<EditorTabType>(itemName);
                ShowEditorTabType(tabType);

                EditorGUILayoutExtensions.EndSelectGrouping();
            }

            private void ShowEditorTabType(EditorTabType type)
            {
                switch (type)
                {
                    case EditorTabType.CanvasBlockDict:
                        Show_CanvasBlockDict();
                        break;
                    case EditorTabType.ScratchDataDict:
                        Show_ScratchDataDict();
                        break;
                    case EditorTabType.HeaderItemInput:
                        Show_HeaderItemInput();
                        break;
                    case EditorTabType.ScratchUI:
                        Show_ScratchUI();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

            private void Show_CanvasBlockDict()
            {
                foreach (var block in BlockCanvasUIManager.Instance.BlockDict.Values)
                {
                    GUILayout.Label($"{block}");
                }
            }

            private void Show_ScratchDataDict()
            {
                foreach (ScratchVMData vmData in BlockDataUIManager.Instance.DataDict.Values)
                {
                    GUILayout.Label($"{vmData}");
                }
            }

            private void Show_HeaderItemInput()
            {
                var inputs = GameObject.FindObjectsOfType<BlockHeaderItem_Input>();

                foreach (var input in inputs)
                {
                    GUILayout.Label($"[{input.ContextData.IdPtr}][{input.ContextData.ChildOperation}]");
                }
            }

            private void Show_ScratchUI()
            {
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
            }

            public override void ShowGUI()
            {
                if (!EditorApplication.isPlaying) return;

                ShowTab();
            }
        }
    }
}