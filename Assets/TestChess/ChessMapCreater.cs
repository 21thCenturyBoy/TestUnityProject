using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
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
public enum HeapType
{
    MinHeap,
    MaxHeap
}

public class BinaryHeap<T> where T : IComparable<T>
{
    public List<T> items;
    public HeapType HType { get; private set; }

    public T Root
    {
        get { return items[0]; }
    }

    public BinaryHeap(HeapType type)
    {
        items = new List<T>();
        this.HType = type;
    }

    public bool Contains(T data)
    {
        return items.Contains(data);
    }



    public void Push(T item)
    {
        items.Add(item);

        int i = items.Count - 1;

        bool flag = HType == HeapType.MinHeap;

        while (i > 0)
        {
            if ((items[i].CompareTo(items[(i - 1) / 2]) > 0) ^ flag)
            {
                //T temp = items[i];
                //items[i] = items[(i - 1) / 2];
                //items[(i - 1) / 2] = temp;
                (items[i], items[(i - 1) / 2]) = (items[(i - 1) / 2], items[i]);
                i = (i - 1) / 2;
            }
            else
                break;
        }
    }

    private void DeleteRoot()
    {
        int i = items.Count - 1;

        items[0] = items[i];
        items.RemoveAt(i);

        i = 0;

        bool flag = HType == HeapType.MinHeap;

        while (true)
        {
            int leftInd = 2 * i + 1;
            int rightInd = 2 * i + 2;
            int largest = i;

            if (leftInd < items.Count)
            {
                if ((items[leftInd].CompareTo(items[largest]) > 0) ^ flag)
                    largest = leftInd;
            }

            if (rightInd < items.Count)
            {
                if ((items[rightInd].CompareTo(items[largest]) > 0) ^ flag)
                    largest = rightInd;
            }

            if (largest != i)
            {
                //T temp = items[largest];
                //items[largest] = items[i];
                //items[i] = temp;
                (items[largest], items[i]) = (items[i], items[largest]);
                i = largest;
            }
            else
                break;
        }
    }

    public T PopRoot()
    {
        T result = items[0];

        DeleteRoot();

        return result;
    }
}
[Serializable]
public struct HoneycombVector2
{
    public int X => m_x;
    public int Y => m_y;
    private int m_x;
    private int m_y;

    public HoneycombVector2(int x, int y)
    {
        m_x = x;
        m_y = y;
    }

    public int GetCartesian_X()
    {
        if (m_x % 2 == 1)//x为奇数
        {
            return (m_x + 1) / 2;
        }
        else return m_x / 2;
    }
    public int GetCartesian_Y()
    {
        return m_y;
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

    public HoneycombVector2[] GetNeighbors_ByY()
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
    public HoneycombVector2[] GetNeighbors_ByX()
    {
        HoneycombVector2[] vector2s = new HoneycombVector2[6];
        vector2s[0] = new HoneycombVector2(X + 2, Y);
        vector2s[1] = new HoneycombVector2(X + 1, Y + 1);
        vector2s[2] = new HoneycombVector2(X - 1, Y + 1);
        vector2s[3] = new HoneycombVector2(X - 2, Y);
        vector2s[4] = new HoneycombVector2(X - 1, Y - 1);
        vector2s[5] = new HoneycombVector2(X + 1, Y - 1);

        return vector2s;
    }
    public static int GetDistanceEstimation(HoneycombVector2 left, HoneycombVector2 right)
    {
        //int x1 = left.GetCartesian_X();
        //int x2 = right.GetCartesian_X();
        //int y1 = left.GetCartesian_Y();
        //int y2 = right.GetCartesian_Y();

        //return Math.Abs(y1 / 2 - y2 / 2 + x1 - x2) + Math.Abs(y1 - y2) + Math.Abs(y1 - y2 + y2 / 2 - y1 / 2 + x2 - x1);

        //int num1 = left.GetCartesian_X() - right.GetCartesian_X();
        //int num2 = left.GetCartesian_Y() - right.GetCartesian_Y();
        //return num1 * num1 + num2 * num2;

        int num1 = left.GetCartesian_X() - right.GetCartesian_X();
        int num2 = left.GetCartesian_Y() - right.GetCartesian_Y();
        return Math.Abs(num1) + Math.Abs(num2);


        //int num1 = left.m_x - right.m_x;
        //int num2 = left.m_y - right.m_y;
        //return num1 * num1 + num2 * num2;
    }
    public override string ToString()
    {
        return $"({X},{Y})";
    }
}
[Serializable]
public enum TileMode
{
    /// <summary>Y轴为主 </summary>
    HexTile_Y,
    /// <summary>X轴为主 </summary>
    HexTile_X,
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
    public Vector2 StartZeroPoint = new Vector2(0, 0);
    public Coordinate MaxSize = new Coordinate(10, 10);
    public Coordinate MinSize = new Coordinate(-10, -10);

