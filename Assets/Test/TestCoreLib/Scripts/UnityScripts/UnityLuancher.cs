using System.Linq;
using UnityEngine;

namespace TestCoreLib
{
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
            Component com = ComponentExtension.Create<Component>();//游离状态不建议，没有生命周期，混沌状态
                                                                   //可以在此初始化...
            Log.Info(nameof(com) + ":" + com.ComEntity.Guid.ToString());
            com.LinkEntity(obj);//游离的组件链接到了实体，开始拥有了生命周期

            //第二种创建
            Component com2 = obj.AddComponent<Component>();//依赖组件
            Log.Info(nameof(com2) + ":" + com2.ComEntity.Guid.ToString());

            Component move = ComponentExtension.Create<Component>();
            ; Log.Info(nameof(move) + ":" + move.ComEntity.Guid.ToString());
            move.Name = "Move";//只改了本地
            Component move2 = obj.AddComponent<Component>();
            Log.Info(nameof(move2) + ":" + move2.ComEntity.Guid.ToString());
            move2.Name = "Down";//只改了本地

            move.LinkEntity(obj);//接入组件系统再修改

            move.Name = "MoveChanageName";//只改了本地
            Component moveChanageName;
            obj.GetComponent<Component>(move.ComEntity.Guid, out moveChanageName);
            Log.Info($"没有修改CommponentSystem的名称:{moveChanageName.Name}");
            move.Update();
            obj.GetComponent<Component>(move.ComEntity.Guid, out moveChanageName);
            Log.Info($"修改了CommponentSystem的名称:{moveChanageName.Name}");

            Log.Info($"{obj.GetComponents<Component>().Count()}");
            Log.Info($"{obj.GetComponents<Component>().Count()}");

            obj.RemoveComponent(moveChanageName);
            Log.Info($"{obj.GetComponents<Component>().Count()}");
            int count = obj.RemoveComponents<Component>();
            Log.Info($"{obj.GetComponents<Component>().Count()}:Remove Count{count}");


            for (int i = 0; i < 1000; i++)
            {
                Component test = obj.AddComponent<Component>();//依赖组件
            }
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
            CoreSystem.Instance.LateUpdtaeSystem(Time.deltaTime);
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

}