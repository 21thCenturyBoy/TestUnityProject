using System.Collections;
using System.Collections.Generic;
using TestRecycleListView;
using UnityEngine;

namespace TestRecycleListView.UI
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
