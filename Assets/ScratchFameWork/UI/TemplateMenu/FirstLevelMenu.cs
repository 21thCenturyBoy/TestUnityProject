using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ScratchFramework
{
    public class FirstLevelMenu : ScratchUISingleton<FirstLevelMenu>
    {
        private List<FirstLevel> m_firstLevels = new List<FirstLevel>();

        private int m_CurrentShowType = -1;
        public FirstLevelType CurrentShow => (FirstLevelType)m_CurrentShowType;

        private void Start()
        {
            ShowMenu();
        }

        private ToggleGroup m_toggleGroup;

        public void ShowMenu()
        {
            FirstLevelType[] types = GetFirstLevelMenuTypes();

            FirstLevel[] firstLevels = transform.GetComponentsInChildren<FirstLevel>();

            for (int i = 0; i < firstLevels.Length; i++)
            {
                bool show = types.Contains(firstLevels[i].Type);
                FirstLevelType type = firstLevels[i].Type;

                firstLevels[i].gameObject.SetActive(show);

                var toggle = firstLevels[i].GetComponent<Toggle>();
                toggle.isOn = false;

                toggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        ShowMenu(type);
                    }
                    else
                    {
                        HideMenu(type);
                    }
                });
            }
            ScratchResourcesManager.Instance.Active = false;
        }

        private FirstLevelType[] GetFirstLevelMenuTypes()
        {
            //TODO
            var array = Enum.GetValues(typeof(FirstLevelType));
            FirstLevelType[] types = new FirstLevelType[array.Length];

            int i = 0;
            foreach (var type in array)
            {
                types[i] = (FirstLevelType)type;
                i++;
            }

            return types;
        }

        private void ShowMenu(FirstLevelType type)
        {
            //TODO
            m_CurrentShowType = (int)type;
            
            ScratchResourcesManager.Instance.ShowFirstLevel(type);
            ScratchResourcesManager.Instance.Active = true;
        }

        private void HideMenu(FirstLevelType type)
        {
            //TODO
            m_CurrentShowType = -1;
            
            ScratchResourcesManager.Instance.Active = false;
        }
    }
}