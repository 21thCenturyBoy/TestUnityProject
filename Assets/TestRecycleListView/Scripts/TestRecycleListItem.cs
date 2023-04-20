using TestRecycleListView;
using UnityEngine.UI;

[System.Serializable]
public class TestRecycleListItemData : ItemData
{
    public string text;
}
public class TestRecycleListItem : RecycleListItem<TestRecycleListItemData>
{
    public Text label;
    // Start is called before the first frame update
    public override void Setup(TestRecycleListItemData data)
    {
        base.Setup(data);
        label.text = data.text;
    }
}
