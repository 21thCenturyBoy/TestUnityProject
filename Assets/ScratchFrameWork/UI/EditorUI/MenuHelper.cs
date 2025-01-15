using System;
using System.Collections.Generic;
using System.Linq;
using Battlehub.UIControls.MenuControl;
using UnityEngine;

namespace ScratchFramework
{
    [Serializable]
    public class MenuConfig
    {
        public List<MenuConfigInfo> ConfigInfos = new List<MenuConfigInfo>();

        public float Width = 200;

        public int GetPrefactWidthRatio()
        {
            int max = 0;
            for (int i = 0; i < ConfigInfos.Count; i++)
            {
                var length = ConfigInfos[i].Path.Split("/")[0].Length;
                max = Mathf.Max(max, length);
            }

            return max;
        }
    }

    [Serializable]
    public class MenuConfigInfo
    {
        public MenuConfigInfo(string command, string allPath)
        {
            Command = command;
            ALLPath = allPath;
            var paths = allPath.Split("/");
            Text = paths[^1];
            MainMenu = paths[0];
            Path = ALLPath.Replace(MainMenu + "/", "");
        }

        public string MainMenu;
        public string Command;

        public string ALLPath;
        public string Path;
        public string Text;
        public Sprite Icon;

        public bool IsVisible = true;
        public bool IsOn = false;
    }

    public static class MenuHelper
    {
        public const string FileMenuName = "File";
        public const string EditMenuName = "Edit";
        public const string GameObjectMenuName = "GameObject";
        public const string BlocklyMenuName = "Blockly";
        public const string DebugMenuName = "Debug";
        public const string WindowMenuName = "Window";
        public const string HelpMenuName = "Help";

        //Path for the menu item
        public const string CMD_Open = FileMenuName + "/Open";
        public const string CMD_Save = FileMenuName + "/Save";
        public const string CMD_Save_As = FileMenuName + "/Save As";
        public const string CMD_Exit = FileMenuName + "/Exit";

        public const string CMD_Cut = EditMenuName + "/Cut";
        public const string CMD_Copy = EditMenuName + "/Copy";
        public const string CMD_Paste = EditMenuName + "/Paste";
        public const string CMD_Delete = EditMenuName + "/Delete";
        public const string CMD_Setting = EditMenuName + "/Setting";

        public const string CMD_Create_Cube = GameObjectMenuName + "/3D Object/Cube";
        public const string CMD_Create_Sphere = GameObjectMenuName + "/3D Object/Sphere";


        public const string CMD_Undo = BlocklyMenuName + "/Undo";
        public const string CMD_Redo = BlocklyMenuName + "/Redo";

        public const string CMD_Play = DebugMenuName + "/Play(Pause)";
        public const string CMD_Stop = DebugMenuName + "/Stop";

        public const string CMD_Open_Hide_ScratchCanvasWindow = WindowMenuName + "/ScratchCanvas";
        public const string CMD_Open_Hide_MaterialWindow = WindowMenuName + "/MaterialBox";

        public const string CMD_About_Me = HelpMenuName + "/About Me";

        private static Dictionary<string, MenuConfig> m_AllMenuDict = new Dictionary<string, MenuConfig>();

        public static Dictionary<string, MenuConfig> GetAllMenu()
        {
            List<MenuConfigInfo> menuConfigInfos = new List<MenuConfigInfo>();

            //TODO Add all menu item
            menuConfigInfos.Add(new MenuConfigInfo(nameof(CMD_Open), CMD_Open));
            menuConfigInfos.Add(new MenuConfigInfo(nameof(CMD_Save), CMD_Save));
            menuConfigInfos.Add(new MenuConfigInfo(nameof(CMD_Save_As), CMD_Save_As));
            menuConfigInfos.Add(new MenuConfigInfo(nameof(CMD_Exit), CMD_Exit));
            menuConfigInfos.Add(new MenuConfigInfo(nameof(CMD_Cut), CMD_Cut));
            menuConfigInfos.Add(new MenuConfigInfo(nameof(CMD_Copy), CMD_Copy));
            menuConfigInfos.Add(new MenuConfigInfo(nameof(CMD_Paste), CMD_Paste));
            menuConfigInfos.Add(new MenuConfigInfo(nameof(CMD_Delete), CMD_Delete));
            menuConfigInfos.Add(new MenuConfigInfo(nameof(CMD_Setting), CMD_Setting));
            menuConfigInfos.Add(new MenuConfigInfo(nameof(CMD_Create_Cube), CMD_Create_Cube));
            menuConfigInfos.Add(new MenuConfigInfo(nameof(CMD_Create_Sphere), CMD_Create_Sphere));
            menuConfigInfos.Add(new MenuConfigInfo(nameof(CMD_Undo), CMD_Undo));
            menuConfigInfos.Add(new MenuConfigInfo(nameof(CMD_Redo), CMD_Redo));
            menuConfigInfos.Add(new MenuConfigInfo(nameof(CMD_Play), CMD_Play));
            menuConfigInfos.Add(new MenuConfigInfo(nameof(CMD_Stop), CMD_Stop));
            menuConfigInfos.Add(new MenuConfigInfo(nameof(CMD_Open_Hide_ScratchCanvasWindow), CMD_Open_Hide_ScratchCanvasWindow));
            menuConfigInfos.Add(new MenuConfigInfo(nameof(CMD_Open_Hide_MaterialWindow), CMD_Open_Hide_MaterialWindow));
            menuConfigInfos.Add(new MenuConfigInfo(nameof(CMD_About_Me), CMD_About_Me));


            for (int i = 0; i < menuConfigInfos.Count; i++)
            {
                string allpath = menuConfigInfos[i].ALLPath;
                string menu = menuConfigInfos[i].MainMenu;

                if (!m_AllMenuDict.ContainsKey(menu))
                {
                    m_AllMenuDict.Add(menu, new MenuConfig());
                }

                m_AllMenuDict[menu].ConfigInfos.Add(menuConfigInfos[i]);
            }

            foreach (var menuConfig in m_AllMenuDict)
            {
                menuConfig.Value.Width = menuConfig.Value.GetPrefactWidthRatio() * 30;
            }

            return m_AllMenuDict;
        }

        public static void AddMenuCommand(MenuItemInfo menuItemInfo)
        {
            menuItemInfo.Action.RemoveAllListeners();
            menuItemInfo.Action.AddListener( (cmd) =>
            {
                var action = GetCMDAction(cmd);
                action?.Invoke();
            });
        }

        public static Action GetCMDAction(string cmd)
        {
            Action action = null;
            switch (cmd)
            {
                case nameof(CMD_Open):
                    break;
                case nameof(CMD_Save):
                    break;
                case nameof(CMD_Save_As):
                    break;
                case nameof(CMD_Exit):
                    break;
                case nameof(CMD_Cut):
                    break;
                case nameof(CMD_Copy):
                    break;
                case nameof(CMD_Paste):
                    break;
                case nameof(CMD_Delete):
                    break;
                case nameof(CMD_Setting):
                    break;
                case nameof(CMD_Create_Cube):
                    break;
                case nameof(CMD_Create_Sphere):
                    break;
                case nameof(CMD_Undo):
                    break;
                case nameof(CMD_Redo):
                    break;
                case nameof(CMD_Play):
                    break;
                case nameof(CMD_Stop):
                    break;
                case nameof(CMD_Open_Hide_ScratchCanvasWindow):
                    break;
                case nameof(CMD_Open_Hide_MaterialWindow):
                    break;
                case nameof(CMD_About_Me):
                    break;
            }
            
            return action;
        }
    }
}