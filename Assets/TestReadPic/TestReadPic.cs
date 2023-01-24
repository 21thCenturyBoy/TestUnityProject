using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace TestReadPic
{
    public class TestReadPic : MonoBehaviour
    {
        [SerializeField]
        public Texture2D ReadTexture;

        [SerializeField]
        private int m_CombineCol_Max = 4;
        [SerializeField]
        private int m_CombineRow_Max = 4;
        [SerializeField]
        private int m_CombineLimit = 4;
        // Start is called before the first frame update
        void Start()
        {
            Color[] colors = ReadTexture.GetPixels();
            int[,] readBytes = new int[ReadTexture.height, ReadTexture.width];
            int row = -1;
            int col = -1;
            for (int i = 0; i < colors.Length; i++)
            {
                col = i % ReadTexture.width;
                if (col == 0)
                {
                    row++;
                }
                if (colors[i].a < 0.99)
                {
                    readBytes[row, col] = 0;
                }
                else
                {
                    readBytes[row, col] = 1;
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("P1");
            sb.AppendLine($"{ReadTexture.width} {ReadTexture.height}");
            for (int i = 0; i < ReadTexture.height; i++)
            {
                for (int j = 0; j < ReadTexture.width; j++)
                {
                    sb.Append(readBytes[i, j]);
                }
                sb.AppendLine();
            }

            File.WriteAllText(Application.streamingAssetsPath + "/test.pbm", sb.ToString(), new UTF8Encoding(false));
            StringBuilder sb2 = new StringBuilder();

            int rowLength = readBytes.GetLength(0);
            int colLength = readBytes.GetLength(1);
            void TraversePoint(int row, int col, int rowLen, int colLen, Action<int, int> traverseAct)
            {
                for (int rowi = row; rowi < rowLen; rowi++)
                {
                    if (rowi >= rowLength) break;
                    for (int colj = col; colj < colLen; colj++)
                    {
                        if (colj >= colLength) break;
                        traverseAct?.Invoke(rowi, colj);
                    }
                }
            }

            sb2.AppendLine("local array = [");

            for (int i = 0; i < ReadTexture.height; i = i + m_CombineRow_Max)
            {
                for (int j = 0; j < ReadTexture.width; j = j + m_CombineCol_Max)
                {
                    int num = 0;
                    TraversePoint(i, j, i + m_CombineRow_Max, j + m_CombineCol_Max, (x, y) =>
                    {
                        if (readBytes[x, y] == 1) num++;
                    });

                    if (num >= m_CombineLimit)
                    {
                        TraversePoint(i, j, i + m_CombineRow_Max, j + m_CombineCol_Max, (x, y) =>
                        {
                            if (x == i && y == j) readBytes[x, y] = 1;
                            else readBytes[x, y] = 0;
                        });
                    }
                    else
                    {
                        TraversePoint(i, j, i + m_CombineRow_Max, j + m_CombineCol_Max, (x, y) =>
                       {
                           readBytes[x, y] = 0;
                       });
                    }
                }
            }
            for (int i = 0; i < ReadTexture.height; i++)
            {
                for (int j = 0; j < ReadTexture.width; j++)
                {
                    sb2.Append(readBytes[i, j] + ",");
                }
                sb2.AppendLine();
            }

            sb2.AppendLine("]");
            File.WriteAllText(Application.streamingAssetsPath + "/test2.txt", sb2.ToString(), new UTF8Encoding(false));
        }
    }

}
