using System;
using UnityEngine;

namespace Utils.GOPool
{
    public class SingleGOPool : BaseGOPool
    {
        protected GameObject m_Prefab;
        protected GameObject Prefab
        {
            get
            {
                if (m_Prefab == null) m_Prefab = OnSearchPrefab?.Invoke();
                return m_Prefab;
            }
        }
        public Func<GameObject> OnSearchPrefab;

        public GameObject Spawn(Transform parent = null, bool createIfPoolEmpty = true)
        {
            return Spawn(Prefab, parent, createIfPoolEmpty);
        }

        public override bool Recycle(GameObject obj)
        {
            if (obj == null) return false;
            var prefab = obj.GetComponent<IGamePrefab>();
            if (prefab == null || Prefab.GetInstanceID() != prefab.PrefabInstId)
            {
                Destroy(obj);
                return true;
            }
            return base.Recycle(obj);
        }
    }
}


