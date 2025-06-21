using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battlehub.UIControls.DockPanels;
using Unity.Collections;
using UnityEngine;

namespace ScratchFramework
{
    [Serializable]
    public class WindowDescriptor
    {
        public Sprite Icon;
        public string Header;
        public GameObject ContentPrefab;

        public int MaxWindows = 1;
        public int Created = 0;

        public WindowType Type;

        public string TypeName;
        public bool IsSingleton => MaxWindows == 1;
    }

    [Serializable]
    public class CustomWindowDescriptor : WindowDescriptor
    {
        public bool IsDialog;
    }

    public enum WindowType
    {
        Custom = -1,

        Canavas,

        Material,
        Variable,
        Parameter,

        Tools,
    }


    public class WindowManager : ScratchSingleton<WindowManager>
    {
        private DockPanel m_DockPanel;
        public DockPanel DockPanel => m_DockPanel;

        public Dictionary<string, CustomWindowDescriptor> m_typeToCustomWindow = new Dictionary<string, CustomWindowDescriptor>();

        public readonly Dictionary<string, HashSet<Transform>> m_windows = new Dictionary<string, HashSet<Transform>>();

        private LayoutInfo m_UserLayout;

        [SerializeField] private List<WindowDescriptor> m_defaultWindows = new List<WindowDescriptor>();

        private Dictionary<WindowType, GameObject> m_SingletonWnidows = new Dictionary<WindowType, GameObject>();


        private void Start()
        {
            Initialize();
        }

        public override bool Initialize()
        {
            m_DockPanel = GetComponentInChildren<DockPanel>();
            LoadUserLayout();

            return base.Initialize();
        }

        private Coroutine m_CoroutineSetLayout;

        public void LoadUserLayout()
        {
            //TODO Read user data
            SetLayout(m_UserLayout);
        }

