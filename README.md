# NatML PlayGround

NatML unity project(s) that runs your local model.

## Snapshots

*FPS display has a bottleneck in camera framerate and webcameraview rendering. inference seems to be much faster. Optimization of image resolution and other hyperparameters may lead to enhancement.*



- OSX (M1 Mac OSX13.5), MobileNetV3_Large_pytorch.coreml 

  <img src="README.assets/image-20230904161500452.png" width=25%></img>

- iOS (iphone 13 Pro iOS16.2), MobileNetV3_Large_pytorch.coreml

<img src = "README.assets/スクリーンショット 2023-09-04 19.40.22.png" width=25%></img>



- Android (Google Pixel 7 Pro, Android 13), MobileNetV3_Large_pytorch_float32.tflite

<img src = "README.assets/Screenshot_20230904-154341.png" width=25%></img>



## Model Compatibility



| Model            | Note                                                         | ONNX | TFLite | CoreML (OSX) | CoreML (iOS) |
| ---------------- | ------------------------------------------------------------ | ---- | ------ | ------------ | ------------ |
| MobileNetV3Large | pytorch official model -> CoreML(7.0b2);<br>pytorch -> ONNX(onnxruntime 1.15.1;opset 11);<br>ONNX -> TFLite (onnx2tf 1.7.3) |      | ✓      | ✓            | ✓            |

