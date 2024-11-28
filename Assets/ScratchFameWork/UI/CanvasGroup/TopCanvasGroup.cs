using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ScratchFramework
{
    public class TopCanvasGroup : ScratchBehaviour
    {
        public override bool Initialize()
        {
            base.Initialize();
            var group = ScratchEngine.Instance.CurrentGroup;
            TMP_Dropdown dropdown = TempCanvasManager.Instance.TopCanvasGroup.GetComponentInChildren<TMP_Dropdown>();
            var options = new List<TMP_Dropdown.OptionData>();
            options.Add(new TMP_Dropdown.OptionData(nameof(EngineBlockCanvasGroup.GlobalCanvas)));
            for (int i = 0; i < group.Canvas.Count; i++)
            {
                options.Add(new TMP_Dropdown.OptionData(group.Canvas[i].Name));
            }

            dropdown.options = options;
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.onValueChanged.AddListener((index) =>
            {
                if (index == 0)
                {
                    ScratchEngine.Instance.Current = ScratchEngine.Instance.CurrentGroup.GlobalCanvas;
                }
                else
                {
                    ScratchEngine.Instance.Current = ScratchEngine.Instance.CurrentGroup.Canvas[index - 1];
                }

                BlockCanvasManager.Instance.RefreshCanvas();
            });

            return true;
        }

        public void Save()
        {
            ScratchDataManager.Instance.Save();
        }
        
        public void AddCanvans()
        {
            var canvas = EngineBlockCanvasGroup.CreateNewCanvas();
            ScratchEngine.Instance.CurrentGroup.Canvas.Add(canvas);
            ScratchEngine.Instance.Current = canvas;

            TempCanvasManager.Instance.TopCanvasGroup.Initialize();
            BlockCanvasManager.Instance.RefreshCanvas();
        }
        
        public void AddCanvans(EngineBlockCanvas canvas)
        {
            ScratchEngine.Instance.CurrentGroup.Canvas.Add(canvas);
            ScratchEngine.Instance.Current = canvas;

            ScratchEngine.Instance.Current.RefreshDataGuids();

            TempCanvasManager.Instance.TopCanvasGroup.Initialize();
            BlockCanvasManager.Instance.RefreshCanvas();
        }
    }
}