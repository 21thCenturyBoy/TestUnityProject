using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Sprites;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(ColorsWheel))]
public class ColorsWheelEditor : ImageEditor
{
    SerializedProperty segments;

    SerializedProperty colors;

    protected override void OnEnable()
    {
        base.OnEnable();

        segments = serializedObject.FindProperty("m_Segments");
        colors = serializedObject.FindProperty("Colors");
    }

    // 在Inspector中重写Toggle的显示
    public override void OnInspectorGUI()
    {
        // 更新序列化对象
        serializedObject.Update();

        base.OnInspectorGUI();

        EditorGUILayout.PropertyField(segments);
        EditorGUILayout.PropertyField(colors);

        serializedObject.ApplyModifiedProperties();
    }
}

#endif
public class ColorsWheel : Image
{
    [Min(1)] [SerializeField] private int m_Segments = 100;

    public Color32[] Colors = new[] { new Color32(255, 255, 255, 255) };

    private float[] m_PerPercents;
    private float m_PerPercent;

    private int pointNum = 0;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (Colors.Length == 0)
        {
            Colors = new[] { new Color32(255, 255, 255, 255) };
        }

        if (m_PerPercents == null || m_PerPercents.Length != Colors.Length)
        {
            int len = Colors.Length;
            m_PerPercents = new float[len];

            m_PerPercent = 1f / len;
            for (int i = 0; i < len; i++)
            {
                m_PerPercents[i] = Mathf.Clamp01((i + 1) * m_PerPercent);
            }
        }

        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        Vector2 originPos = GetOriginPos(width, height);

        pointNum = 0;
        int colorSegmentIndex = 0;
        for (int i = 0; i < m_PerPercents.Length; i++)
        {
            UIVertex origin = new UIVertex();
            origin.color = Colors[colorSegmentIndex];
            origin.position = originPos;

            vh.AddVert(origin);
            pointNum++;
            colorSegmentIndex++;
        }

        colorSegmentIndex = 0;
        float radian = Mathf.PI * 2 / m_Segments;
        float curRadian = 0;
        float radius = width * 0.5f;

        for (int i = 0; i < m_Segments + 1; i++)
        {
            if (colorSegmentIndex == Colors.Length) break;
            float x = Mathf.Cos(curRadian) * radius;
            float y = Mathf.Sin(curRadian) * radius;

            curRadian += radian;
            Vector2 xy = new Vector2(x, y);

            float colorSegment = i / (float)m_Segments;
            bool ifBig = colorSegment >= m_PerPercents[colorSegmentIndex];
            UIVertex uvTemp = new UIVertex();
            uvTemp.color = Colors[colorSegmentIndex];
            uvTemp.position = xy + originPos;
            pointNum++;

            vh.AddVert(uvTemp);
            if (ifBig)
            {
                colorSegmentIndex++;
            }
        }

        colorSegmentIndex = 0;
        int id = m_PerPercents.Length;
        for (int i = 0; i < pointNum; i++)
        {
            if (colorSegmentIndex == Colors.Length) break;

            float colorSegment = i / (float)m_Segments;
            bool ifBig = colorSegment >= m_PerPercents[colorSegmentIndex];
            if (ifBig)
            {
                colorSegmentIndex++;
            }
            else
            {
                vh.AddTriangle(id, colorSegmentIndex, id + 1);
            }

            id++;
        }
    }

    private Vector2 GetConvetRatio(float uvWidth, float uvHeight, float width, float height)
    {
        Vector2 convertRatio = new Vector2(uvWidth / width, uvHeight / height);
        return convertRatio;
    }

    private Vector2 GetUvCenter(float uvWidth, float uvHeight)
    {
        Vector2 center = new Vector2(uvWidth * 0.5f, uvHeight * 0.5f);
        return center;
    }

    private Vector2 GetOriginPos(float width, float height)
    {
        Vector2 originPos = new Vector2((0.5f - rectTransform.pivot.x) * width, ((0.5f - rectTransform.pivot.y) * height));
        return originPos;
    }

    private Vector2 GetUV(Sprite sprite)
    {
        Vector4 temp = sprite != null ? DataUtility.GetOuterUV(sprite) : Vector4.zero;
        float uvHeight = temp.w - temp.y;
        float uvWidth = temp.z - temp.x;
        Vector2 uv = new Vector2(uvWidth, uvHeight);
        return uv;
    }
}