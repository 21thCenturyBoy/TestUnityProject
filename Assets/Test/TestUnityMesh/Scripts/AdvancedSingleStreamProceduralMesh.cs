using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;

using Unity.Mathematics;

using static Unity.Mathematics.math;

/// <summary>
/// 单流网格
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AdvancedSingleStreamProceduralMesh : MonoBehaviour
{
    //避免C# 编译器可能会重新安排内容以优化我们的代码
    [StructLayout(LayoutKind.Sequential)]
    struct Vertex
    {
        public float3 position, normal;
        public half4 tangent;
        public half2 texCoord0;
    }
    void OnEnable()
    {
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        //生成1个网格
        Mesh.MeshData meshData = meshDataArray[0];

        int vertexAttributeCount = 4;//定义属性个数：位置、法线、切线和一组纹理坐标
        int vertexCount = 4;//顶点数
        int triangleIndexCount = 6;//设置三角形索引

        var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
            vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory
        );
        //顶点属性初始化
        vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);//---------------------------------------------------1个流
        vertexAttributes[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3 );
        vertexAttributes[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float16, 4);
        vertexAttributes[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2);

        meshData.SetVertexBufferParams(vertexCount, vertexAttributes);//设置顶点个数，属性
        vertexAttributes.Dispose();

        //设置顶点
        NativeArray<Vertex> vertices = meshData.GetVertexData<Vertex>();

        half h0 = half(0f), h1 = half(1f);

        var vertex = new Vertex
        {
            normal = back(),
            tangent = half4(h1, h0, h0, half(-1f))
        };

        vertex.position = 0f;
        vertex.texCoord0 = h0;
        vertices[0] = vertex;

        vertex.position = right();
        vertex.texCoord0 = half2(h1, h0);
        vertices[1] = vertex;

        vertex.position = up();
        vertex.texCoord0 = half2(h0, h1);
        vertices[2] = vertex;

        vertex.position = float3(1f, 1f, 0f);
        vertex.texCoord0 = h1;
        vertices[3] = vertex;


        //设置三角形
        meshData.SetIndexBufferParams(triangleIndexCount, IndexFormat.UInt16);
        NativeArray<ushort> triangleIndices = meshData.GetIndexData<ushort>();
        triangleIndices[0] = 0;
        triangleIndices[1] = 2;
        triangleIndices[2] = 1;
        triangleIndices[3] = 1;
        triangleIndices[4] = 2;
        triangleIndices[5] = 3;

        //设置网格边界
        var bounds = new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));

        //设置子网格
        meshData.subMeshCount = 1;
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangleIndexCount)
        {
            bounds = bounds,
            vertexCount = vertexCount
        }, MeshUpdateFlags.DontRecalculateBounds);

        var mesh = new Mesh
        {
            bounds = bounds,
            name = "Procedural Mesh"
        };

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);//写入网格
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
