string artifactDir = @"C:\Users\PRASAD BORADE\.gemini\antigravity\brain\8f451b1e-dad1-46a4-b036-688290c11753";

System.Action<string, UnityEngine.Vector3, UnityEngine.Vector3, float> takeShot = (fileName, pos, lookAt, fov) =>
{
    var camGo = new UnityEngine.GameObject("HdriEyeCam");
    var cam = camGo.AddComponent<UnityEngine.Camera>();
    cam.transform.position = pos;
    cam.transform.LookAt(lookAt);
    cam.nearClipPlane = 0.05f;
    cam.farClipPlane = 1000f;
    cam.fieldOfView = fov;
    cam.clearFlags = UnityEngine.CameraClearFlags.Skybox;

    int width = 1280;
    int height = 720;
    var rt = new UnityEngine.RenderTexture(width, height, 24, UnityEngine.RenderTextureFormat.ARGB32);
    cam.targetTexture = rt;
    cam.Render();

    UnityEngine.RenderTexture.active = rt;
    var screenTex = new UnityEngine.Texture2D(width, height, UnityEngine.TextureFormat.RGB24, false);
    screenTex.ReadPixels(new UnityEngine.Rect(0, 0, width, height), 0, 0);
    screenTex.Apply();

    byte[] bytes = screenTex.EncodeToPNG();
    string outPath = System.IO.Path.Combine(artifactDir, fileName);
    System.IO.File.WriteAllBytes(outPath, bytes);

    cam.targetTexture = null;
    UnityEngine.RenderTexture.active = null;
    UnityEngine.Object.DestroyImmediate(rt);
    UnityEngine.Object.DestroyImmediate(screenTex);
    UnityEngine.Object.DestroyImmediate(camGo);
};

// 1. Courtyard eye-level view looking at Central Temple & Shikhara
takeShot("hdri_temple_shikhara_sky.png", new UnityEngine.Vector3(8.0f, 2.0f, -38.0f), new UnityEngine.Vector3(0.0f, 6.0f, -10.0f), 65f);

// 2. Deepstambha & Mandap Corner looking into the open sky
takeShot("hdri_deepstambha_mandap.png", new UnityEngine.Vector3(-10.0f, 2.0f, -65.0f), new UnityEngine.Vector3(0.0f, 5.0f, -50.0f), 65f);

// 3. Colonnade corridor view towards sunny courtyard
takeShot("hdri_corridor_sunlit.png", new UnityEngine.Vector3(17.0f, 2.0f, -15.0f), new UnityEngine.Vector3(10.0f, 2.0f, -15.0f), 70f);

return "Eye-level shots captured!";
