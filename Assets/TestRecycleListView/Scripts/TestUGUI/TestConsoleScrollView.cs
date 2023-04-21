using System;
using System.Collections;
using System.Collections.Generic;
using TestRecycleListView.UI;
using UnityEngine;

public class TestConsoleScrollView : RecycleListScrollViewUI<TestLogData, List<TestLogData>, TestLogItem>
{
    public bool TestLog;
    public override void Initialize()
    {
        base.Initialize();

        Data = new List<TestLogData>();


    }

    public override void AddData(TestLogData data)
    {
        Data.Add(data);
    }

    public override void RemoveData(TestLogData data)
    {
        Data.Remove(data);
    }

    public void LateUpdate()
    {
        if (!TestLog) return;

        AddData(new TestLogData(Time.time.ToString()));
    }
}
