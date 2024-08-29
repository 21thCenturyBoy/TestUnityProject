using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ScratchFramework
{
    public class MenuMask : ScratchUIBehaviour,IScratchBlockClick
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            ScratchMenuManager.Instance.CloseAll();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
        }

        public void OnPointerClick(PointerEventData eventData)
        {
        }
    }
}

