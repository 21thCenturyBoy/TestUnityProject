using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using TreeView = UnityEditor.IMGUI.Controls.TreeView;

namespace ScratchFramework.Editor
{
    public partial class ScratchEditorWindow
    {
        public enum HeadStructType
        {
            Label,
            Input,
            RenturnVariableLabel,
            Icon,
        }

        public List<HeadStructType> preHeadStruct_0 = new List<HeadStructType>();
        public List<HeadStructType> preHeadStruct_1 = new List<HeadStructType>();

        public int ControlBranch = 1;

        public class DrawGridWindow
        {
            // private static Vector2 MIN_SIZE = new Vector2(400, 300);
            private static float MIN_ZOOM = 0.4f;
            private static float MAX_ZOOM = 7f;
            private static float mGraphZoomScaler = 0.05f;

            private Color backgroundColor;
            private Color gridColor;
            private Vector2 drag;
            private Vector2 offset;

            private Rect mGraphRect;
            private Vector2 mGraphOffset = Vector2.zero;
            private float mGraphZoom = 1f;

            private bool isDrawWithHandleAPI = true;

            public void Init()
            {
                backgroundColor = new Color(0.4f, 0.4f, 0.4f);
                gridColor = new Color(0.1f, 0.1f, 0.1f);
            }

            public void DrawGUI(Rect rect)
            {
                mGraphRect = rect;
                // ProcessEvents(Event.current);
                DrawBackground();
                DrawGrid(10 * mGraphZoom, 0.2f);
                DrawGrid(50 * mGraphZoom, 0.4f);
            }

            private void DrawBackground()
            {
                EditorGUI.DrawRect(mGraphRect, backgroundColor);
            }

            private void DrawGrid(float gridSpacing, float gridOpacity)
            {
                int widthDivs = Mathf.CeilToInt(mGraphRect.width / gridSpacing);
                int heightDivs = Mathf.CeilToInt(mGraphRect.height / gridSpacing);
                Handles.BeginGUI();
                Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);
                offset += drag * 0.5f;
                Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);
                for (int i = 0; i < widthDivs; i++)
                {
                    Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset,
                        new Vector3(gridSpacing * i, mGraphRect.height, 0f) + newOffset);
                }

