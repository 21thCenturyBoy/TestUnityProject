namespace TestCoreLib
{

    using UnityEngine;
    using Random = UnityEngine.Random;

    public struct TranslateComponent : IComponent
    {
        public Vector3 Postition;
        public Entity ComEntity { get; set; }
        public Entity Dependency { get; set; }
    }



    public class TranslateSystem : CoreSubSystem<TranslateSystem>
    {
        Entity[] m_arrays = new Entity[1000];
        GameObject[] m_objects = new GameObject[1000];

        public override void AwakeSystem()
        {
            for (int i = 0; i < m_arrays.Length; i++)
            {
                m_arrays[i] = new Entity("Transform");
                var com = ComponentExtension.Create<TranslateComponent>();
                com.Postition = Vector3.zero;
                com.LinkEntity(m_arrays[i]);

                m_objects[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //给这个创建出来的对象起个名字
                m_objects[i].name = "Cube" + i;
            }
        }

        public override void StartSystem()
        {
        }

        public override void UpdtaeSystem(float delta)
        {
            for (int i = 0; i < m_arrays.Length; i++)
            {
                var com = m_arrays[i].GetComponents<TranslateComponent>()[0];
                com.Postition += new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                com.Update();
            }
        }

        public override void LateUpdtaeSystem(float delta)
        {
            for (int i = 0; i < m_objects.Length; i++)
            {
                m_objects[i].transform.position = m_arrays[i].GetComponents<TranslateComponent>()[0].Postition;
            }
        }

        public override void FixedUpdtaeSystem(float delta)
        {
        }

        public override void DestroySystem()
        {
        }

        public override void Dispose()
        {
        }

        public override void OnRegisterParent(ISystem system)
        {
        }
    }
}