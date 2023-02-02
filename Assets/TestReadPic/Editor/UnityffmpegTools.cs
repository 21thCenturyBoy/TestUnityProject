using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TestReadPic
{
    public class FfmpegToolsWindow : EditorWindow
    {
        private static string DefaultDirectoryPath = "";
        private static string FFmpegPath = "";

        private string m_DirectoryPath = "";
        private string m_params = "";

        [MenuItem("Tools/ffmpeg/OpenWindows")]
        static void CreateWindow()
        {
            FfmpegToolsWindow ffmpegWindow = EditorWindow.GetWindow(typeof(FfmpegToolsWindow), false, "FfmpegTools", false) as FfmpegToolsWindow;

            ffmpegWindow.Show();
        }

        public FfmpegToolsWindow()
        {

        }

        void OnEnable()
        {
            Debug.Log("FfmpegToolsWindow->OnEnable");

            m_DirectoryPath = Path.Combine(Application.dataPath, "../FfmpegWorkSpace/");
            FFmpegPath = Application.streamingAssetsPath + "\\TestReadPic\\ffmpeg.exe";

        }

        private bool CheckDirectoryPathExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return true;
            }
            return false;
        }

        private void OnGUI()
        {
            GUILayout.Label("WorkSpaceDirectory工作目录：");
            m_DirectoryPath = GUILayout.TextField(m_DirectoryPath);

            GUILayout.Label("参数命令：");
            m_params = GUILayout.TextField(m_params);
            if (GUILayout.Button("执行"))
            {
                SetProcessParameters(m_params);
            }
        }
        public void SetProcessParameters(string Parameters)
        {
            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = FFmpegPath;
            p.StartInfo.Arguments = Parameters;
            //是否使用操作系统shell启动
            p.StartInfo.UseShellExecute = false;
            //不显示程序窗口
            p.StartInfo.CreateNoWindow = false;
            p.Start();

            float slider = 0;
            if (EditorUtility.DisplayCancelableProgressBar("FFmpeg Progress Bar", "处理中", slider))
            {
                slider = 1;
                EditorUtility.ClearProgressBar();
                p.Close();
            }

            p.WaitForExit();
            p.Close();

            EditorUtility.ClearProgressBar();
        }
    }

}
public class UnityffmpegTools : MonoBehaviour
{
    public static string DirectoryPath = Application.streamingAssetsPath + "\\TestReadPic\\Images\\";

    /// <summary>
    /// 以IO方式进行加载
    /// </summary>
    public void LoadByIO(string fileName, RawImage rawImage)
    {
        string path = DirectoryPath + fileName;
        if (File.Exists(path))
        {
            //创建文件读取流 "D:\\test.jpg"
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            fileStream.Seek(0, SeekOrigin.Begin);
            //创建文件长度缓冲区
            byte[] bytes = new byte[fileStream.Length];
            //读取文件
            fileStream.Read(bytes, 0, (int)fileStream.Length);
            //释放文件读取流
            fileStream.Close();
            fileStream.Dispose();
            fileStream = null;
            //创建Texture
            Texture2D texture = new Texture2D(Screen.width, Screen.height);
            texture.LoadImage(bytes);
            rawImage.texture = texture;
            //创建Sprite
            // Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            //  image.sprite = sprite;
        }

    }
    /// <summary>
    /// 根据传进来的文件名，返回后缀名
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public void LoadImageByName(string name, RawImage rawImage)
    {

        string path = DirectoryPath + name;
        if (File.Exists(path + ".jpg"))
        {
            string FullPath = path + ".jpg";
            loadImage(FullPath, rawImage);
        }
        else if (File.Exists(path + ".png"))
        {
            string FullPath = path + ".png";
            loadImage(FullPath, rawImage);
        }
        else if (File.Exists(path + ".jpeg"))
        {
            string FullPath = path + ".jpeg";
            loadImage(FullPath, rawImage);

        }
    }



