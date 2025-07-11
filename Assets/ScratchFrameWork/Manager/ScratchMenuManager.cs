using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace ScratchFramework
{
    public class ScratchMenuManager : ScratchSingleton<ScratchMenuManager>, IScratchManager
    {
        private RectTransform m_MenuButtonsTrans;
        private RectTransform m_MenuPanelTrans;

        public override bool Initialize()
        {
            m_MenuButtonsTrans = transform.Find("MenuButtons") as RectTransform;
            m_MenuPanelTrans = transform.Find("MenuPanel") as RectTransform;

            var menus = MenuHelper.GetAllMenu();

            GameObject menuButtonPrefab = null;
            GameObject menuPrefab = null;

            EditorUIStyle.GetPrefabStyle(EditorUIStyle.Name_MenuButton, (obj) => { menuButtonPrefab = obj; });
            EditorUIStyle.GetPrefabStyle(EditorUIStyle.Name_Menu, (obj) => { menuPrefab = obj; });

            if (menuButtonPrefab != null && menuPrefab != null)
            {
                foreach (var mainMenuName in menus.Keys)
                {
                    MainMenuButton mainMenuButton = Instantiate(menuButtonPrefab).GetComponent<MainMenuButton>();
                    mainMenuButton.SetParent(m_MenuButtonsTrans, m_MenuButtonsTrans.childCount);
                    
                    mainMenuButton.Text = mainMenuName;
                    mainMenuButton.gameObject.name = mainMenuName;
                    

                    Menu mainMenu = Instantiate(menuPrefab).GetComponent<Menu>();
                    mainMenu.SetParent(m_MenuPanelTrans, m_MenuPanelTrans.childCount);
                    
                    mainMenu.RectTrans.sizeDelta = new Vector2(menus[mainMenuName].Width, mainMenu.RectTrans.sizeDelta.y);
                    mainMenu.gameObject.name = mainMenuName;

                    MenuItemInfo[] items = new MenuItemInfo[menus[mainMenuName].ConfigInfos.Count];

                    for (int i = 0; i < menus[mainMenuName].ConfigInfos.Count; i++)
                    {
                        var info = menus[mainMenuName].ConfigInfos[i];

                        MenuItemInfo menuItemInfo = new MenuItemInfo();
                        menuItemInfo.AllPath = info.ALLPath;
                        menuItemInfo.Path = info.Path;
                        menuItemInfo.Text = info.Text;
                        menuItemInfo.Command = info.Command;
                        menuItemInfo.Icon = info.Icon;
                        menuItemInfo.IsVisible = info.IsVisible;
                        menuItemInfo.IsOn = info.IsOn;

                        items[i] = menuItemInfo;
                    }

                    mainMenuButton.Menu = mainMenu;
                    mainMenuButton.Initialize();
                    
                    mainMenu.Items = items;
                    mainMenu.Initialize();
                    
                    mainMenuButton.gameObject.SetActive(true);
                }
            }


            return base.Initialize();
        }

        public void OnUpdate()
        {
        }

        public void OnLateUpdate()
        {
        }

        public bool Clear()
        {
            return true;
        }
    }
}