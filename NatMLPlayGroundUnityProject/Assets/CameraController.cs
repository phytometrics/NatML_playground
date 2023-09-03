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
