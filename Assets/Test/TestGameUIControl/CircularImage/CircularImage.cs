using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Sprites;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(CircularImage))]
public class CircularImageEditor : ImageEditor
{
    SerializedProperty showPercent;
    SerializedProperty segments;


    protected override void OnEnable()
    {
        base.OnEnable();

        showPercent = serializedObject.FindProperty("m_ShowPercent");
        segments = serializedObject.FindProperty("m_Segments");
    }

    // 在Inspector中重写Toggle的显示
    public override void OnInspectorGUI()
    {
        // 更新序列化对象
        serializedObject.Update();

        base.OnInspectorGUI();

        EditorGUILayout.PropertyField(showPercent);
        EditorGUILayout.PropertyField(segments);

        serializedObject.ApplyModifiedProperties();
    }
}

#endif
public class CircularImage : Image
{
    [SerializeField] private float m_ShowPercent = 1f;
    [SerializeField] private int m_Segments = 100;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        Vector2 uv = GetUV(overrideSprite);
        Vector2 convertRatio = GetConvetRatio(uv.x, uv.y, width, height);
        Vector2 uvCenter = GetUvCenter(uv.x, uv.y);
        Vector2 originPos = GetOriginPos(width, height);
        UIVertex origin = new UIVertex();
        //设置原点颜色渐变
        byte c = (byte)(m_ShowPercent * 255);
        origin.color = new Color32(c, c, c, 255);
        origin.position = originPos;
        origin.uv0 = new Vector2(Vector2.zero.x * convertRatio.x + uvCenter.x, Vector2.zero.y * convertRatio.y + uvCenter.y);
        vh.AddVert(origin);

        float radian = Mathf.PI * 2 / m_Segments;
        float curRadian = 0;
        float radius = width * 0.5f;
        int realSegment = (int)(m_ShowPercent * m_Segments);
        for (int i = 0; i < m_Segments + 1; i++)
        {
            //看上图，计算A点X和Y的坐标
            float x = Mathf.Cos(curRadian) * radius;
            float y = Mathf.Sin(curRadian) * radius;
            //弧度增加，下一个计算B点
            curRadian += radian;
            //存储X和Y的信息
            Vector2 xy = new Vector2(x, y);
            //存储每个点的信息
            UIVertex uvTemp = new UIVertex();
            if (i < realSegment)
            {
                //该点对应的颜色
                uvTemp.color = color;
            }
            else
            {
                if (realSegment == m_Segments)
                {
                    uvTemp.color = color;
                }
                else
                {
                    uvTemp.color = new Color32(60, 60, 60, 255);
                }
            }
            //存储该点在实际精灵中的坐标，OriginPos是精灵的圆点
            uvTemp.position = xy + originPos;
            //UV坐标计算
            uvTemp.uv0 = new Vector2(xy.x * convertRatio.x + uvCenter.x, xy.y * convertRatio.y + uvCenter.y);
            vh.AddVert(uvTemp);
        }

        //id就是我们放入每个点的次序，A就对应1，B对应2，C对应3
        int id = 1;
        for (int i = 0; i < m_Segments; i++)
        {
            vh.AddTriangle(id, 0, id + 1);
            //上面这个方法会自动按照ID顺序查找点的信息，并将这三个点构成三角形绘制出来
            id++;
        }
    }
    /// <summary>
    /// 计算转换比例
    /// </summary>
    /// <param name="uvWidth">uv宽度</param>
    /// <param name="uvHeight">uv长度</param>
    /// <param name="width">精灵宽度</param>
    /// <param name="height">精灵长度</param>
    /// <returns></returns>
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
    /// <summary>
    /// 获取精灵（图片）的uv长宽
    /// </summary>
    /// <param name="sprite">传入当前精灵（图片）</param>
    /// <returns></returns>
    private Vector2 GetUV(Sprite sprite)
    {
        Vector4 temp = sprite != null ? DataUtility.GetOuterUV(sprite) : Vector4.zero;
        float uvHeight = temp.w - temp.y;
        float uvWidth = temp.z - temp.x;
        Vector2 uv = new Vector2(uvWidth, uvHeight);
        return uv;
    }
}