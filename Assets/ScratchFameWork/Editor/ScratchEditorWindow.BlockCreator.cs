using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ScratchFramework.Editor
{


    // public partial class ScratchEditorWindow
    // {
    //     public class BlockCreator : MenuTreeWindow<BlockCreator>
    //     {
    //         public enum EditorTabType
    //         {
    //             Create,
    //             RefreshTool,
    //         }
    //
    //         private EditorGUILayoutExtensions.EditorTab tab;
    //         protected int TabIndex;
    //         private EditorTabType[] items;
    //         private string[] itemNames;
    //
    //         private void ShowTab()
    //         {
    //             if (tab == null)
    //             {
    //                 List<EditorTabType> tabTypes = new List<EditorTabType>();
    //                 foreach (var value in Enum.GetValues(typeof(EditorTabType)))
    //                 {
    //                     tabTypes.Add((EditorTabType)value);
    //                 }
    //
    //                 items = tabTypes.ToArray();
    //                 itemNames = items.Select(i => i.ToString()).ToArray();
    //
    //                 tab = new EditorGUILayoutExtensions.EditorTab(itemNames);
    //             }
    //
    //             if (tab.TabItems == null)
    //             {
    //                 tab = new EditorGUILayoutExtensions.EditorTab(itemNames);
    //             }
    //
    //             TabIndex = EditorGUILayoutExtensions.BeginSelectGrouping(tab);
    //
    //             string itemName = tab.TabItems[TabIndex].Name;
    //
    //             var tabType = Enum.Parse<EditorTabType>(itemName);
    //             ShowEditorTabType(tabType);
    //
    //             EditorGUILayoutExtensions.EndSelectGrouping();
    //         }
    //
    //         public UnityEditor.Editor textureEditor;
    //
    //         private void ShowEditorTabType(EditorTabType type)
    //         {
    //             switch (type)
    //             {
    //                 case EditorTabType.Create:
    //                     Show_Create();
    //                     break;
    //                 case EditorTabType.RefreshTool:
    //                     Show_RefreshTool();
    //                     break;
    //                 default:
    //                     throw new ArgumentOutOfRangeException(nameof(type), type, null);
    //             }
    //         }
    //
    //         private string[] blockNames;
    //         private Vector2 blockNameScrolls;
    //
    //         private string tryCreateBlockName;
    //
    //         private void Show_Create()
    //         {
    //             GUILayout.BeginVertical();
    //
    //             blockNames = Enum.GetNames(typeof(ScratchBlockType)).ToArray();
    //
    //             blockNameScrolls = GUILayout.BeginScrollView(blockNameScrolls, false, true);
    //
    //             for (int i = 0; i < blockNames.Length; i++)
    //             {
    //                 GUILayout.Label(blockNames[i]);
    //             }
    //
    //             GUILayout.EndScrollView();
    //
    //             GUILayout.BeginHorizontal();
    //
    //             tryCreateBlockName = GUILayout.TextField(tryCreateBlockName);
    //
    //             if (GUILayout.Button("CreateBlock"))
    //             {
    //                 ScratchEngine.Instance.Core.BlockCreateCSFile(tryCreateBlockName);
    //             }
    //
    //             if (textureEditor != null)
    //             {
    //                 textureEditor.DrawPreview(new Rect(750, 50, 300, 300));
    //             }
    //
    //
    //             GUILayout.EndHorizontal();
    //
    //             GUILayout.EndVertical();
    //         }
    //
    //         private void Show_RefreshTool()
    //         {
    //         }
    //
    //         public void Init()
    //         {
    //         }
    //
    //         public override string GetMenuPath() => "Engine/BlockCreator";
    //
    //         public override void ShowGUI()
    //         {
    //             ShowTab();
    //         }
    //     }
    // }
}