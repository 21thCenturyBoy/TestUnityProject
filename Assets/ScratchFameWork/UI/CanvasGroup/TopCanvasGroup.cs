using System;
using System.Collections.Generic;
using TMPro;

namespace ScratchFramework
{
    public class TopCanvasGroup : ScratchBehaviour
    {
        public override bool Initialize()
        {
            base.Initialize();

            RefreshGroupDropdown();
            RefreshCanvasDropdown();
            
            
            return true;
        }

        public void Save()
        {
            BlockDataUIManager.Instance.Save();
        }

        private void RefreshGroupDropdown()
        {
            var fileData = ScratchEngine.Instance.FileData;
          
            TMP_Dropdown groupDropdown = TempCanvasUIManager.Instance.TopCanvasGroup.transform.Find("GroupDropdown").GetComponent<TMP_Dropdown>();
            var options = new List<TMP_Dropdown.OptionData>();
            options.Add(new TMP_Dropdown.OptionData(nameof(EngineBlockFileData.Global)));
            for (int i = 0; i < fileData.CanvasGroups.Count; i++)
            {
                options.Add(new TMP_Dropdown.OptionData(fileData.CanvasGroups[i].Name));
            }

            groupDropdown.options = options;
            groupDropdown.onValueChanged.RemoveAllListeners();
            groupDropdown.onValueChanged.AddListener((index) =>
            {
                if (index == 0)
                {
                    ScratchEngine.Instance.CurrentGroup = ScratchEngine.Instance.FileData.Global;
                }
                else
                {
                    ScratchEngine.Instance.CurrentGroup = ScratchEngine.Instance.FileData.CanvasGroups[index - 1];
                }

                RefreshCanvasDropdown();
                
                BlockCanvasUIManager.Instance.RefreshCanvas();
            });

            if (ScratchEngine.Instance.CurrentIsGlobal)
            {
                groupDropdown.SetValueWithoutNotify(0);
            }
            else
            {
                groupDropdown.SetValueWithoutNotify(fileData.CanvasGroups.IndexOf(ScratchEngine.Instance.CurrentGroup) + 1);
            }
        }

        private void RefreshCanvasDropdown()
        {
            var group = ScratchEngine.Instance.CurrentGroup;
            TMP_Dropdown canvasDropdown = TempCanvasUIManager.Instance.TopCanvasGroup.transform.Find("CanvasDropdown").GetComponent<TMP_Dropdown>();
            var canvasOptions = new List<TMP_Dropdown.OptionData>();
            for (int i = 0; i < group.Canvas.Count; i++)
            {
                canvasOptions.Add(new TMP_Dropdown.OptionData(group.Canvas[i].Name));
            }

            canvasDropdown.options = canvasOptions;
            canvasDropdown.onValueChanged.RemoveAllListeners();
            canvasDropdown.onValueChanged.AddListener((index) =>
            {
                ScratchEngine.Instance.Current = ScratchEngine.Instance.CurrentGroup.Canvas[index];
                BlockCanvasUIManager.Instance.RefreshCanvas();
            });
            
            canvasDropdown.SetValueWithoutNotify(group.Canvas.IndexOf(ScratchEngine.Instance.Current));
        }

        public void AddGroups()
        {
            var group = EngineBlockFileData.CreateNewGroup();
            ScratchEngine.Instance.FileData.CanvasGroups.Add(group);
            ScratchEngine.Instance.CurrentGroup = group;

            RefreshGroupDropdown();
            RefreshCanvasDropdown();
            
            BlockCanvasUIManager.Instance.RefreshCanvas();
        }
        
        public void AddCanvans()
        {
            var canvas = EngineBlockFileData.CreateNewCanvas();
            ScratchEngine.Instance.CurrentGroup.Canvas.Add(canvas);
            ScratchEngine.Instance.Current = canvas;
            
            RefreshCanvasDropdown();
            
            BlockCanvasUIManager.Instance.RefreshCanvas();
        }
    }
}