using GameUIControl.RecycleListView.UI;
using UnityEngine;

public class TestConsoleScrollInput : RecycleListScrollMouseScroller<TestConsoleScrollView>
{
    private Vector3 LastScreenPoint;
    public float MouseScrollSensitivity = 30f;
    protected override void HandleInput()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            View.ScrollOffset += MouseScrollSensitivity * Input.mouseScrollDelta.y;
        }

        if (Input.GetMouseButton(0))
        {
            StartScrolling(LastScreenPoint);
        }
        else
        {
            StopScrolling();
        }
        if (m_Scrolling) Scroll(Input.mousePosition);
        LastScreenPoint = Input.mousePosition;//光标失去会导致
    }
}
