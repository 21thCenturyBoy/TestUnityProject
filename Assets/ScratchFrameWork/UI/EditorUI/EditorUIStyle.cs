using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ScratchFramework
{
    public static class EditorUIStyle
    {
        public const string ResourceNamePrefix_EditorIcon = "Round/EditorUI";

        public const string Name_Arrow = "Arrow";
        public const string Name_DarkBackV = "DarkBackV";
        public const string Name_DegDarkH = "DegDarkH";
        public const string Name_Frame = "Frame";
        public const string Name_Frame_1 = "Frame_1";
        public const string Name_Frame_2 = "Frame_2";
        public const string Name_Code = "Code";
        public const string Name_Game = "Game";
        public const string Name_Play = "Play";
        public const string Name_Stop = "Stop";


        public const string ResourceNamePrefix_EditorPrefab = "EditorWindow/";
        
        public const string Name_Menu = "Menu";
        public const string Name_MenuButton = "MenuButton";
        public const string Name_MenuItem = "MenuItem";
        
        public const string Name_Region = "Region";
        
        public static void GetIconStyle(string name,Action<Sprite> callback)
        {
            // string assetname = Path.Combine(ResourceNamePrefix_EditorIcon, name);
            //
            // if (m_IconStyleCache.ContainsKey(assetname))
            // {
            //     callback?.Invoke(m_IconStyleCache[assetname]);
            // }
            // else
            // {
            //     ScratchConfig.ResourceLoad<Sprite>(assetname, (sprite) =>
            //     {
            //         m_IconStyleCache[assetname] = sprite;
            //         callback?.Invoke(sprite);
            //     });
            // }
        }
        
        public static void GetPrefabStyle(string name,Action<GameObject> callback)
        {
            string assetname = Path.Combine(ResourceNamePrefix_EditorPrefab, name);
            
            if (m_PrefabCache.ContainsKey(assetname))
            {
                callback?.Invoke(m_PrefabCache[assetname]);
            }
            else
            {
                ScratchConfig.ResourceLoad<GameObject>(assetname, (obj) =>
                {
                    m_PrefabCache[assetname] = obj;
                    callback?.Invoke(obj);
                });
            }
        }


        private static Dictionary<string, Sprite> m_IconStyleCache = new Dictionary<string, Sprite>();
        private static Dictionary<string, GameObject> m_PrefabCache = new Dictionary<string, GameObject>();

        public static void ResourcePreLoad()
        {
            string resPath = string.Empty;
            
            resPath = Path.Combine(ResourceNamePrefix_EditorIcon, Name_Arrow);
            m_IconStyleCache[resPath] = ScratchConfig.PreResourceLoad<Sprite>(resPath);
            resPath = Path.Combine(ResourceNamePrefix_EditorIcon, Name_DarkBackV);
            m_IconStyleCache[resPath] = ScratchConfig.PreResourceLoad<Sprite>(resPath);
            resPath = Path.Combine(ResourceNamePrefix_EditorIcon, Name_DegDarkH);
            m_IconStyleCache[resPath] = ScratchConfig.PreResourceLoad<Sprite>(resPath);
            resPath = Path.Combine(ResourceNamePrefix_EditorIcon, Name_Frame);
            m_IconStyleCache[resPath] = ScratchConfig.PreResourceLoad<Sprite>(resPath);
            resPath = Path.Combine(ResourceNamePrefix_EditorIcon, Name_Frame_1);
            m_IconStyleCache[resPath] = ScratchConfig.PreResourceLoad<Sprite>(resPath);
            resPath = Path.Combine(ResourceNamePrefix_EditorIcon, Name_Frame_2);
            m_IconStyleCache[resPath] = ScratchConfig.PreResourceLoad<Sprite>(resPath);
            resPath = Path.Combine(ResourceNamePrefix_EditorIcon, Name_Code);
            m_IconStyleCache[resPath] = ScratchConfig.PreResourceLoad<Sprite>(resPath);
            resPath = Path.Combine(ResourceNamePrefix_EditorIcon, Name_Game);
            m_IconStyleCache[resPath] = ScratchConfig.PreResourceLoad<Sprite>(resPath);
            resPath = Path.Combine(ResourceNamePrefix_EditorIcon, Name_Play);
            m_IconStyleCache[resPath] = ScratchConfig.PreResourceLoad<Sprite>(resPath);
            resPath = Path.Combine(ResourceNamePrefix_EditorIcon, Name_Stop);
            m_IconStyleCache[resPath] = ScratchConfig.PreResourceLoad<Sprite>(resPath);
            
            
            resPath = Path.Combine(ResourceNamePrefix_EditorPrefab, Name_Menu);
            m_PrefabCache[resPath] = ScratchConfig.PreResourceLoad<GameObject>(resPath);
            resPath = Path.Combine(ResourceNamePrefix_EditorPrefab, Name_MenuButton);
            m_PrefabCache[resPath] = ScratchConfig.PreResourceLoad<GameObject>(resPath);
            resPath = Path.Combine(ResourceNamePrefix_EditorPrefab, Name_MenuItem);
            m_PrefabCache[resPath] = ScratchConfig.PreResourceLoad<GameObject>(resPath);
            
            
            resPath = Path.Combine(ResourceNamePrefix_EditorPrefab, Name_Region);
            m_PrefabCache[resPath] = ScratchConfig.PreResourceLoad<GameObject>(resPath);
        }
    }
}