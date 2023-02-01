using System;


namespace TestMutiPlay
{
    /// <summary>
    /// Unity编辑器环境
    /// </summary>
    public partial class MultiUserEditorDebuger : Singleton_Mono<MultiUserEditorDebuger>
    {
        public  const string InitUnitySceneName = "TestLogin";
        public static bool CheckTempProjectExits(int index)
        {
            bool res = false;
#if UNITY_EDITOR

            res = MultiUserEditorStartup.Config.TempUnityProjectNames.Count > index &&
                  MultiUserEditorStartup.Config.TempUnityProjectPaths.Count > index;
#endif
            return res;
        }

        public static string GetTempProjectPath(int index)
        {
            string path = String.Empty;
#if UNITY_EDITOR
            if (CheckTempProjectExits(index))
            {
                path = MultiUserEditorStartup.Config.TempUnityProjectPaths[index];
            }
#endif
            return path;
        }

        public static string GetTempProjectName(int index)
        {
            string name = String.Empty;
#if UNITY_EDITOR
            if (CheckTempProjectExits(index))
            {
                name = MultiUserEditorStartup.Config.TempUnityProjectNames[index];
            }
#endif
            return name;
        }



    }
}


