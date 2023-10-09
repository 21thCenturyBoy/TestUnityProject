using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.GOPool;

public class TestGameObjectPool : MonoBehaviour
{
    public GameObject prefabCube;
    public GameObject prefabSphere;

    [SerializeField] private SingleGOPool SingleGOPool_Cylinder;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        SingleGOPool_Cylinder.OnSearchPrefab = () => Resources.Load<GameObject>("Cylinder");
        while (true)
        {
            yield return new WaitForSeconds(2f);
            Spawn();
        }
    }

    private List<GameObject> m_ObjList = new List<GameObject>();
    private List<GameObject> m_SingleGOPoolList = new List<GameObject>();
    void Spawn()
    {
        m_ObjList.Add(MultiGOPool.Instance.Spawn(prefabCube, transform)); 
        m_ObjList.Add(MultiGOPool.Instance.Spawn(prefabSphere, transform));

        m_SingleGOPoolList.Add(SingleGOPool_Cylinder.Spawn());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            foreach (GameObject o in m_ObjList)
            {
                Debug.LogError("MultiGOPool回收:" + MultiGOPool.Instance.Recycle(o));
            }
            m_ObjList.Clear();

            foreach (GameObject o in m_SingleGOPoolList)
            {
                Debug.LogError("SingleGOPool回收:" + SingleGOPool_Cylinder.Recycle(o));
            }
            m_SingleGOPoolList.Clear();
        }
    }
}
