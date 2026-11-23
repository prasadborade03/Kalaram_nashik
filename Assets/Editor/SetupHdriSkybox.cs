using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class SetupHdriSkybox
{
    private const string DoneKey = "HDRI_SETUP_COMPLETED_v3";

    static SetupHdriSkybox()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorPrefs.GetBool(DoneKey, false))
            {
                RunSetup();
                EditorPrefs.SetBool(DoneKey, true);
            }
        };
    }

    [MenuItem("Kalaram/Setup HDRI Skybox")]
    public static void RunSetupMenu()
    {
        RunSetup();
    }

    public static void RunSetup()
    {
        Debug.Log("[SetupHdriSkybox] Starting HDRI Skybox setup...");

        string hdriDir = "Assets/hdri";
        string hdriPath = hdriDir + "/citrus_orchard_road_puresky_2k.exr";
        string matPath = hdriDir + "/citrus_orchard_road_skybox.mat";

        if (!File.Exists(hdriPath))
        {
            Debug.LogError("[SetupHdriSkybox] HDRI file not found at " + hdriPath);
            return;
        }

        // 1. Refresh AssetDatabase and configure TextureImporter
        AssetDatabase.ImportAsset(hdriPath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(hdriPath) as TextureImporter;
        if (importer != null)
        {
            bool modified = false;
            if (importer.textureType != TextureImporterType.Default)
            {
                importer.textureType = TextureImporterType.Default;
                modified = true;
            }
            if (importer.textureShape != TextureImporterShape.Texture2D)
            {
                importer.textureShape = TextureImporterShape.Texture2D;
                modified = true;
            }
            if (importer.sRGBTexture != false)
            {
                importer.sRGBTexture = false; // HDR EXR is linear
                modified = true;
            }
            if (!importer.isReadable)
            {
                importer.isReadable = true;
                modified = true;
            }
            if (importer.wrapMode != TextureWrapMode.Repeat)
            {
                importer.wrapMode = TextureWrapMode.Repeat;
                modified = true;
            }
            if (importer.filterMode != FilterMode.Bilinear)
            {
                importer.filterMode = FilterMode.Bilinear;
                modified = true;
            }
            if (importer.maxTextureSize != 2048)
            {
                importer.maxTextureSize = 2048;
                modified = true;
            }
            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                modified = true;
            }

            if (modified)
            {
                importer.SaveAndReimport();
                Debug.Log("[SetupHdriSkybox] Configured TextureImporter for HDRI.");
            }
        }

        Texture2D hdriTex = AssetDatabase.LoadAssetAtPath<Texture2D>(hdriPath);
        if (hdriTex == null)
        {
            Debug.LogError("[SetupHdriSkybox] Could not load HDRI texture from " + hdriPath);
            return;
        }

        // 2. Locate Sun position in HDRI to calculate sun angle
        float sunAltitude = 45f;
        float sunAzimuth = 0f;
        try
        {
            int w = hdriTex.width;
            int h = hdriTex.height;
            // Sample downsampled grid to find brightest pixel
            int step = 4;
            float maxLum = -1f;
            int bestX = 0, bestY = 0;
            Color[] pixels = hdriTex.GetPixels();
            for (int y = h / 4; y < h; y += step) // Sun is in the sky (upper half)
            {
                for (int x = 0; x < w; x += step)
                {
                    Color c = pixels[y * w + x];
                    float lum = c.r * 0.2126f + c.g * 0.7152f + c.b * 0.0722f;
                    if (lum > maxLum)
                    {
                        maxLum = lum;
                        bestX = x;
                        bestY = y;
                    }
                }
            }
            float u = (bestX + 0.5f) / w;
            float v = (bestY + 0.5f) / h;
            sunAltitude = (v - 0.5f) * 180f;
            sunAzimuth = u * 360f;
            Debug.Log($"[SetupHdriSkybox] Detected sun in HDRI at u={u:F3}, v={v:F3} -> Altitude={sunAltitude:F1}°, Azimuth={sunAzimuth:F1}°, MaxLum={maxLum:F1}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[SetupHdriSkybox] Could not parse sun position: " + ex.Message);
        }

        // 3. Create or update Skybox Material using Skybox/Panoramic shader
        Shader panoShader = Shader.Find("Skybox/Panoramic");
        if (panoShader == null)
        {
            Debug.LogError("[SetupHdriSkybox] Shader 'Skybox/Panoramic' not found!");
            return;
        }

        Material skyMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (skyMat == null)
        {
            skyMat = new Material(panoShader);
            AssetDatabase.CreateAsset(skyMat, matPath);
            Debug.Log("[SetupHdriSkybox] Created new skybox material at " + matPath);
        }
        else
        {
            skyMat.shader = panoShader;
        }

        skyMat.SetTexture("_MainTex", hdriTex);
        skyMat.SetFloat("_Mapping", 1.0f); // 1 = Latitude Longitude Layout (Equirectangular)
        skyMat.SetFloat("_ImageType", 0.0f); // 0 = 360 degrees
        skyMat.SetFloat("_MirrorOnBack", 0.0f);
        skyMat.SetFloat("_Layout", 0.0f);
        skyMat.SetFloat("_Exposure", 1.15f); // Natural bright daylight
        skyMat.SetColor("_Tint", new Color(0.5f, 0.5f, 0.5f, 0.5f)); // Default 1.0 multiplier
        skyMat.SetFloat("_Rotation", 0.0f);

        EditorUtility.SetDirty(skyMat);
        AssetDatabase.SaveAssets();

        // 4. Apply to Scene RenderSettings
        RenderSettings.skybox = skyMat;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1.0f;
        RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Skybox;
        RenderSettings.defaultReflectionResolution = 512;
        RenderSettings.reflectionIntensity = 1.0f;
        RenderSettings.reflectionBounces = 2;

        // 5. Connect Directional Light as RenderSettings.sun and harmonize sun angle
        Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        Light dirLight = null;
        foreach (var l in lights)
        {
            if (l.type == LightType.Directional && l.gameObject.activeInHierarchy)
            {
                dirLight = l;
                break;
            }
        }

        if (dirLight != null)
        {
            RenderSettings.sun = dirLight;
            dirLight.shadows = LightShadows.Soft;
            dirLight.color = new Color(1.0f, 0.96f, 0.90f, 1.0f); // Warm natural sun tint
            dirLight.intensity = 1.5f;

            // Align skybox rotation or directional light rotation
            // If sunAltitude > 10, align directional light with sun altitude
            if (sunAltitude > 10f && sunAltitude < 85f)
            {
                // Align directional light pitch to match sun altitude, and skybox rotation to match light yaw
                // Light yaw is currently -30 deg (330 deg).
                // Let's set Skybox rotation so its sun azimuth matches light yaw!
                float lightYaw = dirLight.transform.eulerAngles.y;
                // Unity Skybox/Panoramic rotation rotates clockwise around Y
                float neededRotation = Mathf.Repeat(sunAzimuth - lightYaw + 180f, 360f);
                skyMat.SetFloat("_Rotation", neededRotation);
                dirLight.transform.rotation = Quaternion.Euler(sunAltitude, lightYaw, 0f);
                EditorUtility.SetDirty(dirLight);
                EditorUtility.SetDirty(skyMat);
                Debug.Log($"[SetupHdriSkybox] Harmonized Skybox rotation={neededRotation:F1}° with Directional Light (Pitch={sunAltitude:F1}°, Yaw={lightYaw:F1}°)");
            }
        }

        // 6. Update GI & Environment
        DynamicGI.UpdateEnvironment();

        // 7. Save Scene & Assets
        var activeScene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        Debug.Log("[SetupHdriSkybox] Successfully applied HDRI Skybox and saved scene!");

        // 8. Capture Verification Screenshots
        CaptureVerificationShots();
    }

    private static void CaptureVerificationShots()
    {
        string artifactDir = @"C:\Users\PRASAD BORADE\.gemini\antigravity\brain\8f451b1e-dad1-46a4-b036-688290c11753";
        if (!Directory.Exists(artifactDir))
        {
            Directory.CreateDirectory(artifactDir);
        }

        Action<string, Vector3, Vector3, float> takeShot = (fileName, pos, lookAt, fov) =>
        {
            var camGo = new GameObject("HdriVerifCam");
            var cam = camGo.AddComponent<Camera>();
            cam.transform.position = pos;
            cam.transform.LookAt(lookAt);
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 1000f;
            cam.fieldOfView = fov;
            cam.clearFlags = CameraClearFlags.Skybox;

            int width = 1280;
            int height = 720;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            string outPath = Path.Combine(artifactDir, fileName);
            File.WriteAllBytes(outPath, bytes);

            cam.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(camGo);

            Debug.Log("[SetupHdriSkybox] Saved screenshot: " + fileName);
        };

        try
        {
            // Shot 1: Mandap Facade & Sky
            takeShot("hdri_mandap_facade.png", new Vector3(14.0f, 6.0f, -68.0f), new Vector3(0.06f, 3.5f, -40.0f), 60f);

            // Shot 2: Wide Compound Aerial with Horizon & Sky
            takeShot("hdri_aerial_compound.png", new Vector3(35.0f, 25.0f, -45.0f), new Vector3(0.0f, 6.0f, -10.0f), 65f);

            // Shot 3: East Entrance Gateway under Sky
            takeShot("hdri_east_gateway.png", new Vector3(0.0f, 4.0f, -80.0f), new Vector3(0.0f, 8.0f, -30.0f), 70f);

            // Shot 4: Sabha Mandap Nave Interior with Natural HDRI Ambient Light
            takeShot("hdri_nave_interior.png", new Vector3(0.06f, 2.5f, -62.0f), new Vector3(0.06f, 2.5f, -35.0f), 60f);
        }
        catch (Exception ex)
        {
            Debug.LogError("[SetupHdriSkybox] Failed taking verification shots: " + ex);
        }
    }
}
