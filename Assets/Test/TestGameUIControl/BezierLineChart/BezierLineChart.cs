using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils.Ease;

[RequireComponent(typeof(Image))]
[ExecuteInEditMode]
public class BezierLineChart : BaseMeshEffect
{
    public Image image;
    [Tooltip("设定值")]
    /// <summary>
    /// 设定值
    /// </summary>
    public List<float> valueList = new List<float>();
    [Tooltip("两值的间隔")]
    /// <summary>
    /// 两值的间隔
    /// </summary>
    public float valueInternal = 1;
    /// <summary>
    /// 幅度
    /// </summary>
    [Tooltip("振幅")]
    public float amplitude = 1;
    [Tooltip("采样密度")]
    /// <summary>
    /// 贝塞尔点的密度
    /// </summary>
    [Range(5, 100)]
    public int density = 10;

    /// <summary>
    /// 线宽
    /// </summary>
    public float lineWidth = 1;
    /// <summary>
    /// 最高点
    /// </summary>
    public float heightPos;


    [Tooltip("线的颜色")]
    public Color lineColor = Color.white;
    [Tooltip("填充颜色0")]
    public Color fillColor0 = Color.white;
    [Tooltip("填充颜色1")]
    public Color fillColor1 = Color.white;
    [Tooltip("是否填充面积颜色")]
    public bool fillAreaColor;






    private List<Vector2> points = new List<Vector2>();
    /// <summary>
    /// 两点之间的定位点
    /// </summary>
    private List<Vector2> controlPointList = new List<Vector2>();
    //计算出的点放这里
    private List<Vector3> resList = new List<Vector3>();
    /// <summary>
    /// 矩形的四角的点
    /// </summary>
    private List<Vector3> quadAnchorList = new List<Vector3>();

