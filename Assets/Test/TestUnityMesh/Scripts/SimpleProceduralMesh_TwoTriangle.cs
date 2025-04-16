using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 创建一个三角形
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public class SimpleProceduralMesh_TwoTriangle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }
    void OnEnable()
    {
        var mesh = new Mesh
        {
            name = "Procedural Mesh"
        };

        mesh.vertices = new Vector3[] {
            Vector3.zero, Vector3.right, Vector3.up,
            new Vector3(1.1f, 0f), new Vector3(0f, 1.1f), new Vector3(1.1f, 1.1f)
        };
        mesh.triangles = new int[] {
            0,2,1,3,4,5//顺时针
        };
        mesh.normals = new Vector3[] {
            Vector3.back, Vector3.back, Vector3.back,
            Vector3.back, Vector3.back, Vector3.back,
        };
        mesh.uv = new Vector2[] {
            Vector2.zero, Vector2.right, Vector2.up,
            Vector2.right, Vector2.up, Vector2.one,
        };
        //2-V(0,1,0)-N(0,0,-1)-UV(1,0)
        //
        //0-V(0,0,0)-N(0,0,-1)-UV(0,0)          1-V(1,0,0)-N(0,0,-1)-UV(1,1)
        mesh.tangents = new Vector4[] {
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
        };
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
