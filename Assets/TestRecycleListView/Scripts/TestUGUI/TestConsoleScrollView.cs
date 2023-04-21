using System;
using System.Collections;
using System.Collections.Generic;
using TestRecycleListView.UI;
using UnityEngine;

public class TestConsoleScrollView : RecycleListScrollViewUI<TestLogData, TestLogItem>
{
    public bool TestLog;
    public override void Initialize()
    {
        base.Initialize();

        Data = new List<TestLogData>();

        for (int i = 0; i < 20; i++)
        {
            Data.Add(new TestLogData("Text " +i));
        }
    }

    public void LateUpdate()
    {
        if (!TestLog) return;

        Data.Add(new TestLogData(Time.time.ToString()));
    }
}
