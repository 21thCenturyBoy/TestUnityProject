using System;
using GameUIControl.RecycleListView;
using GameUIControl.RecycleListView.UI;
using UnityEngine.UI;

[Serializable]
public class TestLogData : ItemData
{
    public TestLogData(string infoText)
    {
        InfoText = infoText;
    }

    public string InfoText;
}
public class TestLogItem : RecycleListScrollViewItem<TestLogData>
{
    public Text LabelText;

    public override void ShowData(TestLogData data)
    {
        base.ShowData(data);
        LabelText.text = data.InfoText;
    }
}