        public LayoutInfo SetDefaultLayout()
        {
            LayoutInfo canvasLayoutInfo = null;
            LayoutInfo materialLayoutInfo = null;
            LayoutInfo variableLayoutInfo = null;
            LayoutInfo parameterLayoutInfo = null;
            LayoutInfo toolsLayoutInfo = null;

            for (int i = 0; i < m_defaultWindows.Count; i++)
            {
                var windowDescriptor = m_defaultWindows[i];

                CreateWindow(windowDescriptor, out var gameObjectContent, out bool isDialog);
                LayoutInfo layout = CreateLayoutInfo(gameObjectContent.transform, windowDescriptor.Header, windowDescriptor.Icon);
                switch (windowDescriptor.Type)
                {
                    case WindowType.Custom:
                        break;
                    case WindowType.Canavas:
                        canvasLayoutInfo = layout;
                        break;
                    case WindowType.Material:
                        materialLayoutInfo = layout;
                        break;
                    case WindowType.Variable:
                        variableLayoutInfo = layout;
                        break;
                    case WindowType.Parameter:
                        parameterLayoutInfo = layout;
                        break;
                    case WindowType.Tools:
                        toolsLayoutInfo = layout;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            LayoutInfo alllayout = new LayoutInfo(false,
                toolsLayoutInfo,
                new LayoutInfo(false,
                    canvasLayoutInfo,
                    new LayoutInfo(materialLayoutInfo, variableLayoutInfo, parameterLayoutInfo),
                    0.8f),
                0.02f);
            return alllayout;
        }

        public LayoutInfo CreateLayoutInfo(Transform content, string header, Sprite icon)
        {
            Tab tab = Instantiate(DockPanel.TabPrefab);
            tab.Text = header;
            tab.Icon = icon;
            return new LayoutInfo(content, tab);
        }


        public Transform CreateWindow(WindowDescriptor wd, out GameObject content, out bool isDialog)
        {
            if (DockPanel == null)
            {
                Debug.LogError("Unable to create window. m_dockPanel == null. Set m_dockPanel field");
            }

            content = null;
            isDialog = false;
            if (wd is CustomWindowDescriptor customWindowDescriptor)
            {
                isDialog = customWindowDescriptor.IsDialog;
            }

            if (wd.Created >= wd.MaxWindows)
            {
                return null;
            }

            wd.Created++;

            if (wd.ContentPrefab != null)
            {
                wd.ContentPrefab.SetActive(false);
                content = Instantiate(wd.ContentPrefab);
                content.name = wd.TypeName;
            }
            else
            {
                content = new GameObject();
                content.AddComponent<RectTransform>();
                content.name = "Empty Content";
            }


            HashSet<Transform> windows;
            if (!m_windows.TryGetValue(wd.TypeName, out windows))
            {
                windows = new HashSet<Transform>();
                m_windows.Add(wd.TypeName, windows);
            }

            windows.Add(content.transform);

            BaseWindowsView windowView = content.GetComponent<BaseWindowsView>();
            if (windowView != null)
            {
                if (wd.MaxWindows == 1)
                {
                    m_SingletonWnidows.Add(wd.Type, content);
                }

                windowView.WindowType = wd;
                windowView.Initialize();
            }

            return content.transform;
        }

        private bool m_lockUpdateLayout;

        private void ClearRegion(Region rootRegion)
        {
            rootRegion.CloseAllTabs();
        }

        public void ClearAllRegion()
        {
            Region rootRegion = DockPanel.RootRegion;

            ClearRegion(rootRegion);
            foreach (Transform child in DockPanel.Free)
            {
                Region region = child.GetComponent<Region>();
                ClearRegion(region);

                Destroy(region.gameObject);
            }

            Destroy(rootRegion.gameObject);
        }

        public void HideAllRegion()
        {
            Region rootRegion = DockPanel.RootRegion;
            if (rootRegion == null)
            {
                return;
            }
            rootRegion.gameObject.SetActive(false);
        }

        public bool RootRegionIsActive()
        {
            Region rootRegion = DockPanel.RootRegion;
            if (rootRegion == null)
            {
                return false;
            }
            return rootRegion.gameObject.activeSelf;
        }

        public void ShowAllRegion()
        {
            Region rootRegion = DockPanel.RootRegion;
            if (rootRegion == null)
            {
                return;
            }
            rootRegion.gameObject.SetActive(true);
        }


        public void SetLayout(LayoutInfo layoutInfo, string activateWindowOfType = null, Action<LayoutInfo> callback = null)
        {
            Region rootRegion = DockPanel.RootRegion;
            if (rootRegion == null)
            {
                return;
            }

            try
            {
                m_lockUpdateLayout = true;

                bool hasChildren = rootRegion.HasChildren;
                ClearRegion(rootRegion);
                foreach (Transform child in DockPanel.Free)
                {
                    Region region = child.GetComponent<Region>();
                    ClearRegion(region);
                }

                m_CoroutineSetLayout = StartCoroutine(CoSetLayout(hasChildren, layoutInfo, activateWindowOfType, callback));
            }
            catch
            {
                m_lockUpdateLayout = false;
            }
        }

        public event Action<WindowManager> AfterLayout;

        private IEnumerator CoSetLayout(bool waitForEndOfFrame, LayoutInfo layoutInfo, string activateWindowOfType = null, Action<LayoutInfo> callback = null)
        {
            if (waitForEndOfFrame)
            {
                //Wait for OnDestroy of destroyed windows 
                yield return new WaitForEndOfFrame();
            }

            try
            {
                m_lockUpdateLayout = true;
                if (layoutInfo == null)
                {
                    layoutInfo = SetDefaultLayout();
                }

                if (layoutInfo.Content != null || layoutInfo.Child0 != null && layoutInfo.Child1 != null)
                {
                    DockPanel.RootRegion.Build(layoutInfo);
                }

                if (!string.IsNullOrEmpty(activateWindowOfType))
                {
                    ActivateWindow(activateWindowOfType);
                }
            }
            finally
            {
                m_lockUpdateLayout = false;
            }


            if (AfterLayout != null)
            {
                AfterLayout(this);
            }

            m_UserLayout = layoutInfo;
        }

        public Transform GetWindow(string windowTypeName)
        {
            HashSet<Transform> hs;
            if (m_windows.TryGetValue(windowTypeName.ToLower(), out hs))
            {
                return hs.FirstOrDefault();
            }

            return null;
        }

        private bool m_isPointerOverActiveWindow = true;

        public bool IsPointerOverActiveWindow
        {
            get { return m_isPointerOverActiveWindow; }
            set { m_isPointerOverActiveWindow = value; }
        }

        public virtual Vector3 GetPointerXY(int pointer)
        {
            if (pointer == 0)
            {
                return Input.mousePosition;
            }
            else
            {
                Touch touch = Input.GetTouch(pointer);
                return touch.position;
            }
        }

        public bool ActivateWindow(string windowTypeName)
        {
            Transform content = GetWindow(windowTypeName);
            if (content == null)
            {
                return false;
            }

            if (content == null)
            {
                return false;
            }

            Region region = content.GetComponentInParent<Region>();
            if (region != null)
            {
                region.MoveRegionToForeground();
                IsPointerOverActiveWindow = RectTransformUtility.RectangleContainsScreenPoint((RectTransform)region.transform, GetPointerXY(0), ScratchProgrammingManager.Instance.CanvasCamera);
                if (IsPointerOverActiveWindow)
                {
                }
            }

            Tab tab = Region.FindTab(content);
            if (tab == null)
            {
                return false;
            }

            tab.IsOn = true;
            return true;
        }

        public bool SingletonIsOpen<T>(out T o)
        {
            foreach (var wnidowObj in m_SingletonWnidows.Values)
            {
                if (wnidowObj.GetComponent<T>() != null)
                {
                    o = wnidowObj.GetComponent<T>();
                    return true;
                }
            }

            o = default;
            return false;
        }

        public void RemoveWindow(BaseWindowsView baseWindowsView)
        {
            if (baseWindowsView.WindowType.Created > 0)
            {
                baseWindowsView.WindowType.Created--;
            }

            HashSet<Transform> windows;
            if (m_windows.TryGetValue(baseWindowsView.WindowType.TypeName, out windows))
            {
                if (windows.Contains(baseWindowsView.transform))
                {
                    windows.Remove(baseWindowsView.transform);
                }
            }

            if (m_SingletonWnidows.ContainsKey(baseWindowsView.WindowType.Type))
            {
                m_SingletonWnidows.Remove(baseWindowsView.WindowType.Type);
            }
        }
    }
}