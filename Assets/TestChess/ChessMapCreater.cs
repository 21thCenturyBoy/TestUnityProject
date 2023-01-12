using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[Serializable]
public struct Coordinate
{
    public int Row;
    public int Col;

    public Coordinate(int row, int col)
    {
        Row = row;
        Col = col;
    }
}

[Serializable]
public struct HoneycombVector2
{
    public int X;
    public int Y;

    public HoneycombVector2(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(HoneycombVector2 other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object obj)
    {
        return obj is HoneycombVector2 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static bool operator ==(HoneycombVector2 left, HoneycombVector2 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(HoneycombVector2 left, HoneycombVector2 right)
    {
        return !left.Equals(right);
    }

    public HoneycombVector2[] GetNeighbors()
    {
        HoneycombVector2[] vector2s = new HoneycombVector2[6];
        vector2s[0] = new HoneycombVector2(X + 1, Y + 1);
        vector2s[1] = new HoneycombVector2(X, Y + 2);
        vector2s[2] = new HoneycombVector2(X - 1, Y + 1);
        vector2s[3] = new HoneycombVector2(X - 1, Y - 1);
        vector2s[4] = new HoneycombVector2(X, Y - 2);
        vector2s[5] = new HoneycombVector2(X + 1, Y - 1);

        return vector2s;
    }
}

[Serializable]
public class ChessMapItem
{

}
[Serializable]
public class ChessMapData
{

}
[Serializable]
public enum GenerateMode
{
    /// <summary> 一般 </summary>
    Normal,
    /// <summary> 扩散 </summary>
    Spread,
}

/// <summary>
/// 棋盘生成
/// </summary>
public class ChessMapCreater : MonoBehaviour
{
    public HoneycombVector2 CoordinatePoint = new HoneycombVector2(0, 0);
    public Coordinate MaxSize = new Coordinate(10, 10);
    public Coordinate MinSize = new Coordinate(-10, -10);

    public Vector3 ChessSpacing = new Vector3(5, 0, 3);
    public float GenerateInterval = 0.5f;

    [SerializeField]
    private Vector3 m_InitPosition = new Vector3(0, 0, 0);
    [SerializeField]
    private GameObject m_PrefabChessObject;

    public GenerateMode Mode;

    private List<ChessItemComponent> m_cachelist = new List<ChessItemComponent>();

    public static ChessItemComponent StartChess;
    public static ChessItemComponent EndChess;
    // Start is called before the first frame update
    void Start()
    {
        switch (Mode)
        {
            case GenerateMode.Normal:
            case GenerateMode.Spread:
                NormalGenerate();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void NormalGenerate()
    {
        StartCoroutine(GenerateItem());


        List<ChessItemComponent> items = new List<ChessItemComponent>();

        IEnumerator GenerateItem()
        {
            HashSet<HoneycombVector2> starts = new HashSet<HoneycombVector2>();
            starts.Add(CoordinatePoint);

            HashSet<HoneycombVector2> ends = new HashSet<HoneycombVector2>();

            while (starts.Count != 0)
            {
                yield return new WaitForSeconds(GenerateInterval);
                var chess = starts.First();
                m_cachelist.Add(CreateChessItem(chess)); 
                var list = chess.GetNeighbors().Where(v2 => v2.X > MinSize.Row && v2.X < MaxSize.Row && v2.Y > MinSize.Col && v2.Y < MaxSize.Col && !ends.Contains(v2));
                starts.UnionWith(list);
                starts.Remove(chess);
                ends.Add(chess);
            }
        }
    }

    private ChessItemComponent CreateChessItem(HoneycombVector2 point)
    {
        Vector3 pos = new Vector3(point.X * ChessSpacing.x + m_InitPosition.x, m_InitPosition.y, point.Y * ChessSpacing.z + m_InitPosition.z);

        var com = GameObject.Instantiate(m_PrefabChessObject, pos, Quaternion.identity, transform).AddComponent<ChessItemComponent>();
        com.Point = point;

        return com;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                var chess = hit.transform.GetComponent<ChessItemComponent>();
                if (chess != null)
                {
                    if (StartChess!=null)
                    {
                        StartChess.SetColor(ChessItemComponent.DefaultColor);
                    }

                    if (EndChess == chess) EndChess = null;

                    StartChess = chess;
                    StartChess.SetColor(ChessItemComponent.StartColor);
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                var chess = hit.transform.GetComponent<ChessItemComponent>();
                if (chess != null)
                {
                    if (EndChess != null)
                    {
                        EndChess.SetColor(ChessItemComponent.DefaultColor);
                    }

                    if (StartChess == chess) StartChess = null;

                    EndChess = chess;
                    EndChess.SetColor(ChessItemComponent.EndColor);
                }
            }
        }
    }
}
