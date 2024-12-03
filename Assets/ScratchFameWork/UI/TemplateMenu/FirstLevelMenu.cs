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

        private FirstLevelType m_CurrentShowType = FirstLevelType.None;
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
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        m_CurrentShowType = type;
                        
                        ScratchResourcesManager.Instance.ShowFirstLevel(type);
                        ScratchResourcesManager.Instance.Active = true;
           
                    }
                    else
                    {
                        m_CurrentShowType = FirstLevelType.None;
                        ScratchResourcesManager.Instance.Active = false;
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
        
    }
}