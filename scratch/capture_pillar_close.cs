var tempCamObj = new GameObject("TempCaptureCam");
var cam = tempCamObj.AddComponent<Camera>();
// Position camera looking closely at the pillar at (19.8, 3.8, 15.0)
cam.transform.position = new Vector3(16.0f, 3.8f, 15.0f);
cam.transform.LookAt(new Vector3(19.8f, 3.8f, 15.0f));
cam.fieldOfView = 50f;
cam.nearClipPlane = 0.1f;
cam.farClipPlane = 50f;

var rt = new RenderTexture(1024, 1024, 24);
cam.targetTexture = rt;
cam.Render();

RenderTexture.active = rt;
var tex = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
tex.ReadPixels(new Rect(0, 0, 1024, 1024), 0, 0);
tex.Apply();

byte[] bytes = tex.EncodeToPNG();
string savePath = Application.dataPath + "/pillar_close_view.png";
System.IO.File.WriteAllBytes(savePath, bytes);

// Cleanup
cam.targetTexture = null;
RenderTexture.active = null;
UnityEngine.Object.DestroyImmediate(rt);
UnityEngine.Object.DestroyImmediate(tex);
UnityEngine.Object.DestroyImmediate(tempCamObj);

AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
return "Saved close view to: " + savePath;
