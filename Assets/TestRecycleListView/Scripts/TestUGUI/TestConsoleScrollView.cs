using System;
using System.Collections;
using System.Collections.Generic;
using TestRecycleListView.UI;
using UnityEngine;

public class TestConsoleScrollView : RecycleListScrollViewUI<TestLogData, List<TestLogData>, TestLogItem>
{
    public enum Direction
    {
        HorizontalAdd,
        VerticalAdd,
        HorizontalDecrease,
        VerticalDecrease,
    }
    public Direction ViewDirection;
    public override void Initialize()
    {
        switch (ViewDirection)
        {
            case Direction.HorizontalAdd:
                Orientation = Vector3.right;
                break;
            case Direction.VerticalAdd:
                Orientation = Vector3.up;
                break;
            case Direction.HorizontalDecrease:
                Orientation = Vector3.left;
                break;
            case Direction.VerticalDecrease:
                Orientation = Vector3.down;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }


        base.Initialize();

        Data = new List<TestLogData>();

        m_ViewRect = transform.Find("ContentView").GetComponent<RectTransform>();
    }
    [SerializeField]
    private RectTransform m_ViewRect;
    public override void AddData(TestLogData data)
    {
        Data.Add(data);
    }

    public override void RemoveData(TestLogData data)
    {
        Data.Remove(data);
    }

    private void OnEnable()
    {
        StartCoroutine(Add());
    }

    public override bool ComputeConditions()
    {

        base.ComputeConditions();

        switch (ViewDirection)
        {
            case Direction.HorizontalAdd:
                m_ViewRect.transform.position = new Vector3(m_LeftSide.x + (Range - m_ItemSize.x) * 0.5f, m_LeftSide.y, m_LeftSide.z);
                m_ViewRect.sizeDelta = new Vector2(Range - Padding.x, m_ItemSize.y);
                break;
            case Direction.HorizontalDecrease:
                m_ViewRect.transform.position = new Vector3(m_LeftSide.x - (Range - m_ItemSize.x) * 0.5f, m_LeftSide.y, m_LeftSide.z);
                m_ViewRect.sizeDelta = new Vector2(Range - Padding.x, m_ItemSize.y);
                break;
            case Direction.VerticalAdd:
                m_ViewRect.transform.position = new Vector3(m_LeftSide.x, m_LeftSide.y + (Range - m_ItemSize.y) * 0.5f, m_LeftSide.z);
                m_ViewRect.sizeDelta = new Vector2(m_ItemSize.x, Range - Padding.y);
                break;
            case Direction.VerticalDecrease:
                m_ViewRect.transform.position = new Vector3(m_LeftSide.x, m_LeftSide.y - (Range - m_ItemSize.y) * 0.5f, m_LeftSide.z);
                m_ViewRect.sizeDelta = new Vector2(m_ItemSize.x, Range - Padding.y);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }


        return true;

    }
    protected override TestLogItem GetItem(TestLogData data, Transform parent = null)
    {
        return base.GetItem(data, m_ViewRect.transform);
    }
    private IEnumerator Add()
    {

        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if (Input.GetKey(KeyCode.A))
            {
                AddData(new TestLogData(Time.time.ToString()));
            }
        }
    }

}
