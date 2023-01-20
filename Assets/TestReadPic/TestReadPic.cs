using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace TestReadPic
{
    public class TestReadPic : MonoBehaviour
    {
        public Texture2D ReadTexture;
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
            sb2.AppendLine("local array = [");
            for (int i = 1; i < ReadTexture.height - 2; i = i + 4)
            {
                for (int j = 1; j < ReadTexture.width - 2; j = j + 4)
                {

                    int num = 0;
                    if (readBytes[i - 1, j - 1] == 1) num++;
                    if (readBytes[i - 1, j] == 1) num++;
                    if (readBytes[i - 1, j + 1] == 1) num++;
                    if (readBytes[i - 1, j + 2] == 1) num++;

                    if (readBytes[i, j - 1] == 1) num++;
                    if (readBytes[i, j] == 1) num++;
                    if (readBytes[i, j + 1] == 1) num++;
                    if (readBytes[i, j + 2] == 1) num++;

                    if (readBytes[i + 1, j - 1] == 1) num++;
                    if (readBytes[i + 1, j] == 1) num++;
                    if (readBytes[i + 1, j + 1] == 1) num++;
                    if (readBytes[i + 1, j + 2] == 1) num++;

                    if (readBytes[i + 2, j - 1] == 1) num++;
                    if (readBytes[i + 2, j] == 1) num++;
                    if (readBytes[i + 2, j + 1] == 1) num++;
                    if (readBytes[i + 2, j + 2] == 1) num++;


                    if (num >= 4)
                    {
                        readBytes[i - 1, j - 1] = 0;
                        readBytes[i - 1, j] = 0;
                        readBytes[i - 1, j + 1] = 0;
                        readBytes[i - 1, j + 2] = 0;

                        readBytes[i, j - 1] = 0;
                        readBytes[i, j] = 0;
                        readBytes[i, j + 1] = 0;
                        readBytes[i, j + 2] = 0;

                        readBytes[i + 1, j - 1] = 0;
                        readBytes[i + 1, j] = 0;
                        readBytes[i + 1, j + 1] = 0;
                        readBytes[i + 1, j + 2] = 0;

                        readBytes[i + 2, j - 1] = 0;
                        readBytes[i + 2, j] = 0;
                        readBytes[i + 2, j + 1] = 0;
                        readBytes[i + 2, j + 2] = 1;
                    }
                    else
                    {
                        readBytes[i - 1, j - 1] = 0;
                        readBytes[i - 1, j] = 0;
                        readBytes[i - 1, j + 1] = 0;
                        readBytes[i - 1, j + 2] = 0;

                        readBytes[i, j - 1] = 0;
                        readBytes[i, j] = 0;
                        readBytes[i, j + 1] = 0;
                        readBytes[i, j + 2] = 0;

                        readBytes[i + 1, j - 1] = 0;
                        readBytes[i + 1, j] = 0;
                        readBytes[i + 1, j + 1] = 0;
                        readBytes[i + 1, j + 2] = 0;

                        readBytes[i + 2, j - 1] = 0;
                        readBytes[i + 2, j] = 0;
                        readBytes[i + 2, j + 1] = 0;
                        readBytes[i + 2, j + 2] = 0;
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

        // Update is called once per frame
        void Update()
        {

        }
    }

}
