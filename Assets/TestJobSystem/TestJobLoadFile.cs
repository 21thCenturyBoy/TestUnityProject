using System.IO;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class TestJobLoadFile : MonoBehaviour
{
    public enum LoadState
    {
        Verify,
        Download,
        Decompress,
        LoadMap,
    }
    public class LoadMapFile
    {
        
    }
    public void Update()
    {
    }
}