using TestRecycleListView;
using UnityEngine.UI;

[System.Serializable]
public class TestRecycleListItemData : ItemInspectorData
{
    public string text;
}
public class TestRecycleListItem : RecycleListItem<TestRecycleListItemData>
{
    public Text label;
    // Start is called before the first frame update
    public override void Initialize(TestRecycleListItemData data)
    {
        base.Initialize(data);
        label.text = data.text;
    }
}
