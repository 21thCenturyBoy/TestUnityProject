using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utils.UGUIExtension
{
    public class UGUIPointerEvent : MonoBehaviour
    {
        public UnityEvent<PointerEventData> OnEvent = new UnityEvent<PointerEventData>();

        protected virtual void OnEventCall(PointerEventData eventData)
        {
            OnEvent?.Invoke(eventData);
        }
    }

    public class UGUIPointerClickEvent : UGUIPointerEvent, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            OnEventCall(eventData);
            transform.parent?.GetComponentInParent<IPointerClickHandler>()?.OnPointerClick(eventData);
        }
    }
    
}