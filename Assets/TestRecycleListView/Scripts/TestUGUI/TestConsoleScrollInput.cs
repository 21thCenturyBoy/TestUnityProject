using System;
using System.Collections;
using System.Collections.Generic;
using TestRecycleListView.UI;
using UnityEngine;

public class TestConsoleScrollInput : RecycleListScrollMouseScroller<TestConsoleScrollView>
{
    private Vector3 screenPoint;
    private void OnEnable()
    {
        screenPoint = Input.mousePosition;
    }

    protected override void HandleInput()
    {
        if (Input.GetMouseButton(0))
        {
            StartScrolling(screenPoint);
        }
        else
        {
            StopScrolling();
        }
        screenPoint = Input.mousePosition;
        Scroll(screenPoint);
    }
}
