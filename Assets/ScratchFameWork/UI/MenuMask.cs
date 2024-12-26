using UnityEngine.EventSystems;

namespace ScratchFramework
{
    public class MenuMask : ScratchUIBehaviour,IScratchBlockClick
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            MenuUIManager.Instance.CloseAll();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
        }

        public void OnPointerClick(PointerEventData eventData)
        {
        }
    }
}