    public float GenerateInterval = 0.003f;

    [SerializeField]
    private Vector3 m_InitPosition = new Vector3(0, 0, 0);
    [SerializeField]
    private GameObject m_PrefabChessObject_X;
    [SerializeField]
    private GameObject m_PrefabChessObject_Y;
    [SerializeField]
    private Vector3 m_ChessSpacing_Y = new Vector3(5, 0, 3);
    [SerializeField]
    private Vector3 m_ChessSpacing_X = new Vector3(3, 0, 5);

    public GenerateMode Generate;
    public TileMode Tile;

    private Dictionary<HoneycombVector2, ChessItemComponent> m_CacheDict = new Dictionary<HoneycombVector2, ChessItemComponent>();

    public static ChessItemComponent StartChess;
    public static ChessItemComponent EndChess;

    public static Dictionary<HoneycombVector2, ChessItemComponent> m_ObstacleDict = new Dictionary<HoneycombVector2, ChessItemComponent>();
    // Start is called before the first frame update
    void Start()
    {
        switch (Generate)
        {
            case GenerateMode.Normal:
                NormalGenerate();
                break;
            case GenerateMode.Spread:
                SpreadGenerate();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    private void SpreadGenerate()
    {
        StartCoroutine(GenerateItem());


        List<ChessItemComponent> items = new List<ChessItemComponent>();

        IEnumerator GenerateItem()
        {
            HashSet<HoneycombVector2> starts = new HashSet<HoneycombVector2>();
            var startPoint = new HoneycombVector2((int)StartZeroPoint.x, (int)StartZeroPoint.y);
            starts.Add(startPoint);
            int length = 0;
            while (length != starts.Count)
            {
                HashSet<HoneycombVector2> list = new HashSet<HoneycombVector2>();
                length = starts.Count;
                foreach (HoneycombVector2 vector2 in starts)
                {
                    if (m_CacheDict.ContainsKey(vector2)) continue;
                    yield return new WaitForSeconds(GenerateInterval);
                    m_CacheDict.Add(vector2, CreateChessItem(vector2));
                    switch (Tile)
                    {
                        case TileMode.HexTile_Y:
                            list.UnionWith(vector2.GetNeighbors_ByY().Where(v2 => v2.X > MinSize.Row && v2.X < MaxSize.Row && v2.Y > MinSize.Col && v2.Y < MaxSize.Col && !starts.Contains(v2)));
                            break;
                        case TileMode.HexTile_X:
                            list.UnionWith(vector2.GetNeighbors_ByX().Where(v2 => v2.X > MinSize.Row && v2.X < MaxSize.Row && v2.Y > MinSize.Col && v2.Y < MaxSize.Col && !starts.Contains(v2)));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                starts.UnionWith(list);

            }
        }
    }
    private void NormalGenerate()
    {
        StartCoroutine(GenerateItem());


        List<ChessItemComponent> items = new List<ChessItemComponent>();

        IEnumerator GenerateItem()
        {
            HashSet<HoneycombVector2> starts = new HashSet<HoneycombVector2>();
            var startPoint = new HoneycombVector2((int)StartZeroPoint.x, (int)StartZeroPoint.y);
            starts.Add(startPoint);

            HashSet<HoneycombVector2> ends = new HashSet<HoneycombVector2>();

            while (starts.Count != 0)
            {
                yield return new WaitForSeconds(GenerateInterval);
                var chess = starts.First();
                m_CacheDict.Add(chess, CreateChessItem(chess));
                IEnumerable<HoneycombVector2> list = null;
                switch (Tile)
                {
                    case TileMode.HexTile_Y:
                        list = chess.GetNeighbors_ByY().Where(v2 => v2.X > MinSize.Row && v2.X < MaxSize.Row && v2.Y > MinSize.Col && v2.Y < MaxSize.Col && !ends.Contains(v2));
                        break;
                    case TileMode.HexTile_X:
                        list = chess.GetNeighbors_ByX().Where(v2 => v2.X > MinSize.Row && v2.X < MaxSize.Row && v2.Y > MinSize.Col && v2.Y < MaxSize.Col && !ends.Contains(v2));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                starts.UnionWith(list);
                starts.Remove(chess);
                ends.Add(chess);
            }
        }
    }

    private ChessItemComponent CreateChessItem(HoneycombVector2 point)
    {

        Vector2 spacing = Vector2.zero;
        switch (Tile)
        {
            case TileMode.HexTile_Y:
                spacing = new Vector2(m_ChessSpacing_Y.x, m_ChessSpacing_Y.z);
                break;
            case TileMode.HexTile_X:
                spacing = new Vector2(m_ChessSpacing_X.x, m_ChessSpacing_X.z);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        Vector3 pos = new Vector3(point.X * spacing.x + m_InitPosition.x, m_InitPosition.y, point.Y * spacing.y + m_InitPosition.z);
        ChessItemComponent com = null;
        switch (Tile)
        {
            case TileMode.HexTile_Y:
                com = GameObject.Instantiate(m_PrefabChessObject_Y, pos, Quaternion.identity, transform).AddComponent<ChessItemComponent>();
                break;
            case TileMode.HexTile_X:
                com = GameObject.Instantiate(m_PrefabChessObject_X, pos, Quaternion.identity, transform).AddComponent<ChessItemComponent>();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        com.Point = point;

        com.GetComponentInChildren<TextMesh>().text = point.ToString() + $"\n[{point.GetCartesian_X()},{point.GetCartesian_Y()}]";
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
                    if (StartChess != null)
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
        else if (Input.GetMouseButtonDown(2))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                var chess = hit.transform.GetComponent<ChessItemComponent>();
                if (chess != null)
                {

                    if (m_ObstacleDict.ContainsKey(chess.Point))
                    {
                        m_ObstacleDict.Remove(chess.Point);
                        chess.SetColor(ChessItemComponent.DefaultColor);
                    }
                    else
                    {
                        m_ObstacleDict[chess.Point] = chess;
                        chess.SetColor(ChessItemComponent.ObstacleColor);
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Clear();
            EndChess?.SetColor(ChessItemComponent.EndColor);
            StartChess?.SetColor(ChessItemComponent.StartColor);
            HoneycombNode[] paths;
            var target = StartPathing(StartChess.Point, EndChess.Point, out paths);

            if (target.Parent == null)//没找到路
            {
                return;
            }
            while (target != target.Parent)
            {
                target = target.Parent;
                if (target != null && target.Parent != target)//target!=null有路径
                {
                    m_CacheDict[target.Vector].SetColor(ChessItemComponent.PathColor);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            Clear();

            m_ObstacleDict.Clear();
            EndChess = null;
            StartChess = null;
        }
    }

    private void Clear()
    {
        foreach (var honeyKey in m_CacheDict.Keys)
        {
            if (m_ObstacleDict.ContainsKey(honeyKey)) continue;

            m_CacheDict[honeyKey].SetColor(ChessItemComponent.DefaultColor);
        }
    }

    public HoneycombNode StartPathing(HoneycombVector2 start, HoneycombVector2 end, out HoneycombNode[] paths)
    {
        //生成导航节点数据,内存换时间
        Dictionary<HoneycombVector2, HoneycombNode> cache = new Dictionary<HoneycombVector2, HoneycombNode>();//TODO 缓存&&走障碍
        HoneycombNode target = new HoneycombNode(this, end, 1);
        cache.Add(end, target);
        foreach (var vector in m_CacheDict.Keys)
        {
            if (vector == end) continue;
            if (m_ObstacleDict.ContainsKey(vector)) continue;

            var node = new HoneycombNode(this, vector, 1);
            node.Target = target;
            cache.Add(vector, node);
        }

        //初始化开始节点的g和f
        HoneycombNode startNode = cache[start];
        startNode.IsOpen = true;
        startNode.Parent = startNode;

        //初始化Open表
        BinaryHeap<HoneycombNode> openset = new BinaryHeap<HoneycombNode>(HeapType.MinHeap);
        openset.Push(startNode);

        //最后结果路径
        paths = null;
        List<HoneycombNode> closedSet = new List<HoneycombNode>();
        //开表有值
        while (openset.items.Count != 0)
        {

            var tempnode = openset.PopRoot();

            closedSet.Add(tempnode);

            tempnode.IsOpen = false;
            tempnode.IsCloseed = true;

            //闭表最后到头了
            if (closedSet[^1].Vector == end)
            {
                closedSet.ToArray();
                return target;
            }

            //没到头，查他邻居
            foreach (HoneycombVector2 vector2 in tempnode.GetNeighbors())
            {
                if (m_ObstacleDict.ContainsKey(vector2)) continue;

                var Node = cache[vector2];
                if (!Node.IsCloseed)
                {
                    //当前节点
                    int tempGScore = tempnode.G + tempnode.H;
                    if (!Node.IsOpen)//不在开表
                    {
                        Node.IsOpen = true;
                        Node.Parent = tempnode;
                        Node.G = tempGScore;
                        Node.F = Node.H + tempGScore;
                        openset.Push(Node);
                    }
                    else if (tempGScore < Node.G)
                    {
                        Node.Parent = tempnode;
                        Node.G = tempGScore;
                        Node.F = Node.H + tempGScore;
                    }
                }
            }
        }

        return target;
    }

    public HoneycombVector2[] ComputeNeighbors(HoneycombVector2 vector2)
    {
        HoneycombVector2[] array = null;
        switch (Tile)
        {
            case TileMode.HexTile_Y:
                array = vector2.GetNeighbors_ByY().Where(v2 => v2.X > MinSize.Row && v2.X < MaxSize.Row && v2.Y > MinSize.Col && v2.Y < MaxSize.Col)
                    .Where(vector => !m_ObstacleDict.ContainsKey(vector))
                    .ToArray();
                break;
            case TileMode.HexTile_X:
                array = vector2.GetNeighbors_ByX().Where(v2 => v2.X > MinSize.Row && v2.X < MaxSize.Row && v2.Y > MinSize.Col && v2.Y < MaxSize.Col)
                    .Where(vector => !m_ObstacleDict.ContainsKey(vector))
                    .ToArray();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        StringBuilder sb = new StringBuilder();
        foreach (HoneycombVector2 honeycombVector2 in array)
        {
            sb.Append(honeycombVector2.ToString());
        }

        return array;//筛选边界
    }

    [Serializable]
    public class HoneycombNode : IComparable<HoneycombNode>
    {
        public HoneycombVector2 Vector;
        public int G { get; set; } //从起始节点到当前节点的代价
        public int H => GetDistanceEstimation(); //从当前节点到目标节点的估计代价
        public int F
        {
            get;
            set;
        }//对经过当前节点的这条路径的代价的一个最好的估计值

        private ChessMapCreater m_HoneycombMap;
        public HoneycombNode Parent { get; set; }
        public HoneycombNode Target { get; set; }//目标

        public HoneycombVector2[] GetNeighbors()
        {
            return m_HoneycombMap.ComputeNeighbors(Vector);
        }
        public int GetDistanceEstimation()
        {
            if (Target == null) return 0;
            else return HoneycombVector2.GetDistanceEstimation(Vector, Target.Vector);
        }
        protected bool Equals(HoneycombNode other)
        {
            return Vector.Equals(other.Vector);
        }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((HoneycombNode)obj);
        }

        public override int GetHashCode()
        {
            return Vector.GetHashCode();
        }

        public static bool operator ==(HoneycombNode left, HoneycombNode right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(HoneycombNode left, HoneycombNode right)
        {
            return !Equals(left, right);
        }

        public HoneycombNode(ChessMapCreater map, HoneycombVector2 vector, int g)
        {
            m_HoneycombMap = map;
            Vector = vector;
            G = g;
        }

        public bool IsOpen { get; set; } = false;
        public bool IsCloseed { get; set; } = false;

        public int CompareTo(HoneycombNode other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;

            return F.CompareTo(other.F);
        }
    }

}

