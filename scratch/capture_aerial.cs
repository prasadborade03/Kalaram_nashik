// Render an aerial view screenshot without modifying Main Camera
var tempCamObj = new GameObject("TempCaptureCam");
var cam = tempCamObj.AddComponent<Camera>();
cam.transform.position = new Vector3(45f, 40f, -35f);
cam.transform.LookAt(new Vector3(5f, 2f, 0f));
cam.fieldOfView = 50f;
cam.nearClipPlane = 0.3f;
cam.farClipPlane = 500f;

var rt = new RenderTexture(1920, 1080, 24);
cam.targetTexture = rt;
cam.Render();

RenderTexture.active = rt;
var tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
tex.Apply();

byte[] bytes = tex.EncodeToPNG();
string savePath = Application.dataPath + "/screenshot_environment_aerial.png";
System.IO.File.WriteAllBytes(savePath, bytes);

// Cleanup
cam.targetTexture = null;
RenderTexture.active = null;
UnityEngine.Object.DestroyImmediate(rt);
UnityEngine.Object.DestroyImmediate(tex);
UnityEngine.Object.DestroyImmediate(tempCamObj);

AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
return "Saved aerial screenshot to: " + savePath + " (" + bytes.Length + " bytes)";
