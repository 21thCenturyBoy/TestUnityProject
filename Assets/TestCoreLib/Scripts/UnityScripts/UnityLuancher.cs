using System.Linq;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;


public class UnityLuancher : MonoBehaviour
{
    class UnityLog : ILog
    {
        public void Info(string str)
        {
            Debug.Log(str);
        }
        public void Warning(string str)
        {
            Debug.LogWarning(str);
        }
        public void Error(string str)
        {
            Debug.LogError(str);
        }
    }

    void Awake()
    {
        Log.Initialize(new UnityLog());
        GameObject.DontDestroyOnLoad(this.gameObject);
        CoreSystem.Instance.AwakeSystem();

        var obj = new Entity("测试");
        Log.Info(obj.Id);

        //第一种创建
        Component com = CommponentSystem.Create<Component>();//游离状态不建议，没有生命周期，混沌状态
        //可以在此初始化...
        Log.Info(nameof(com) + ":" + com.ComEntity.Guid.ToString());
        com.LinkEntity(obj);//游离的组件链接到了实体，开始拥有了生命周期

        //第二种创建
        Component com2 = obj.AddComponent<Component>();//依赖组件
        Log.Info(nameof(com2) + ":" + com2.ComEntity.Guid.ToString());

        MoveComponent move = CommponentSystem.Create<MoveComponent>();
        ; Log.Info(nameof(move) + ":" + move.ComEntity.Guid.ToString());
        move.Name = "Move";//只改了本地
        MoveComponent move2 = obj.AddComponent<MoveComponent>(); 
        Log.Info(nameof(move2) + ":" + move2.ComEntity.Guid.ToString());
        move2.Name = "Down";//只改了本地

        move.LinkEntity(obj);//接入组件系统再修改

        move.Name = "MoveChanageName";//只改了本地
        MoveComponent moveChanageName;
        obj.GetComponent<MoveComponent>(move.ComEntity.Guid, out moveChanageName);
        Log.Info($"没有修改CommponentSystem的名称:{moveChanageName.Name}");
        move.Update();
        obj.GetComponent<MoveComponent>(move.ComEntity.Guid, out moveChanageName);
        Log.Info($"修改了CommponentSystem的名称:{moveChanageName.Name}");

        Log.Info($"{obj.GetComponents<MoveComponent>().Count()}");
        Log.Info($"{obj.GetComponents<Component>().Count()}");

    }

    void Start()
    {
        CoreSystem.Instance.StartSystem();
    }
    void Update()
    {
        CoreSystem.Instance.UpdtaeSystem(Time.deltaTime);
    }
    void LateUpdate()
    {
        CoreSystem.Instance.UpdtaeSystem(Time.deltaTime);
    }
    void FixedUpdate()
    {
        CoreSystem.Instance.FixedUpdtaeSystem(Time.fixedDeltaTime);
    }
    void OnDestroy()
    {
        CoreSystem.Instance.DestroySystem();
    }

}
