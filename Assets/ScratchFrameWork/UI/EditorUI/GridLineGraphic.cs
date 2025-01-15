using UnityEngine;
using UnityEngine.UI;

namespace ScratchFramework
{
   [RequireComponent(typeof(RectTransform))]
   public class GridLineGraphic : MaskableGraphic
   {
       [Header("Grid Settings")]
       public int rows = 10;
       public int columns = 10;
       public Color lineColor = Color.black;
       public float lineThickness = 1f;

       public float cellHeight = 1f;
       public float cellWidth = 1f;
       protected override void OnPopulateMesh(VertexHelper vh)
       {
           vh.Clear();
           Rect rect = GetPixelAdjustedRect();
           float width = rect.width;
           float height = rect.height;
   
           // float cellWidth = width / columns;
           // float cellHeight = height / rows;
   
           // 设置线条颜色
           Color currentColor = lineColor;
           UIVertex vert = UIVertex.simpleVert;
           vert.color = currentColor;
   
           // 绘制水平线
           for (int i = 0; i <= rows; i++)
           {
               float y = rect.yMin + i * cellHeight;
               AddQuad(vh, new Vector2(rect.xMin, y), new Vector2(rect.xMax, y + lineThickness));
           }
   
           // 绘制垂直线
           for (int j = 0; j <= columns; j++)
           {
               float x = rect.xMin + j * cellWidth;
               AddQuad(vh, new Vector2(x, rect.yMin), new Vector2(x + lineThickness, rect.yMax));
           }
       }
   
       void AddQuad(VertexHelper vh, Vector2 start, Vector2 end)
       {
           UIVertex vert = UIVertex.simpleVert;
           vert.color = lineColor;
   
           // 左下
           vert.position = new Vector3(start.x, start.y);
           vh.AddVert(vert);
   
           // 右下
           vert.position = new Vector3(end.x, start.y);
           vh.AddVert(vert);
   
           // 右上
           vert.position = new Vector3(end.x, end.y);
           vh.AddVert(vert);
   
           // 左上
           vert.position = new Vector3(start.x, end.y);
           vh.AddVert(vert);
   
           int startIndex = vh.currentVertCount - 4;
           vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
           vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
       }
   } 
}
