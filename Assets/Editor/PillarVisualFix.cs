using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// [InitializeOnLoad]
public static class PillarVisualFix
{
    // static PillarVisualFix()
    // {
    //     EditorApplication.delayCall += RunFix;
    // }

    [MenuItem("Kalaram/Fix Pillar Visual Bug")]
    public static void RunFix()
    {
        Debug.Log("[PillarVisualFix] Starting fix...");

        // 1. Fix material to be Two-Sided (Cull Off)
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/plane-stone_for_piller.mat");
        if (mat != null)
        {
            mat.SetFloat("_Cull", 0f); // 0 = Cull Off (Two Sided)
            mat.doubleSidedGI = true;
            mat.SetOverrideTag("RenderType", "Opaque");
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            Debug.Log("[PillarVisualFix] Set _Cull = 0 (Two Sided) on plane-stone_for_piller.mat");
        }

        var envMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/plane-stone.mat");
        if (envMat != null)
        {
            envMat.doubleSidedGI = true;
            EditorUtility.SetDirty(envMat);
            AssetDatabase.SaveAssets();
        }

        EditorPrefs.SetBool("MCPForUnity.AutoStartOnLoad", true);

        // 2. Render comprehensive verification shots
        RenderAllVerificationShots();
    }

    private static void RenderAllVerificationShots()
    {
        string artifactDir = @"C:\Users\PRASAD BORADE\.gemini\antigravity\brain\50f6dece-1829-4ffa-8dad-03444903a374";

        Action<string, Vector3, Vector3, float> takeShot = (fileName, pos, lookAt, fov) =>
        {
            var camGo = new GameObject("VerifCam");
            var cam = camGo.AddComponent<Camera>();
            cam.transform.position = pos;
            cam.transform.LookAt(lookAt);
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 300f;
            cam.fieldOfView = fov;

            int width = 1280;
            int height = 720;
            var rt = new RenderTexture(width, height, 24);
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

            Debug.Log("[PillarVisualFix] Saved: " + fileName);
        };

        // 1. Pillar Closeup (Direct front view of Sabha Mandap front pillar)
        var pTarget = GameObject.Find("Pillar_R10_C02");
        Vector3 pPos = pTarget != null ? pTarget.transform.position + Vector3.up * 1.5f : new Vector3(-1.89f, 2.5f, -57.32f);
        takeShot("pillar_closeup_solid.png", pPos + new Vector3(0.5f, 0.2f, -2.2f), pPos, 45f);

        // 2. Sabha Mandap Nave Interior View
        takeShot("mandap_nave_solid.png", new Vector3(0.06f, 2.5f, -62.0f), new Vector3(0.06f, 2.5f, -35.0f), 60f);

        // 3. Sabha Mandap Facade & Hanuman Shrine
        takeShot("mandap_facade_solid.png", new Vector3(14.0f, 6.0f, -68.0f), new Vector3(0.06f, 2.5f, -45.0f), 55f);

        // 4. North Corridor Colonnade
        takeShot("north_corridor_solid.png", new Vector3(15.0f, 3.5f, -20.0f), new Vector3(19.69f, 3.5f, 15.0f), 60f);

        // 5. South Corridor Colonnade
        takeShot("south_corridor_solid.png", new Vector3(-15.0f, 3.5f, -20.0f), new Vector3(-19.58f, 3.5f, 15.0f), 60f);

        // 6. Compound Aerial View
        takeShot("compound_aerial_solid.png", new Vector3(35.0f, 28.0f, -30.0f), new Vector3(0.0f, 5.0f, -10.0f), 60f);
    }
}
