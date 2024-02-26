using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
///     "com.unity.entities": "0.51.1-preview.21", -->     "com.unity.entities": "1.0.16",
/// </summary>
//public class Test : MonoBehaviour
//{
//    public GameObject cube;
//    public float interval, sum;

//    // Start is called before the first frame update
//    void Start()
//    {
//        GameObjectConversionSettings tempSettings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
//        Unity.Entities.Entity tempEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(cube, tempSettings);

//        EntityManager tempEntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

//        Translation tempTranslation = new Translation();
//        tempTranslation.Value.x = -interval;
//        for (int i = 0; i < sum; i++)
//        {
//            for (int j = 0; j < sum; j++)
//            {
//                Unity.Entities.Entity tempCube = tempEntityManager.Instantiate(tempEntityPrefab);
//                tempTranslation.Value.x += interval;
//                tempEntityManager.SetComponentData(tempCube, tempTranslation);
//            }

//            tempTranslation.Value.x = -interval;
//            tempTranslation.Value.y += interval;
//        }
//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }
//}
//public class TestTranslationSystem : ComponentSystem
//{
//    protected override void OnUpdate()
//    {
//        Entities.ForEach((ref Translation pTranslationComponentData1) =>
//        {
//            pTranslationComponentData1.Value = new float3(1, 1, 1);
//        });
//    }
//}
