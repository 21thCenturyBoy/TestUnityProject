using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScratchFramework
{
    public class TestIL2CPP : MonoBehaviour
    {
        void Start()
        {
            TMPro.TMP_Text text = GetComponent<TMPro.TMP_Text>();
            string output = "";
            for (int i = 0; i <= 576; i++)
            {
                output += $"\tpublic class EventBase_B_{i}:EventBase_B<EventBase_B_{i}>" + "{}\n";
            }

            text.text = output;
        }
    }

    public class EventBase_B<T> : EventArgs where T : class
    {
        protected EventBase_B()
        {
        }

        public static T Get()
        {
            return EventPool.GetEvent(typeof(T)) as T;
        }

        public virtual void Recyle()
        {
            EventPool.RecycleEvent(this);
        }
    }

    public static class EventPool
    {
        private static Dictionary<Type, Queue<EventArgs>> mEventPool = new Dictionary<Type, Queue<EventArgs>>();
        private static int MAX_CACHE_EVENT_NUMBER = 5;

        public static EventArgs GetEvent(Type t)
        {
            if (!mEventPool.ContainsKey(t))
            {
                RegisterEvent(t);
            }

            if (mEventPool[t].Count == 0)
            {
                for (int i = 0; i < MAX_CACHE_EVENT_NUMBER; i++)
                {
                    EventArgs evt = System.Activator.CreateInstance(t) as EventArgs;
                    mEventPool[t].Enqueue(evt);
                }
            }

            return mEventPool[t].Dequeue();
        }

        public static void RecycleEvent(EventArgs e)
        {
            mEventPool[e.GetType()].Enqueue(e);
        }

        /// <summary>
        /// 添加事件类型到事件池
        /// 生成新事件类型的缓存队列，并填满该事件类型的事件队列
        /// TODO 可以动态填充队列，无需初始化就填满
        /// </summary>
        /// <param name="t"></param>
        private static void RegisterEvent(Type t)
        {
            if (!mEventPool.ContainsKey(t))
            {
                mEventPool[t] = new Queue<EventArgs>();
            }

            for (int i = 0; i < MAX_CACHE_EVENT_NUMBER; i++)
            {
                EventArgs evt = System.Activator.CreateInstance(t) as EventArgs;
                mEventPool[t].Enqueue(evt);
            }
        }
    }

    // public interface IEventBaseRecyle
    // {
    //     void Recyle();
    // }
    //
    // public abstract class EventBase_A
    // {
    //     protected EventBase_A()
    //     {
    //     }
    //
    //     public static T Get<T>() where T : EventBase_A
    //     {
    //         return EventPool.GetEvent(typeof(T)) as T;
    //     }
    //
    //     public void Recyle()
    //     {
    //         EventPool.RecycleEvent(this);
    //     }
    // }
    //
    // public static class EventPool
    // {
    //     private static Dictionary<Type, Queue<EventBase_A>> mEventPool = new Dictionary<Type, Queue<EventBase_A>>();
    //     private static int MAX_CACHE_EVENT_NUMBER = 5;
    //
    //     public static EventBase_A GetEvent(Type t)
    //     {
    //         if (!mEventPool.ContainsKey(t))
    //         {
    //             RegisterEvent(t);
    //         }
    //
    //         if (mEventPool[t].Count == 0)
    //         {
    //             for (int i = 0; i < MAX_CACHE_EVENT_NUMBER; i++)
    //             {
    //                 EventBase_A evt = System.Activator.CreateInstance(t) as EventBase_A;
    //                 mEventPool[t].Enqueue(evt);
    //             }
    //         }
    //
    //         return mEventPool[t].Dequeue();
    //     }
    //
    //     public static void RecycleEvent(EventBase_A e)
    //     {
    //         mEventPool[e.GetType()].Enqueue(e);
    //         if (e is IEventBaseRecyle recyle)
    //         {
    //             recyle.Recyle();
    //         }
    //     }
    //
    //     /// <summary>
    //     /// 添加事件类型到事件池
    //     /// 生成新事件类型的缓存队列，并填满该事件类型的事件队列
    //     /// TODO 可以动态填充队列，无需初始化就填满
    //     /// </summary>
    //     /// <param name="t"></param>
    //     private static void RegisterEvent(Type t)
    //     {
    //         if (!mEventPool.ContainsKey(t))
    //         {
    //             mEventPool[t] = new Queue<EventBase_A>();
    //         }
    //
    //         for (int i = 0; i < MAX_CACHE_EVENT_NUMBER; i++)
    //         {
    //             EventBase_A evt = System.Activator.CreateInstance(t) as EventBase_A;
    //             mEventPool[t].Enqueue(evt);
    //         }
    //     }
    // }
}