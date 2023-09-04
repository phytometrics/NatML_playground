using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NatML.Vision;  

public class CameraController : MonoBehaviour
{
    private WebCamTexture webCamTexture;
    private Texture2D texture2DFrame;
    private MLClassificationModelPredictor predictor;
    public UIController uiController;
    private int frameCount = 0;
    private float timePassed = 0.0f;

    public Material rotate90Material;

    IEnumerator Init()
    {
        while(true){
            if (webCamTexture.width > 16 && webCamTexture.height > 16){
                // colors = new Color32[webCamTexture.width * webCamTexture.height];
                texture2DFrame = new Texture2D(webCamTexture.width, webCamTexture.height);
                break;
            }
            yield return null;
        }
    }
    async void Start()
    {
        webCamTexture = new WebCamTexture();
        webCamTexture.Play();
        predictor = await MLClassificationModelPredictor.Create();
        StartCoroutine(Init());
        uiController.rawImage.texture = (Texture) texture2DFrame;
        uiController.modelDetailsText.text = predictor.model.ToString();
    }

    // Update is called once per frame
    void Update()
    {
     if (texture2DFrame != null && webCamTexture.didUpdateThisFrame)
     {
        texture2DFrame.SetPixels32(webCamTexture.GetPixels32());
        texture2DFrame.Apply();

        // Check if on Android and image is not upright
        // different to Application.platform == RuntimePlatform.Android, below will be included in Unity Editor only when building for Android
        
        if (Application.platform == RuntimePlatform.Android && texture2DFrame.width > texture2DFrame.height && Screen.orientation == ScreenOrientation.Portrait)
        {
            // Create a temporary RenderTexture of the same size as the texture
            RenderTexture rt = RenderTexture.GetTemporary(texture2DFrame.width, texture2DFrame.height);
            
            // Set the RenderTexture as active and blit (copy) the texture to it using the Rotate90 material
            Graphics.Blit(texture2DFrame, rt, rotate90Material);
            
            // Now read the pixels back from RenderTexture to the original texture
            RenderTexture.active = rt;
            texture2DFrame.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            texture2DFrame.Apply();
            
            // Release the temporary RenderTexture
            RenderTexture.ReleaseTemporary(rt);
            
        }
        

        var predictions = predictor.Predict(texture2DFrame);
        texture2DFrame.Apply();
        
        // reflext output text
        uiController.outputText.text = predictions;   

        frameCount++;

     }
    //fps counter
    // update time passed
    timePassed += Time.deltaTime;

    // update fps every second
    if (timePassed >= 1.0f)
    {
        uiController.FPSText.text = string.Format("{0} fps", frameCount);
        frameCount = 0;
        timePassed = 0.0f;
    }
    }

}
