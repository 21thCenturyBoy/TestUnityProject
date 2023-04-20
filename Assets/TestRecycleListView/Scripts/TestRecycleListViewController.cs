using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRecycleListViewController : TestRecycleListView.RecycleListView<TestRecycleListItemData, TestRecycleListItem>
{
    public string defaultTemplate;

    protected override void Setup()
    {
        base.Setup();

        data = GetData();
    }

    private TestRecycleListItemData[] GetData()
    {
        TestRecycleListItemData[] newDatas = new TestRecycleListItemData[100];
        for (int i = 0; i < newDatas.Length; i++)
        {
            Debug.Log(i);
            newDatas[i] = new TestRecycleListItemData();
            newDatas[i].text = $"Text {i}";
            newDatas[i].template = defaultTemplate;
        }
        return newDatas;
    }
}
