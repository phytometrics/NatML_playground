using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using NatML;
using NatML.Features;
using NatML.Internal;
using NatML.Types;


namespace NatML.Vision
{
    public class MLClassificationModelPredictor : IMLPredictor<string>
    {
        public static string[] labels;
        // private float[] imageFeatureMean = new float[] {0.485f, 0.456f, 0.406f, 0.0f};
        // also express as Unity Engine Vector4
        private Vector4 imageFeatureMean = new Vector4(0.485f, 0.456f, 0.406f, 0.0f);

        // private float[] imageFeatureStd = new float[] {0.229f, 0.224f, 0.225f, 0.0f};
        // also express as Unity Engine Vector4
        private Vector4 imageFeatureStd = new Vector4(0.229f, 0.224f, 0.225f, 0.0f);

        public static async Task<MLClassificationModelPredictor> Create()
        {
            string labelFileName = "imageNetLabels";
            string MLModelParentFolder = "MobileNetV3";
            string MLModelFileNameCoreML = "mobilenetv3_large_pytorch.mlmodel";
            string MLModelFileNameOnnx = "mobilenetv3_large_pytorch.onnx";
            string MLModelFileNameTflite = "mobilenetv3_large_pytorch_float16.tflite";
            string path;

            


            // load imagenetlabels
            // Load the text file from the Resources folder
            TextAsset labelData = Resources.Load<TextAsset>(labelFileName);
            // Split the text file into an array of labels
            labels = labelData.text.Split('\n');

            if (Application.platform == RuntimePlatform.IPhonePlayer || 
                Application.platform == RuntimePlatform.OSXEditor || 
                Application.platform == RuntimePlatform.OSXPlayer)
            {     
                path = Path.Combine(Application.dataPath, "StreamingAssets", MLModelParentFolder, MLModelFileNameCoreML);
            }
            else if (Application.platform == RuntimePlatform.WebGLPlayer || 
                    Application.platform == RuntimePlatform.WindowsEditor || 
                    Application.platform == RuntimePlatform.WindowsPlayer)
            {
                path = Path.Combine(Application.dataPath, "StreamingAssets", MLModelParentFolder, MLModelFileNameOnnx);
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                // Get the file path in the application's persistent data path
                path = Path.Combine(Application.persistentDataPath, MLModelFileNameTflite);
                if (!File.Exists(path))
                {
                    // Get the file path in the StreamingAssets folder
                    string srcPath = Path.Combine(Application.streamingAssetsPath, MLModelParentFolder, MLModelFileNameTflite);
                    await CopyFile(srcPath, path);
                }
            }
            else
            {
                throw new Exception("Unsupported platform");
            }
            var model = await MLEdgeModel.Create(path);
            var predictor = new MLClassificationModelPredictor(model);
            return predictor;
        }
        // private readonly MLEdgeModel model;
        public readonly MLEdgeModel model;
        private MLClassificationModelPredictor(MLEdgeModel model)
        {
            this.model = model;
        }
        public unsafe string Predict(params MLFeature[] inputs)
        {
            var imageFeature = inputs[0] as MLImageFeature;
            if (imageFeature == null)
                throw new ArgumentException(@"predictor expects an image feature", nameof(inputs));
            // model.normalization was readonly, so cannot modify
            // (imageFeature.mean, imageFeature.std) = model.normalization;
            imageFeature.mean = imageFeatureMean;
            imageFeature.std = imageFeatureStd;
            imageFeature.aspectMode = model.aspectMode;
            
            //Predict
            var inputType = model.inputs[0] as MLImageType;
            using var inputFeature = (imageFeature as IMLEdgeFeature).Create(inputType);
            using var outputFeatures = model.Predict(inputFeature);
            var scores = new MLArrayFeature<float>(outputFeatures[0]).ToArray();
            var softmaxScores = Softmax(scores);
            Debug.Log(softmaxScores);
            float maxValue = softmaxScores.Max();
            int maxIndex = Array.IndexOf(softmaxScores, maxValue);
            string labelname = labels[maxIndex];
            return labelname + ":" + maxValue.ToString();
            // actual result is outputFeatures.Data. now we apply Softmax
            // var outputList = new List<float>();
            // for(int i = 0; i < 1000; i++)
            // {
            //     outputList.Add(outputFeatures[0].data[i]);
            // }
            // var scores = new MLArrayFeature<float>(outputFeatures[0]);
            // var softmaxScores = Softmax(scores);
            // int maxIndex = softmaxScores.IndexOf(softmaxScores.Max());
            // float maxValue = softmaxScores[maxIndex];
            // string labelname = labels[maxIndex];
            // return scores.ToArray().ToString();
            // var softmaxScores = Softmax(outputFeatures);
            // int maxIndex = softmaxScores.IndexOf(softmaxScores.Max());
            // float maxValue = softmaxScores[maxIndex];
            // string labelname = labels[maxIndex];

            // return labelname + ":" + maxValue.ToString();
        }
        void IDisposable.Dispose(){ }
        static async Task CopyFile(string srcPath, string dstPath)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(srcPath))
            {
                www.SendWebRequest();

                while (!www.isDone)
                {
                    await Task.Yield();
                }

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError("Error while copying file: " + www.error);
                }
                else
                {
                    File.WriteAllBytes(dstPath, www.downloadHandler.data);
                }
            }
        }        
    
        float[] Softmax(float[] input)
        {
            float max = input[0];
            for (int i = 1; i < input.Length; i++)
            {
                if (input[i] > max)
                    max = input[i];
            }

            float scale = 0.0f;
            for (int i = 0; i < input.Length; i++)
            {
                scale += Mathf.Exp(input[i] - max);
            }

            float[] output = new float[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = Mathf.Exp(input[i] - max) / scale;
            }

            return output;
        }
    }
    
}