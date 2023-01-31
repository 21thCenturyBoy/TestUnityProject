using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace TestMutiPlay
{

    public static class MultiUserEditorCommunication
    {
        public enum Code
        {
            Error = -1,
            Normal,
            HeartLoopRequest,
            HeartLoopResponse,
        }

        public class Pipeline : IDisposable
        {

            public Guid ID { get; }

            private NamedPipeServerStream Server;
            private Task m_Task;
            private AutoResetEvent Get, Got;
            private string inputContext;
            private StreamWriter Writer;
            private StreamReader Reader;
            private bool m_Switch = false;

            public const int MaxServer = 100;
            public const string ServerName = "testpipe";
            public const int ServerWaitReadMillisecs = 10000; //10s
            public const int MaxTimeout = 3;

            private bool _isDisposed = false;


            protected virtual void Dispose(bool disposing)
            {
                if (!_isDisposed)
                {
                    m_Switch = false;
                    if (disposing)
                    {
                    }

                    try
                    {
                        Debug.Log("...");
                        var th = new Thread(() =>
                        {
                            try
                            {
                                Debug.Log("111");
                                if (m_Wait != null) Server.EndWaitForConnection(m_Wait);
                            }
                            catch (Exception e)
                            {
                                Debug.Log("333" + e.ToString());
                            }

                            Debug.Log("444");

                        });
                        th.IsBackground = true;
                        th.Start();

                        Debug.Log("222");
                        Thread.Sleep(300);
                        m_thread?.Interrupt();
                        m_thread?.Abort();
                        m_thread = null;

                        if (Server.IsConnected) Server.Disconnect();

                        Writer?.Dispose();
                        Reader?.Close();
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.ToString());
                    }

                    //Server?.Dispose();
                    m_Task?.Dispose();
                    Get?.Dispose();
                    Got?.Dispose();
                    //try
                    //{
                    //    Writer?.Close();
                    //    Reader?.Close();
                    //}
                    //catch (Exception e)
                    //{
                    //}

                    Debug.Log("释放了...");
                    Debug.Log("释放了" + Thread.CurrentThread.ManagedThreadId);
                }

                _isDisposed = true;
            }

            ~Pipeline()
            {
                Dispose(false);
            }

            public Pipeline()
            {
                ID = Guid.NewGuid();
                Get = new AutoResetEvent(false);
                Got = new AutoResetEvent(false);

                Server = new NamedPipeServerStream(ServerName, PipeDirection.InOut, MaxServer);

            }

            private Thread m_thread;

            private IAsyncResult m_Wait = null;

            public void Start()
            {


                m_Switch = true;
                Debug.Log("Start" + Thread.CurrentThread.ManagedThreadId);

                Debug.Log($"Waiting for client connection...");
                try
                {
                    m_thread = new Thread(() =>
                    {
                        Thread.Sleep(0);
                        m_Wait = Server.BeginWaitForConnection(ar =>
                        {
                            if (Server.IsConnected)
                            {
                                Debug.Log($"Client connected.");
                                Writer = new StreamWriter(Server);
                                Reader = new StreamReader(Server);
                                PipelinePool.Instance.CreatePipeLineAsync();
                                Debug.Log($"CreatePipeLineAsync");
                                while (m_Switch)
                                {
                                    var input = TryReadLine();
                                    if (string.IsNullOrEmpty(input)) break;
                                    Debug.Log($"Server Get Message:{1000}");
                                }
                            }
                        }, Server);
                    });
                    m_thread.IsBackground = true;
                    m_thread.Start();
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }

                //m_thread = new Thread(() =>
                //{
                //    Debug.Log($"Waiting for client connection...");
                //    Debug.Log("Waiting" + Thread.CurrentThread.ManagedThreadId);

                //    try
                //    {

                //    }
                //    catch (Exception e)
                //    {
                //        Debug.Log("异常" + e.ToString());
                //    }

                //})
                //{
                //    IsBackground = true
                //};
                //m_thread.Start();


                //m_Task = Task.Run(TaskRun);
            }

            public void Write(string msg)
            {
                try
                {
                    Writer.Write(msg);
                    Writer.Flush();
                }
                catch (Exception e)
                {
                }
            }

            private async void TaskRun()
            {
                try
                {
                    Debug.Log($"Waiting for client connection...");
                    await Server.WaitForConnectionAsync();
                    Debug.Log($"Client connected.");


                    PipelinePool.Instance.CreatePipeLineAsync();
                    Writer = new StreamWriter(Server);
                    Reader = new StreamReader(Server);
                    Debug.Log($"CreatePipeLineAsync");
                    while (m_Switch)
                    {
                        var input = await Reader.ReadLineAsync();

                        ////var input = TryReadLine();
                        if (string.IsNullOrEmpty(input)) break;

                        Debug.Log($"Server Get Message:{1000}");
                    }
                }
                catch (Exception e)
                {
                    //Debug.Log($"管道{ID}超时次数过多，视为丢失链接");
                }
                //Debug.Log($"管道{ID}即将关闭");
            }


            private void readerThread()
            {
                Get.WaitOne();
                inputContext = Reader.ReadLine();
                Got.Set();
            }

            private string TryReadLine()
            {
                int TimeOutCount = 0;
                var thread = new Thread(readerThread);

                thread.Start();
                Get.Set();
                while (!Got.WaitOne(ServerWaitReadMillisecs))
                {
                    if (TimeOutCount++ > MaxTimeout)
                    {
                        thread.Abort();
                        throw new TimeoutException();
                    }

                    Debug.LogError($"管道{ID}第{TimeOutCount}次超时");

                }

                return inputContext;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);

            }
        }

        public class PipelinePool : IDisposable
        {
            private static PipelinePool m_Instance;
            public static PipelinePool GetInstance => m_Instance;

            public static PipelinePool Instance
            {
                get
                {
                    if (m_Instance == null) m_Instance = new PipelinePool();

                    return m_Instance;
                }
            }

            public PipelinePool()
            {
            }

            private bool _isDisposed = false;


            protected virtual void Dispose(bool disposing)
            {
                if (!_isDisposed)
                {
                    if (disposing)
                    {
                    }

                    lock (m_ServerPool)
                    {
                        var ids = m_ServerPool.Keys.ToArray();
                        if (ids.Length == 0) return;

                        for (int i = 0; i < ids.Length; i++)
                        {
                            m_ServerPool.TryRemove(ids[i], out Pipeline pipe);
                            pipe?.Dispose();
                        }

                        m_ServerPool.Clear();
                    }
                }

                m_Instance = null;
                _isDisposed = true;
            }

            ~PipelinePool()
            {
                Dispose(false);
            }

            /// <summary>
            /// 用于存储和管理管道的进程池
            /// </summary>
            private readonly ConcurrentDictionary<Guid, Pipeline> m_ServerPool =
                new ConcurrentDictionary<Guid, Pipeline>();

            /// <summary>
            /// 创建一个新的管道
            /// </summary>
            private void CreatePipeLine()
            {
                lock (m_ServerPool)
                {
                    if (m_ServerPool.Count < Pipeline.MaxServer)
                    {
                        var pipe = new Pipeline();
                        pipe.Start();

                        m_ServerPool.TryAdd(pipe.ID, pipe);

                    }

                    Debug.Log($"管道池添加新管道 当前管道总数{m_ServerPool.Count}");
                }
            }

            /// <summary>
            /// 根据ID从管道池中释放一个管道
            /// </summary>
            private void DisposablePipeLine(Guid Id)
            {
                lock (m_ServerPool)
                {
                    Debug.Log($"开始尝试释放,管道{Id}");
                    if (m_ServerPool.TryRemove(Id, out Pipeline pipe)) Debug.Log($"管道{Id},已经关闭,并完成资源释放");
                    else Debug.Log($"未找到ID为{Id}的管道");
                    pipe?.Dispose();

                    if (m_ServerPool.Count == 0) CreatePipeLine();

                }
            }

            /// <summary>
            /// (异步)创建一个新的管道进程
            /// </summary>
            public async void CreatePipeLineAsync() => await Task.Run(CreatePipeLine);

            /// <summary>
            /// (异步)根据ID从管道池中释放一个管道
            /// </summary>
            /// <param name="id"></param>
            public async void DisposablePipeLineAsync(Guid id) => await Task.Run(() => { DisposablePipeLine(id); });

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        public class PipeClient : IDisposable
        {
            public string Name { get; }

            private NamedPipeClientStream m_client;

            private StreamWriter sw;
            private StreamReader sr;

            private Task m_listen;

            public const int ConnectMaxTime = 3000;

            private bool m_Listenswitch = false;

            public PipeClient(string name)
            {
                Name = name;

                Debug.Log($"客户端管道建立:{name}!");
                m_client = new NamedPipeClientStream(".", Pipeline.ServerName, PipeDirection.InOut, PipeOptions.None,
                    TokenImpersonationLevel.None);

                sw = new StreamWriter(m_client);
                sr = new StreamReader(m_client);

                m_Listenswitch = true;
                m_listen = Task.Factory.StartNew(Listen);
            }

            public void Write(string msg)
            {
                try
                {
                    sw.Write(msg);
                    sw.Flush();
                }
                catch (Exception e)
                {
                }
            }

            public void Listen()
            {
                try
                {
                    Debug.Log($"客户端 {Name}Listen !");
                    m_client.Connect();

                    while (m_Listenswitch)
                    {
                        string temp = sr.ReadLine();
                        if (!m_client.IsConnected) break;

                        Debug.Log($"客户端 {Name}Message:{temp}");
                    }
                }
                catch (Exception ex)
                {
                }
            }


            public void Dispose()
            {
                try
                {
                    m_client?.Dispose();
                    sw?.Dispose();
                    sr?.Dispose();

                    m_Listenswitch = false;

                    m_listen?.Dispose();

                    Debug.Log($"客户端 {Name}Dispose !");
                }
                catch (Exception e)
                {

                }
            }
        }

        public static void MessagePack(string str)
        {
            //string date = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]:{msg}";
        }

        public static PipeClient PipeClientStartUp(string clientName)
        {
            PipeClient client = new PipeClient(clientName);
            return client;
        }

        public static void PipeServerStartUp()
        {
            Debug.Log("服务器管道池启用...");
            if (PipelinePool.GetInstance != null)
            {
                PipelinePool.GetInstance.Dispose();
            }

            PipelinePool.Instance.CreatePipeLineAsync();
        }

        public static void PipeServerClose()
        {
            PipelinePool.Instance.Dispose();
        }
    }
}