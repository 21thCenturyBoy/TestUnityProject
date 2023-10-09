using UnityEngine;
using Utils.GOPool;

public class TestPrefab : MonoBehaviour,IGamePrefab
{
    public int PrefabInstId { get; set; }
    public void OnPrefabAwake()
    {
        Debug.LogError("OnPrefabAwake"+PrefabInstId);
    }

    public void OnPrefabDestroy()
    {
        Debug.LogError("OnPrefabDestroy"+PrefabInstId);
    }
}
