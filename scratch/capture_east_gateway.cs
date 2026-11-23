var tempCamObj = new GameObject("TempCaptureCam");
var cam = tempCamObj.AddComponent<Camera>();
cam.transform.position = new Vector3(45.0f, 4.0f, 0f);
cam.transform.LookAt(new Vector3(30.0f, 3.5f, 0f));
cam.fieldOfView = 65f;
cam.nearClipPlane = 0.3f;
cam.farClipPlane = 100f;

var rt = new RenderTexture(1920, 1080, 24);
cam.targetTexture = rt;
cam.Render();

RenderTexture.active = rt;
var tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
tex.Apply();

byte[] bytes = tex.EncodeToPNG();
string savePath = Application.dataPath + "/east_gateway_view.png";
System.IO.File.WriteAllBytes(savePath, bytes);

// Cleanup
cam.targetTexture = null;
RenderTexture.active = null;
UnityEngine.Object.DestroyImmediate(rt);
UnityEngine.Object.DestroyImmediate(tex);
UnityEngine.Object.DestroyImmediate(tempCamObj);

AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
return "Saved gateway view to: " + savePath;
