using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestPhysic
{
    public class TestPhysicMono : MonoBehaviour
    {
        public bool AutoSimulation;

        private static Scene clientScene = default;
        // Start is called before the first frame update
        void Start()
        {
            Physics.autoSimulation = AutoSimulation;

            if (clientScene== default)
            {
                clientScene = SceneManager.CreateScene("Client", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
            }


            var cube = GameObject.Find("Cube");
            SceneManager.MoveGameObjectToScene(cube, clientScene);
            
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                SceneManager.MoveGameObjectToScene(gameObject, clientScene);
            }
        }

        void OnEnable()
        {
            RaycastHit hit;
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            if (Physics.Raycast(ray,out hit))
            {
                Debug.LogError(AutoSimulation);
            }
        }
    }

}
