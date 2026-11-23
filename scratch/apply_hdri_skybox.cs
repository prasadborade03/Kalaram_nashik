string hdriDir = "Assets/hdri";
string hdriPath = hdriDir + "/citrus_orchard_road_puresky_2k.exr";
string matPath = hdriDir + "/citrus_orchard_road_skybox.mat";

// 1. Ensure Texture is loaded
UnityEngine.Texture2D tex = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(hdriPath);
if (tex == null) return "Texture null at " + hdriPath;

// 2. Find sun position in HDRI
int w = tex.width;
int h = tex.height;
int step = 4;
float maxLum = -1f;
int bestX = 0, bestY = 0;
UnityEngine.Color[] pixels = tex.GetPixels();
for (int y = h / 4; y < h; y += step)
{
    for (int x = 0; x < w; x += step)
    {
        UnityEngine.Color c = pixels[y * w + x];
        float lum = c.r * 0.2126f + c.g * 0.7152f + c.b * 0.0722f;
        if (lum > maxLum)
        {
            maxLum = lum;
            bestX = x;
            bestY = y;
        }
    }
}

float u = (bestX + 0.5f) / (float)w;
float v = (bestY + 0.5f) / (float)h;
float sunAltitude = (v - 0.5f) * 180f;
float sunAzimuth = u * 360f;

// 3. Create or update Skybox Material
var panoShader = UnityEngine.Shader.Find("Skybox/Panoramic");
if (panoShader == null) return "Panoramic shader not found";

UnityEngine.Material skyMat = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(matPath);
if (skyMat == null)
{
    skyMat = new UnityEngine.Material(panoShader);
    UnityEditor.AssetDatabase.CreateAsset(skyMat, matPath);
}
else
{
    skyMat.shader = panoShader;
}

skyMat.SetTexture("_MainTex", tex);
skyMat.SetFloat("_Mapping", 1.0f); // 1 = Latitude Longitude Layout
skyMat.SetFloat("_ImageType", 0.0f); // 0 = 360 degrees
skyMat.SetFloat("_MirrorOnBack", 0.0f);
skyMat.SetFloat("_Layout", 0.0f);
skyMat.SetFloat("_Exposure", 1.05f);
skyMat.SetColor("_Tint", new UnityEngine.Color(0.5f, 0.5f, 0.5f, 0.5f));
skyMat.SetFloat("_Rotation", 0.0f);

UnityEditor.EditorUtility.SetDirty(skyMat);
UnityEditor.AssetDatabase.SaveAssets();

// 4. Set RenderSettings
UnityEngine.RenderSettings.skybox = skyMat;
UnityEngine.RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
UnityEngine.RenderSettings.ambientIntensity = 1.0f;
UnityEngine.RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Skybox;
UnityEngine.RenderSettings.defaultReflectionResolution = 512;
UnityEngine.RenderSettings.reflectionIntensity = 1.0f;
UnityEngine.RenderSettings.reflectionBounces = 2;

// 5. Connect Directional Light
UnityEngine.Light[] allLights = UnityEngine.Object.FindObjectsByType<UnityEngine.Light>(UnityEngine.FindObjectsSortMode.None);
UnityEngine.Light dirLight = null;
foreach (var l in allLights)
{
    if (l.type == UnityEngine.LightType.Directional && l.gameObject.activeInHierarchy)
    {
        dirLight = l;
        break;
    }
}

string lightReport = "No DirLight found";
if (dirLight != null)
{
    UnityEngine.RenderSettings.sun = dirLight;
    dirLight.shadows = UnityEngine.LightShadows.Soft;
    dirLight.color = new UnityEngine.Color(1.0f, 0.97f, 0.92f, 1.0f); // Warm natural sunlight
    dirLight.intensity = 1.6f;

    // Harmonize sun angle:
    // If sun altitude in HDRI is realistic (e.g. 15 to 75 deg), use it for pitch
    // Light original yaw:
    float lightYaw = dirLight.transform.eulerAngles.y;
    // Align Skybox rotation so sun matches light direction:
    float neededSkyRotation = Mathf.Repeat(sunAzimuth - lightYaw + 180f, 360f);
    skyMat.SetFloat("_Rotation", neededSkyRotation);
    dirLight.transform.rotation = UnityEngine.Quaternion.Euler(Mathf.Clamp(sunAltitude, 25f, 65f), lightYaw, 0f);

    UnityEditor.EditorUtility.SetDirty(dirLight);
    UnityEditor.EditorUtility.SetDirty(skyMat);
    lightReport = $"DirLight synced: Pitch={Mathf.Clamp(sunAltitude, 25f, 65f):F1}°, Yaw={lightYaw:F1}°, SkyRotation={neededSkyRotation:F1}°";
}

// 6. Update Environment Lighting
UnityEngine.DynamicGI.UpdateEnvironment();

// 7. Save Scene & Assets
var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
bool sceneSaved = UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene);
UnityEditor.AssetDatabase.SaveAssets();

// 8. Capture Verification Screenshots
string artifactDir = @"C:\Users\PRASAD BORADE\.gemini\antigravity\brain\8f451b1e-dad1-46a4-b036-688290c11753";
if (!System.IO.Directory.Exists(artifactDir))
{
    System.IO.Directory.CreateDirectory(artifactDir);
}

System.Action<string, UnityEngine.Vector3, UnityEngine.Vector3, float> takeShot = (fileName, pos, lookAt, fov) =>
{
    var camGo = new UnityEngine.GameObject("HdriVerifCam");
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

takeShot("hdri_mandap_facade.png", new UnityEngine.Vector3(14.0f, 6.0f, -68.0f), new UnityEngine.Vector3(0.06f, 3.5f, -40.0f), 60f);
takeShot("hdri_aerial_compound.png", new UnityEngine.Vector3(35.0f, 25.0f, -45.0f), new UnityEngine.Vector3(0.0f, 6.0f, -10.0f), 65f);
takeShot("hdri_east_gateway.png", new UnityEngine.Vector3(0.0f, 4.0f, -80.0f), new UnityEngine.Vector3(0.0f, 8.0f, -30.0f), 70f);
takeShot("hdri_nave_interior.png", new UnityEngine.Vector3(0.06f, 2.5f, -62.0f), new UnityEngine.Vector3(0.06f, 2.5f, -35.0f), 60f);

return $"SUCCESS! HDRI applied. Sun: altitude={sunAltitude:F1}°, azimuth={sunAzimuth:F1}°, maxLum={maxLum:F1}. {lightReport}. SceneSaved={sceneSaved}";