    protected override void Awake()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }
    }

    private void Update()
    {


        image.SetVerticesDirty();
    }
    public override void ModifyMesh(VertexHelper vh)
    {
        vh.Clear();
        points.Clear();
        quadAnchorList.Clear();
        heightPos = 0;


        for (int i = 0; i < valueList.Count; i++)
        {
            points.Add(new Vector2(i * valueInternal, valueList[i] * amplitude));
            //当前值大于最高值就替换
            if (valueList[i] * amplitude > heightPos)
            {
                heightPos = valueList[i] * amplitude;
            }
        }



        resList.Clear();
        controlPointList.Clear();

        for (int i = 0; i < points.Count - 1; i++)
        {
            //先取两点中间点
            controlPointList.Add(new Vector2((points[i] + points[i + 1]).x / 2, points[i].y));
            controlPointList.Add(new Vector2((points[i] + points[i + 1]).x / 2, points[i + 1].y));

        }







        for (int i = 0; i < points.Count - 1; i++)
        {
            //todo:用三阶贝塞尔曲线,不然结果是奇怪的波形
            //设定贝塞尔曲线采样密度
            for (int j = 0; j < density; j++)
            {
                resList.Add(BezierMath.Bezier_3(points[i], controlPointList[i * 2], controlPointList[i * 2 + 1], points[i + 1], j * (1 / (float)density)));
            }
        }
        resList.Add(points[points.Count - 1]);


        for (int i = 0; i < resList.Count - 1; i++)
        {
            DrawQuad(vh, resList[i], resList[i + 1], i);
        }
        DrawTriangle(vh);
        FillAreaColor(vh);




    }


    /// <summary>
    /// 设置数据
    /// </summary>
    public void SetData(List<float> val)
    {
        valueList = val;
    }

    /// <summary>
    /// 画方块,给两点位置,同时给startindex的序号
    /// </summary>
    public void DrawQuad(VertexHelper vh, Vector2 startPos, Vector2 endPos, int index)
    {

        var normal = endPos - startPos;//路径向量
        float angle = Vector2.SignedAngle(Vector2.right, normal);//算出向量与x轴正方向角度
        angle *= Mathf.Deg2Rad;//一定要转成弧度
        var _pList = new List<Vector3>();
        Vector3 p0, p1, p2, p3;

        //两点间距离
        float length = Vector2.Distance(startPos, endPos);


        //先把矩形沿x轴横躺
        p0 = new Vector3(startPos.x, startPos.y + lineWidth);
        p1 = new Vector3(startPos.x, startPos.y - lineWidth);
        p2 = new Vector3(startPos.x + length, startPos.y + lineWidth);
        p3 = new Vector3(startPos.x + length, startPos.y - lineWidth);
        //四个点按照0   3
        //          1   2
        //的顺序
        _pList.Add(p0);
        _pList.Add(p1);
        _pList.Add(p2);
        _pList.Add(p3);

        //然后四个点沿着startpos旋转

        for (int i = 0; i < _pList.Count; i++)
        {
            //平移
            var _p = new Vector3(_pList[i].x - startPos.x, _pList[i].y - startPos.y, 0);

            //旋转
            _p = new Vector3(Mathf.Cos(angle) * _p.x - Mathf.Sin(angle) * _p.y,
                Mathf.Sin(angle) * _p.x + Mathf.Cos(angle) * _p.y,
                0);

            //反平移
            _p += new Vector3(startPos.x, startPos.y);
            _pList[i] = _p;
        }




        foreach (var i in _pList)
        {
            vh.AddVert(i, lineColor, Vector2.zero);
            //加到填缝的列表里
            quadAnchorList.Add(i);
        }


        //添加三角形
        vh.AddTriangle(index * 4, index * 4 + 1, index * 4 + 2);
        vh.AddTriangle(index * 4 + 1, index * 4 + 2, index * 4 + 3);

    }

    /// <summary>
    /// 方块与方块之间有空隙,用三角形补上
    /// </summary>
    public void DrawTriangle(VertexHelper vh)
    {
        //0  3 | 0'  3'
        //1  2 | 1'  2'
        //这个是两个方块

        //0 | 3  0' | 3'
        //1 | 2  1' | 2'
        //在这中间再插一个方块补缝

        for (int i = 0; i < quadAnchorList.Count / 4 - 1; i++)
        {
            vh.AddVert(quadAnchorList[i * 4 + 3], lineColor, Vector2.zero);
            vh.AddVert(quadAnchorList[i * 4 + 2], lineColor, Vector2.zero);
            vh.AddVert(quadAnchorList[(i + 1) * 4 + 1], lineColor, Vector2.zero);
            vh.AddVert(quadAnchorList[(i + 1) * 4], lineColor, Vector2.zero);
        }

        for (int i = resList.Count - 1; i < resList.Count * 2 - 3; i++)
        {
            vh.AddTriangle(i * 4, i * 4 + 1, i * 4 + 2);
            vh.AddTriangle(i * 4 + 1, i * 4 + 2, i * 4 + 3);
        }


    }


    /// <summary>
    /// 填充面积颜色
    /// </summary>
    public void FillAreaColor(VertexHelper vh)
    {
        if (!fillAreaColor)
        {
            return;
        }

        for (int i = 0; i < resList.Count; i++)
        {
            var p0 = resList[i];
            var p1 = resList[i];
            p1.y = 0;
            vh.AddVert(p0, ComputeColor(p0.y), Vector2.zero);
            vh.AddVert(p1, ComputeColor(p1.y), Vector2.zero);
        }
        //从补缝的下一个三角形开始
        //0   2
        //1   3
        int startIndex = (resList.Count * 2 - 3) * 4;
        for (int i = startIndex; i < startIndex + resList.Count * 2 - 2; i += 2)
        {

            var p0 = i;
            var p1 = i + 1;
            var p2 = i + 2;
            var p3 = i + 3;

            vh.AddTriangle(p0, p1, p3);
            vh.AddTriangle(p0, p2, p3);
        }

    }

    /// <summary>
    /// 计算当前高度点的颜色
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private Color ComputeColor(float pos)
    {
        float rate = pos / (heightPos - 0);

        return Color.Lerp(fillColor1, fillColor0, rate);
    }
}

