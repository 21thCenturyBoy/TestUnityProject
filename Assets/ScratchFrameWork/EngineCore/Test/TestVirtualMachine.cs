using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScratchFramework
{
    public class TestVirtualMachine : IVirtualMachine
    {
        private static readonly string GameMapUnitySceneName = "ScratchGameMap";

        private Scene m_GameScene;
        private PhysicsScene m_GamePhysics;

        AsyncOperation m_SceneAo;

        private float m_TickDelta;

        public float TickDelta => m_TickDelta;

        public TestVirtualMachine Run()
        {
            m_TickDelta = Time.fixedDeltaTime;
            Physics.simulationMode = SimulationMode.Script;

            return this;
        }

        void LoadGameMap(Action<string> callback)
        {
            if (m_SceneAo != null)
            {
                callback?.Invoke("Scene Is Loading or UnLoading!");
                return;
            }

            m_SceneAo = SceneManager.LoadSceneAsync(GameMapUnitySceneName, LoadSceneMode.Additive);

            m_SceneAo.completed += (ao) =>
            {
                m_GameScene = SceneManager.GetSceneByName(GameMapUnitySceneName);
                m_GamePhysics = m_GameScene.GetPhysicsScene();

                m_SceneAo = null;

                callback?.Invoke(null);
            };
        }

        void UnLoadGameMap(Action<string> callback)
        {
            if (m_SceneAo != null)
            {
                callback?.Invoke("Scene Is Loading or UnLoading!");
                return;
            }

            m_SceneAo = SceneManager.UnloadSceneAsync(m_GameScene);

            m_SceneAo.completed += (ao) =>
            {
                m_GameScene = default;
                m_GamePhysics = default;

                m_SceneAo = null;

                callback?.Invoke(null);
            };
        }

        private IVirtualMachine.State m_State = IVirtualMachine.State.Stop;

        public void PreInit()
        {
            UpdateState(IVirtualMachine.State.PreInit);
        }


        private void UpdateState(IVirtualMachine.State state)
        {
            if (m_State == state) return;
            var orginState = m_State;
            m_State = state;
            switch (state)
            {
                case IVirtualMachine.State.PreInit:
                    //Load Game Map
                    LoadGameMap(info =>
                    {
                        if (!string.IsNullOrEmpty(info))
                        {
                            Debug.LogError(info);
                        }
                        else
                        {
                            UpdateState(IVirtualMachine.State.Init);
                        }
                    });
                    break;
                case IVirtualMachine.State.Init:
                    Init();
                    break;
                case IVirtualMachine.State.PrePlay:
                    if (orginState == IVirtualMachine.State.Init || orginState == IVirtualMachine.State.Pause)
                    {
                        UpdateState(IVirtualMachine.State.Play);
                    }
                    break;
                case IVirtualMachine.State.Play:
                    Awake();
                    break;
                case IVirtualMachine.State.PrePause:
                    if (orginState == IVirtualMachine.State.Play)
                    {
                        UpdateState(IVirtualMachine.State.Pause);
                    }

                    break;
                case IVirtualMachine.State.Pause:
                    break;
                case IVirtualMachine.State.PreStop:
                    if (orginState == IVirtualMachine.State.Play || orginState == IVirtualMachine.State.Pause)
                    {
                        Destroy();
                        //Clear
                        UnLoadGameMap(info =>
                        {
                            if (!string.IsNullOrEmpty(info))
                            {
                                Debug.LogError(info);
                            }
                            else
                            {
                                UpdateState(IVirtualMachine.State.Stop);
                            }
                        });
                    }

                    break;
                case IVirtualMachine.State.Stop:
                    Dispose();
                    UpdateState(IVirtualMachine.State.PreInit);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            OnUpdateState(orginState, state);
        }

        private void OnUpdateState(IVirtualMachine.State preState, IVirtualMachine.State newState)
        {
            if (preState == IVirtualMachine.State.Init && newState == IVirtualMachine.State.Play)
            {
            }
        }

        public IVirtualMachine.State GetCurrentState() => m_State;

        public void SetState(IVirtualMachine.Switch state)
        {
            if (((int)m_State & 1) == 0) return;

            switch (state)
            {
                case IVirtualMachine.Switch.Play:
                    UpdateState(IVirtualMachine.State.PrePlay);
                    break;
                case IVirtualMachine.Switch.Pause:
                    UpdateState(IVirtualMachine.State.PrePause);
                    break;
                case IVirtualMachine.Switch.Next:
                    break;
                case IVirtualMachine.Switch.Stop:
                    UpdateState(IVirtualMachine.State.PreStop);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Tick()
        {
            if (m_State != IVirtualMachine.State.Play) return;
            PhysicUpdate();
        }


        public void RenderUpdate(float deltaTime)
        {
            RealTime += deltaTime;
        }

        #region lifecycle

        private ulong m_PhysicFrame = 0;
        private float RealTime = 0f;

        private void Init()
        {
            RealTime = 0;
            m_PhysicFrame = 0;
            // Time.timeScale = 0;
        }

        private void Awake()
        {
        }


        private void PhysicUpdate()
        {
            m_PhysicFrame++;
            m_GamePhysics.Simulate(TickDelta);
        }

        private void Destroy()
        {
        }

        private void Dispose()
        {
        }

        #endregion
    }
}