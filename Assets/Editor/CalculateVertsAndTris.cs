using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEditor;

public class CalculateVertsAndTris : EditorWindow
{

    public static int verts;
    public static int tris;
    public static int count;

    [MenuItem("Tools/Planner/CalculateVertsAndTris Windows")]
    static void CreateCalculateVertsAndTrisWindows()
    {
        CalculateVertsAndTris m_window = EditorWindow.GetWindow(typeof(CalculateVertsAndTris), false, "CalculateVertsAndTris Windows", false) as CalculateVertsAndTris;
        m_window.Show();
    }
    void OnGUI()
    {
        GetAllObjects();
        GUILayout.Label($"ɸѡGameObject������" + count);
        string vertsdisplay = verts.ToString("#,##0 verts");
        GUILayout.Label($"��������" + vertsdisplay);
        string trisdisplay = tris.ToString("#,##0 tris");
        GUILayout.Label($"�����棺" + trisdisplay);
    }
    /// <summary>
    /// �õ����������е�GameObject
    /// </summary>
    void GetAllObjects()
    {
        verts = 0;
        tris = 0;
        count = 0;
        MeshFilter[] ob = FindObjectsOfType(typeof(MeshFilter)) as MeshFilter[];
        GetAllVertsAndTris(ob);
    }
    //�õ�������Ͷ�����
    void GetAllVertsAndTris(MeshFilter[] obj)
    {
        var filters = obj.Where(obj => obj.GetComponent<MeshRenderer>() != null);
        count += filters.Count();
        foreach (MeshFilter f in filters)
        {
            tris += f.sharedMesh.triangles.Length / 3;
            verts += f.sharedMesh.vertexCount;
        }
    }
}