                for (int j = 0; j < heightDivs; j++)
                {
                    Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset,
                        new Vector3(mGraphRect.width, gridSpacing * j, 0f) + newOffset);
                }

                Handles.color = Color.white;
                Handles.EndGUI();
            }

            private void ProcessEvents(Event e)
            {
                drag = Vector2.zero;
                switch (e.type)
                {
                    case EventType.MouseDrag:
                        if (e.button == 0)
                        {
                            OnDrag(e.delta);
                        }

                        break;
                    case EventType.ScrollWheel:
                        OnMouseZoom();
                        break;
                }
            }

            private void OnDrag(Vector2 delta)
            {
                drag = delta;
                GUI.changed = true;
            }

            private void OnMouseZoom()
            {
                mGraphZoom += mGraphZoomScaler * Event.current.delta.y;
                mGraphZoom = Mathf.Clamp(mGraphZoom, MIN_ZOOM, MAX_ZOOM);
                Event.current.Use();
            }


            private void DrawLine(Vector2 p1, Vector2 p2)
            {
                GL.Vertex(p1);
                GL.Vertex(p2);
            }
        }

        public class BlockBrowser : MenuTreeWindow<BlockBrowser>
        {
            class BlockAssetTreeView : TreeView
            {
                public event Action<int> onContextClickedItem;
                public event Action<int> onSingleClickedItem;
                public event Action<int> onDoubleClickedItem;

                public BlockAssetTreeView(TreeViewState treeViewState) : base(treeViewState)
                {
                    Reload();
                }

                protected override TreeViewItem BuildRoot()
                {
                    var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

                    var allItems = new List<TreeViewItem>();

                    for (int i = 0; i < ScratchConfig.Instance.TemplateDatas.Count; i++)
                    {
                        var item = new TreeViewItem
                        {
                            id = i + 1,
                            depth = 0,
                            displayName = ScratchConfig.Instance.TemplateDatas[i].name
                        };
                        allItems.Add(item);
                    }

                    // Utility method that initializes the TreeViewItem.children and -parent for all items.
                    SetupParentsAndChildrenFromDepths(root, allItems);

                    // Return root of the tree
                    return root;
                }

                protected override void SingleClickedItem(int id)
                {
                    base.SingleClickedItem(id);
                    onSingleClickedItem?.Invoke(id);
                }

                protected override void DoubleClickedItem(int id)
                {
                    base.DoubleClickedItem(id);
                    onDoubleClickedItem?.Invoke(id);
                }

                protected override void ContextClickedItem(int id)
                {
                    base.ContextClickedItem(id);
                    onContextClickedItem?.Invoke(id);
                }
            }

            TreeViewState m_TreeViewState;

            BlockAssetTreeView m_TreeView;
            SearchField m_SearchField;

            public bool Inited = false;
            GUIStyle m_Skin;


            private SerializedProperty serializedProperty_0;
            private SerializedProperty serializedProperty_1;

            public void Init()
            {
                m_TreeViewState = new TreeViewState();
                // EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

                m_TreeView = new BlockAssetTreeView(m_TreeViewState);
                m_SearchField = new SearchField();
                m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;

                m_TreeView.onSingleClickedItem += (id) => { PreViewData = Serialize(id - 1); };

                Inited = true;
            }

            public string PreViewData;

            private string Serialize(int id)
            {
                MemoryStream memoryStream = ScratchUtils.CreateMemoryStream(ScratchConfig.Instance.TemplateDatas[id].bytes);

                BlockData newBlockData = new BlockData();
                newBlockData.BlockData_Deserialize(memoryStream, ScratchConfig.Instance.Version, true);
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(newBlockData, Newtonsoft.Json.Formatting.Indented);
                return json;
            }

            void DoToolbar()
            {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label("ScratchConfig of TemplateDatas:");
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


            public override string GetMenuPath() => "Engine/BlockBrowser(Test)";
            private DrawGridWindow _drawGridWindow;

            private FucType preCreateBlockType;
            private string[] preCreateBlockTypeS;

            public enum EditorTabType
            {
                PreViewData,
                CreateBlock,
            }

            private EditorGUILayoutExtensions.EditorTab tab;
            protected int TabIndex;
            private EditorTabType[] items;
            private string[] itemNames;


            private Dictionary<HeadStructType, string> HeadPrefab = new Dictionary<HeadStructType, string>()
            {
                { HeadStructType.Label, "Prefab_Label" },
                { HeadStructType.Input, "Prefab_Input" },
                { HeadStructType.RenturnVariableLabel, "Prefab_ReturnVariableLabel" },
                { HeadStructType.Icon, "Prefab_Icon" },
            };


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

            private void ShowEditorTabType(EditorTabType tabType)
            {
                switch (tabType)
                {
                    case EditorTabType.PreViewData:
                        PreViewData = GUILayout.TextArea(PreViewData, GUILayout.ExpandHeight(true));
                        break;
                    case EditorTabType.CreateBlock:
                        
                        serializedProperty_0 = serializedObject.FindProperty(nameof(preHeadStruct_0));
                        serializedProperty_1 = serializedObject.FindProperty(nameof(preHeadStruct_0));
                        if (ScratchEditorWindow.Instance.ControlBranch == 1)
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(serializedProperty_0, true);
                            if (EditorGUI.EndChangeCheck())
                            {
                                serializedObject.ApplyModifiedProperties();
                            }
                        }
                        else if (ScratchEditorWindow.Instance.ControlBranch == 2)
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(serializedProperty_0, true);
                            if (EditorGUI.EndChangeCheck())
                            {
                                serializedObject.ApplyModifiedProperties();
                            }

                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(serializedProperty_1, true);
                            if (EditorGUI.EndChangeCheck())
                            {
                                serializedObject.ApplyModifiedProperties();
                            }
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(tabType), tabType, null);
                }
            }


            public override void ShowGUI()
            {
                if (!Inited)
                {
                    Init();
                }

                if (_drawGridWindow == null)
                {
                    _drawGridWindow = new DrawGridWindow();
                    _drawGridWindow.Init();
                }

                if (preCreateBlockTypeS == null)
                {
                    preCreateBlockTypeS = Enum.GetNames(typeof(FucType));
                }

                if (m_Skin == null)
                {
                    m_Skin = new GUIStyle("TreeNode");
                }

                var rect = horizontalSplitView.BeginSplitView();
                _drawGridWindow.DrawGUI(rect);
                DrawCreateBlock(Vector2.one * 40, rect.size);
                rect = horizontalSplitView.Split();
                verticalSplitView.BeginSplitView();
                ShowTab();
                verticalSplitView.Split();
                DoToolbar();
                DoTreeView();

                m_TreeView.Reload();
                verticalSplitView.EndSplitView();
                horizontalSplitView.EndSplitView();
            }

            EditorGUISplitView horizontalSplitView = new EditorGUISplitView(EditorGUISplitView.Direction.Horizontal);
            EditorGUISplitView verticalSplitView = new EditorGUISplitView(EditorGUISplitView.Direction.Vertical);


            List<int> m_SelectedIDs = new List<int>();

            void DrawCreateBlock(Vector2 pos, Vector2 size)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Create Block", GUILayout.MaxWidth(100)))
                {
                }

                preCreateBlockType = (FucType)EditorGUILayout.Popup("FucType", (int)preCreateBlockType, preCreateBlockTypeS, GUILayout.MaxWidth(300));

                Sprite sprite = null;
                Color color = Color.grey;
                switch (preCreateBlockType)
                {
                    case FucType.Undefined:
                        break;
                    case FucType.Event:
                        sprite = ScratchConfig.Instance.Trigger_Header;
                        color = ScratchConfig.Instance.BlockColor_Event;
                        ScratchEditorWindow.Instance.ControlBranch = 1;
                        break;
                    case FucType.Action:
                        sprite = ScratchConfig.Instance.Simple_Header;
                        color = ScratchConfig.Instance.BlockColor_Action;
                        ScratchEditorWindow.Instance.ControlBranch = 1;
                        break;
                    case FucType.Control:
                        sprite = ScratchConfig.Instance.Condition_RoundConditional;
                        color = ScratchConfig.Instance.BlockColor_Control;
                        ScratchEditorWindow.Instance.ControlBranch = EditorGUILayout.IntSlider("ControlBranch:", ScratchEditorWindow.Instance.ControlBranch, 1, 2);
                        break;
                    case FucType.Condition:
                        sprite = ScratchConfig.Instance.Condition_Header;
                        color = ScratchConfig.Instance.BlockColor_Condition;
                        ScratchEditorWindow.Instance.ControlBranch = 1;
                        break;
                    case FucType.GetValue:
                        sprite = ScratchConfig.Instance.Operation_Header;
                        color = ScratchConfig.Instance.BlockColor_GetValue;
                        ScratchEditorWindow.Instance.ControlBranch = 1;
                        break;
                    case FucType.Variable:
                        sprite = ScratchConfig.Instance.Operation_Header;
                        color = ScratchConfig.Instance.BlockColor_Variable;
                        ScratchEditorWindow.Instance.ControlBranch = 1;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                GUILayout.EndHorizontal();

                // float ratio = size.x / size.y;
                if (sprite != null)
                {
                    var texture2D = sprite.texture;
                    m_Skin.border = new RectOffset((int)sprite.border.x, (int)sprite.border.y, (int)sprite.border.z, (int)sprite.border.w);
                    m_Skin.normal.background = texture2D;
                    // GUI.Box(new Rect(pos.x, pos.y, texture2D.width, texture2D.height), "Text", m_Skin);

                    GUI.Box(new Rect(pos.x, pos.y, texture2D.width, texture2D.height), "", m_Skin);
                }

                // GUI.DrawTexture(new Rect(pos.x, pos.y, texture.width, texture.height),);
                //
                // if (texture != null)
                // {
                //     GUI.DrawTexture(new Rect(pos.x, pos.y, texture.width, texture.height), texture, ScaleMode.StretchToFill, true, 0, color, 0, 0);
                // }
            }
        }
    }
}