using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ScratchFramework.Editor
{
    [Serializable]
    public abstract class BasicMenuEditorWindow : EditorWindow
    {
        [SerializeField] ResizableArea resizableArea = new ResizableArea();
        protected Rect resizableAreaRect = new Rect(0, 0, 150, 150);

        string searchText;
        SearchField searchField;
        CustomMenuTreeView menuTreeView;
        TreeViewState treeViewState = new TreeViewState();

        Rect rightRect;
        Vector2 rightScroll;

        public virtual float LeftMinWidth
        {
            get { return 50; }
        }

        public virtual float RightMinWidth
        {
            get { return 500; }
        }

        public Rect RightRect
        {
            get { return rightRect; }
        }

        protected virtual void OnEnable()
        {
            resizableArea.minSize = new Vector2(LeftMinWidth, 50);
            resizableArea.side = 10;
            resizableArea.EnableSide(UIDirection.Right);
            resizableArea.SideOffset[UIDirection.Right] = resizableArea.side / 2;

            searchField = new SearchField();
            menuTreeView = BuildMenuTree(treeViewState);
            menuTreeView.Reload();
        }

        void OnGUI()
        {
            Rect searchFieldRect = resizableAreaRect;
            searchFieldRect.height = 20;
            searchFieldRect.y += 3;
            searchFieldRect.x += 5;
            searchFieldRect.width -= 10;
            string tempSearchText = searchField.OnGUI(searchFieldRect, searchText);
            if (tempSearchText != searchText)
            {
                searchText = tempSearchText;
                menuTreeView.searchString = searchText;
            }

            resizableArea.maxSize = position.size;
            resizableAreaRect.height = position.height;
            resizableAreaRect = resizableArea.OnGUI(resizableAreaRect);

            Rect treeviewRect = resizableAreaRect;
            treeviewRect.y += searchFieldRect.height;
            treeviewRect.height -= searchFieldRect.height;
            menuTreeView.OnGUI(treeviewRect);

            Rect sideRect = resizableAreaRect;
            sideRect.x += sideRect.width;
            sideRect.width = 1;
            EditorGUI.DrawRect(sideRect, new Color(0.5f, 0.5f, 0.5f, 1));

            rightRect = sideRect;
            rightRect.x += rightRect.width + 1;
            rightRect.width = position.width - resizableAreaRect.width - sideRect.width - 2;
            rightRect.width = Mathf.Max(rightRect.width, RightMinWidth);

            GUILayout.BeginArea(rightRect);
            rightRect.x = 0;
            rightRect.y = 0;
            IList<int> selection = menuTreeView.GetSelection();
            if (selection.Count > 0)
            {
                rightScroll = GUILayout.BeginScrollView(rightScroll, false, false);
                OnRightGUI(menuTreeView.Find(selection[0]) as CustomMenuTreeViewItem);
                GUILayout.EndScrollView();
            }

            GUILayout.EndArea();
        }

        protected abstract CustomMenuTreeView BuildMenuTree(TreeViewState _treeViewState);

        protected virtual void OnRightGUI(CustomMenuTreeViewItem _selectedItem)
        {
        }
    }

    public class CustomMenuTreeView : CustomTreeView
    {
        public CustomMenuTreeView(TreeViewState state) : base(state)
        {
            rowHeight = 30;
#if !UNITY_2019_1_OR_NEWER
            customFoldoutYOffset = rowHeight / 2 - 8;
#endif
        }

        public T AddMenuItem<T>(string _path) where T : CustomMenuTreeViewItem, new()
        {
            return AddMenuItem<T>(_path, (Texture2D)null);
        }

        public T AddMenuItem<T>(string _path, Texture2D _icon) where T : CustomMenuTreeViewItem, new()
        {
            if (string.IsNullOrEmpty(_path))
                return null;
            T item = new T();
            item.icon = _icon;
            return item;
        }

        public string GetParentPath(string _path)
        {
            int index = _path.LastIndexOf('/');
            if (index == -1)
                return null;
            return _path.Substring(0, index);
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return false;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);
            CustomMenuTreeViewItem item = args.item as CustomMenuTreeViewItem;
            if (item != null)
                item.itemDrawer?.Invoke(args.rowRect, item);
        }
    }

    public class CustomMenuTreeViewItem : CustomTreeViewItem
    {
        public Action<Rect, CustomMenuTreeViewItem> itemDrawer;

        public CustomMenuTreeViewItem() : base()
        {
        }
    }
}