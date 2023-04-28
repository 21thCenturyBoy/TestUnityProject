using UnityEngine;

namespace GameUIControl.RecycleListView.UI
{

    public class RecycleListScrollViewItem<DataType> :MonoBehaviour where DataType : ItemData
    {
        public DataType data;
        public virtual void ShowData(DataType data)
        {
            this.data = data;
            data.Item = this;
        }
    }
}