    void loadImage(string FullPath, RawImage rawImage)
    {
        FileStream fileStream = new FileStream(FullPath, FileMode.Open, FileAccess.Read);
        fileStream.Seek(0, SeekOrigin.Begin);
        //创建文件长度缓冲区
        byte[] bytes = new byte[fileStream.Length];
        //读取文件
        fileStream.Read(bytes, 0, (int)fileStream.Length);
        //释放文件读取流
        fileStream.Close();
        fileStream.Dispose();
        fileStream = null;
        //创建Texture
        Texture2D texture = new Texture2D(Screen.width, Screen.height);
        texture.LoadImage(bytes);
        rawImage.texture = texture;
    }
    /// <summary>
    /// 以IO方式进行加载
    /// </summary>
    public void LoadByIO(string fileName, RawImage rawImage, int rawImageWidth, int rawImageHeight)
    {
        string path = DirectoryPath + fileName;

        //创建文件读取流 "D:\\test.jpg"
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        fileStream.Seek(0, SeekOrigin.Begin);
        //创建文件长度缓冲区
        byte[] bytes = new byte[fileStream.Length];
        //读取文件
        fileStream.Read(bytes, 0, (int)fileStream.Length);
        //释放文件读取流
        fileStream.Close();
        fileStream.Dispose();
        fileStream = null;
        //创建Texture
        Texture2D texture = new Texture2D(rawImageWidth, rawImageHeight);
        texture.LoadImage(bytes);
        rawImage.texture = texture;
        //创建Sprite
        // Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        //  image.sprite = sprite;
    }
    /// <summary>
    /// 扫描文件并加载
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="rawImage"></param>
    public void scan(string fileName, RawImage rawImage)
    {
        string jpgPath = DirectoryPath + fileName + ".jpg";
        // 改名字文件存在mp4文件　删除以前的截图　重新生成

        if (File.Exists(DirectoryPath + fileName + ".mp4"))
        {
            // ffmpeg 截图
            FfmpegFun(jpgPath, DirectoryPath + fileName + ".mp4");
        }
        else if (File.Exists(DirectoryPath + fileName + ".flv"))
        {
            FfmpegFun(jpgPath, DirectoryPath + fileName + ".flv");
        }
        else if (File.Exists(DirectoryPath + fileName + ".mov"))
        {
            FfmpegFun(jpgPath, DirectoryPath + fileName + ".mov");
        }
        else if (File.Exists(DirectoryPath + fileName + ".avi"))
        {
            FfmpegFun(jpgPath, DirectoryPath + fileName + ".avi");
        }
        LoadImageByName(fileName, rawImage);

        //如果该文件存在jpg格式
        //if (File.Exists(jpgPath))
        //{
        //    // 根据后缀名判断是 视频 还是 图片
        //    string extensionName = Path.GetExtension(jpgPath);
        //    if (extensionName == ".jpg" || extensionName == ".png")
        //    {
        //        LoadByIO(fileName+".jpg",rawImage);

        //    }
        //}

    }//end scan

    /// <summary>
    /// 判断一个文件夹下包含特殊字符串文件名的数量
    /// </summary>

    public int ScanNum(string str)
    {
        // string str = "B1-";
        int num = 0;
        DirectoryInfo folder = new DirectoryInfo(DirectoryPath);

        foreach (FileInfo file in folder.GetFiles(str + "*" + ".jpg"))
        {
            num++;
        }
        foreach (FileInfo file in folder.GetFiles(str + "*" + ".jpeg"))
        {
            num++;
        }
        foreach (FileInfo file in folder.GetFiles(str + "*" + ".png"))
        {
            num++;
        }
        return num;

    }
    /// <summary>
    ///  是否存在文件
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool ExitFile(string name)
    {
        if (File.Exists(DirectoryPath + name))
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    /// <summary>
    /// fffmpegFun方式实现播放视频
    /// </summary>
    private void FfmpegFun(string jpgPath, string MP4Path)
    {
        // jpg图片名字
        string jpgTempName = jpgPath;
        // 如果图片已存在 删除
        DeleteFile(jpgTempName);

        // 截取视频的预览图
        //string para = "-i " + MP4Path + " -y -f image2 -t 0.001 -s 300x180 " + jpgTempName;
        string para = $"-i {MP4Path} -r 5 -f image2 {jpgTempName}1_frame_%05d.bmp";

        GetPicFromVideo(para);
    }

    /// <summary>
    /// 根据路径删除文件
    /// </summary>
    /// <param name="path"></param>
    public void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            FileAttributes attr = File.GetAttributes(path);
            if (attr == FileAttributes.Directory)
            {
                Directory.Delete(path, true);
            }
            else
            {
                File.Delete(path);
            }
        }
    }

    public static string FFmpegPath = Application.streamingAssetsPath + "\\TestReadPic\\ffmpeg.exe";

    // 从视频画面中截取一帧画面为图片
    /// <summary>
    /// 从视频画面中截取一帧画面为图片
    /// </summary>
    /// <param name="VideoName">视频文件，绝对路径</param>
    /// <param name="WidthAndHeight">图片的尺寸如:240*180</param>
    /// <param name="CutTimeFrame">开始截取的时间如:"1"</param>
    /// <returns></returns>
    public void GetPicFromVideo(string Parameters)
    {
        var p = new System.Diagnostics.Process();
        p.StartInfo.FileName = FFmpegPath;
        p.StartInfo.Arguments = Parameters;
        //是否使用操作系统shell启动
        p.StartInfo.UseShellExecute = false;
        //不显示程序窗口
        p.StartInfo.CreateNoWindow = true;
        p.Start();
        // Debug.Log("\n开始转码...\n");
        p.WaitForExit();
        p.Close();
        // Debug.Log("\n转码完毕...\n");

    }
}