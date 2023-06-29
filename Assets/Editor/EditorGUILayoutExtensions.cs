#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

public partial class EditorGUILayoutExtensions
{
    private static GUIStyle s_TabOnlyOne;
    private static GUIStyle s_TabFirst;
    private static GUIStyle s_TabMiddle;
    private static GUIStyle s_TabLast;

    protected const int kTabButtonHeight = 22;

    protected static GUIStyle m_FrameBox;
    protected static GUIStyle FrameBox
    {
        get
        {
            if (m_FrameBox == null)
            {
                m_FrameBox = GUI.skin.FindStyle("FrameBox") ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("FrameBox");
            }
            return m_FrameBox;
        }
    }

    public class EditorTab
    {
        private Item[] m_TabItems;
        public Item[] TabItems => m_TabItems;

        private Item m_Select;
        public Item SelectTab => m_Select;
        public EditorTab(string[] names)
        {
            m_TabItems = Array.ConvertAll(names, name => new Item(name));
            m_Select = GetDefaultTabItem();
        }

        public Item GetDefaultTabItem()
        {
            if (m_TabItems.Length > 0)
            {
                return m_TabItems[0];
            }
            return null;
        }

        public Item SetDefaultSelect()
        {
            if (m_Select != null)
            {
                m_Select.SetSelected(false);
            }

            m_Select = GetDefaultTabItem();
            if (m_Select != null)
            {
                m_Select.SetSelected(true);
            }
            return m_Select;
        }
        public void SetTabSelectState(int index)
        {
            if (index < 0 || index >= m_TabItems.Length)
            {
                Debug.LogWarning("Index of EditorTab TabItems is error!");
                return;
            }

            for (int i = 0; i < m_TabItems.Length; i++)
            {
                m_TabItems[i].SetSelected(i == index);
            }
        }

        public class Item
        {
            public string Name;

            protected bool m_IsSelected;

            public event Action<bool> OnSelectedBeforeChanged;
            public event Action<bool> OnSelectedChanged;
            public bool IsSelected()
            {
                return m_IsSelected;
            }

            public bool SetSelected(bool select)
            {
                OnSelectedBeforeChanged?.Invoke(select);
                m_IsSelected = select;
                OnSelectedChanged?.Invoke(select);
                return m_IsSelected;
            }

            public Item(string name)
            {
                Name = name;
            }
        }
    }
    public static int BeginSelectGrouping(EditorTab platforms, GUIContent defaultTab = null, GUIStyle style = null)
    {
        int selectedPlatform = -1;
        for (int i = 0; i < platforms.TabItems.Length; i++)
        {
            if (platforms.TabItems[i].IsSelected())
            {
                selectedPlatform = i;
                break;
            }
        }
        if (selectedPlatform == -1)
        {
            platforms.SetDefaultSelect();
            selectedPlatform = 0;
        }

        style ??= FrameBox;

        int selected = selectedPlatform;

        bool tempEnabled = GUI.enabled;
        GUI.enabled = true;
        EditorGUI.BeginChangeCheck();
        Rect r = EditorGUILayout.BeginVertical(style);
        int platformCount = platforms.TabItems.Length;
        int buttonCount = platformCount;
        int startIndex = 0;

        if (defaultTab != null)
        {
            buttonCount++;
            startIndex = -1;
        }

        int buttonIndex = 0;
        for (int i = startIndex; i < platformCount; i++, buttonIndex++)
        {
            GUIContent content = GUIContent.none;

            if (i == -1)
            {
                content = defaultTab;
            }
            else
            {
                content = new GUIContent(platforms.TabItems[i].Name);
            }

            GUIStyle buttonStyle = null;
            Rect buttonRect = GetTabRect(r, buttonIndex, buttonCount, out buttonStyle);

            if (GUI.Toggle(buttonRect, selected == i, content, buttonStyle))
            {
                selected = i;
            }

        }

        GUILayoutUtility.GetRect(10, kTabButtonHeight);

        GUI.enabled = tempEnabled;

        if (EditorGUI.EndChangeCheck())
        {
            if (defaultTab == null)
            {
                platforms.SetTabSelectState(selected);
            }
            else
            {
                if (selected < 0)
                {
                    platforms.SetDefaultSelect();
                }
                else
                {
                    platforms.SetTabSelectState(selected);
                }
            }
        }

        return selected;
    }
    static Rect GetTabRect(Rect rect, int tabIndex, int tabCount, out GUIStyle tabStyle)
    {
        if (s_TabOnlyOne == null)
        {
            s_TabOnlyOne = "Tab onlyOne";
            s_TabFirst = "Tab first";
            s_TabMiddle = "Tab middle";
            s_TabLast = "Tab last";
        }

        tabStyle = s_TabMiddle;

        if (tabCount == 1)
        {
            tabStyle = s_TabOnlyOne;
        }
        else if (tabIndex == 0)
        {
            tabStyle = s_TabFirst;
        }
        else if (tabIndex == (tabCount - 1))
        {
            tabStyle = s_TabLast;
        }

        float tabWidth = rect.width / tabCount;
        int left = Mathf.RoundToInt(tabIndex * tabWidth);
        int right = Mathf.RoundToInt((tabIndex + 1) * tabWidth);
        return new Rect(rect.x + left, rect.y, right - left, kTabButtonHeight);
    }
    public static void EndSelectGrouping()
    {
        EditorGUILayout.EndVertical();
    }
}

#endif