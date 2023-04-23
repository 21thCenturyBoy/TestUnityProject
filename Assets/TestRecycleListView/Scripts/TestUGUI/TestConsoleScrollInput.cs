using System;
using System.Collections;
using System.Collections.Generic;
using TestRecycleListView.UI;
using UnityEngine;

public class TestConsoleScrollInput : RecycleListScrollMouseScroller<TestConsoleScrollView>
{
    private Vector3 LastScreenPoint;
    protected override void HandleInput()
    {
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
