using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestDictGC
{
    public enum TestEnum_GC
    {
        Test1,
        Test2,
        Test3,
        Test4,
        Test5,
        Test6,
        Test7,
        Test8,
        Test9,
        Test10,
        Test20,
        Test30,
        Test40,
        Test50,
        Test60,
        Test70,
        Test80,
        Test90,
        Test100,
        Test200,
        Test300,
        Test400,
        Test500,
        Test600,
        Test700,
        Test800,
        Test900,
        Test1000,
        Test2000,
        Test3000,
        Test4000,
        Test5000,
        Test6000,
        Test7000,
        Test8000,
        Test9000,
        Test10000,
        Test20000,
        Test30000,
        Test40000,
        Test50000,
        Test60000,
        Test70000,
        Test80000,
        Test90000,
        Test100000,
        Test200000,
        Test300000,
        Test400000,
        Test500000,
        Test600000,
        Test700000,
        Test800000,
        Test900000,
        Test1000000,
        Test2000000,
        Test3000000,
        Test4000000,
        Test5000000,
        Test6000000,
        Test7000000,
        Test8000000,
        Test9000000,
    }

    public class TestDictGC : MonoBehaviour
    {
        private Dictionary<TestEnum_GC, int> m_TestDictionary = new Dictionary<TestEnum_GC, int>();
        // Start is called before the first frame update
        void Start()
        {
            foreach (var val in Enum.GetValues(typeof(TestEnum_GC)))
            {
                m_TestDictionary.Add((TestEnum_GC)val, (int)val);
            }
            Application.targetFrameRate = 60;
        }

        // Update is called once per frame
        void FixedUpdate()
        {

            ////测试字典做Key
            //for (int i = 0; i < 10000000; i++)
            //{
            //    if (m_TestDictionary.ContainsKey(TestEnum_GC.Test10000))
            //    {

            //    }
            //}
            //测试枚举比较
            TestEnum_GC e = TestEnum_GC.Test1;
            for (int i = 0; i < 10000000; i++)
            {
                if (TestEnum_GC.Test10000 == e)
                {
                }
            }
        }
    }

}
