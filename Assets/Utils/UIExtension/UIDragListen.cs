using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utils.UGUIExtension
{
    public class UIDragListen : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public UnityEvent<PointerEventData> OnBeginDragEvent = new UnityEvent<PointerEventData>();
        public UnityEvent<PointerEventData> OnEndDragEvent = new UnityEvent<PointerEventData>();

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnBeginDragEvent.Invoke(eventData);
            transform.parent?.GetComponentInParent<IBeginDragHandler>()?.OnBeginDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnEndDragEvent.Invoke(eventData);
            transform.parent?.GetComponentInParent<IEndDragHandler>()?.OnEndDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.parent?.GetComponentInParent<IDragHandler>()?.OnDrag(eventData);
        }
    }

    public static partial class UGUIExtension
    {
        public static UnityEvent<PointerEventData> OnBeginDragListen(this Slider slider)
        {
            if (slider.handleRect == null) return null;

            UIDragListen listen = slider.handleRect.GetComponent<UIDragListen>();
            if (listen == null) listen = slider.handleRect.gameObject.AddComponent<UIDragListen>();

            return listen.OnBeginDragEvent;
        }
    
        public static UnityEvent<PointerEventData> OnEndDragListen(this Slider slider)
        {
            if (slider.handleRect == null) return null;

            UIDragListen listen = slider.handleRect.GetComponent<UIDragListen>();
            if (listen == null) listen = slider.handleRect.gameObject.AddComponent<UIDragListen>();

            return listen.OnEndDragEvent;
        }
    }

}

