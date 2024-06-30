using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlockViewBase : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler {
    protected Action OnClickDown;
    protected Action OnEneter;

    public void OnPointerDown(PointerEventData eventData) {
        OnClickDown?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        OnEneter?.Invoke();
    }
}
