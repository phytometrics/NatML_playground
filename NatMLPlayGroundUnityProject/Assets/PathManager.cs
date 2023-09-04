using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

public class PathManager : MonoBehaviour
{
    public static PathManager Instance { get; private set; }
    public string MLModelParentFolder = "MobileNetV3";
    public string MLModelFileNameCoreML = "mobilenetv3_large_pytorch.mlmodel";
    public string MLModelFileNameOnnx = "mobilenetv3_large_pytorch.onnx";
    public string MLModelFileNameTflite = "mobilenetv3_large_pytorch_float16.tflite";
    [HideInInspector]
    public string modelPath;
    [HideInInspector]
    public bool isModelFileReady = false;
    void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer || 
            Application.platform == RuntimePlatform.OSXEditor || 
            Application.platform == RuntimePlatform.OSXPlayer)
            {     
                // modelPath = Application.dataPath + "/" + MLModelParentFolder + "/" + MLModelFileNameCoreML;
                modelPath = Path.Combine(Application.dataPath, "StreamingAssets", MLModelParentFolder, MLModelFileNameCoreML);
                isModelFileReady = true;
            }
            else if (Application.platform == RuntimePlatform.WebGLPlayer || 
                    Application.platform == RuntimePlatform.WindowsEditor || 
                    Application.platform == RuntimePlatform.WindowsPlayer)
            {
                // modelPath = Application.dataPath + "/" + MLModelParentFolder + "/" + MLModelFileNameOnnx;
                // rewrite using Path.Combine
                modelPath = Path.Combine(Application.dataPath, "StreamingAssets", MLModelParentFolder, MLModelFileNameOnnx);
                isModelFileReady = true;
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
            // Get the file path in the application's persistent data path
            modelPath = Path.Combine(Application.persistentDataPath, MLModelFileNameTflite);

            // Check if the file already exists
            if (!File.Exists(modelPath))
            {
                // Get the file path in the StreamingAssets folder
                string srcPath = Path.Combine(Application.streamingAssetsPath, MLModelParentFolder, MLModelFileNameTflite);

                // Start a coroutine to copy the file from the StreamingAssets folder to the persistentDataPath
                StartCoroutine(CopyFile(srcPath, modelPath));
            }
            else
            {
                isModelFileReady = true;
            }
        }

        }
      
    IEnumerator CopyFile(string srcPath, string dstPath)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(srcPath))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("Error while copying file: " + www.error);
            }
            else
            {
                File.WriteAllBytes(dstPath, www.downloadHandler.data);
                isModelFileReady = true;
            }
        }
    }
    
}