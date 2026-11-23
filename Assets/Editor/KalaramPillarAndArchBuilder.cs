using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class KalaramPillarAndArchBuilder : EditorWindow
{
    private const float SPACING = 3.00224f; // Exact flush alignment center-to-center
    private const float ARCH_SPAN = 2.44224f;
    private const float SHAFT_WIDTH = 0.5600f;
    
    // Mandap floor and alignment
    private const float MANDAP_FLOOR_Y = 0.8328f;
    private const float MANDAP_ARCH_Y = 3.6080f;
    
    // Corridor floor and alignment
    private const float CORR_FLOOR_Y = 2.0200f;
    private const float CORR_ARCH_Y = CORR_FLOOR_Y + (MANDAP_ARCH_Y - MANDAP_FLOOR_Y); // 4.7952f

    private static readonly Vector3 PILLAR_SCALE = new Vector3(1.0000f, 1.0501f, 1.0000f);
    // User calibrated low-poly mandir-design scale:
    private static readonly Vector3 MANDIR_ARCH_SCALE = new Vector3(180.6628f, 255.3217f, 213.8829f);
    
    private static readonly Quaternion ARCH_ROT_Z = Quaternion.Euler(270f, 180f, 0f);
    private static readonly Quaternion ARCH_ROT_X = Quaternion.Euler(270f, 270f, 0f);

    static KalaramPillarAndArchBuilder()
    {
        EditorApplication.delayCall += () =>
        {
            string triggerFile = @"C:\Users\PRASAD BORADE\.gemini\antigravity\brain\50f6dece-1829-4ffa-8dad-03444903a374\scratch\trigger_clean_rebuild.flag";
            if (File.Exists(triggerFile))
            {
                File.Delete(triggerFile);
                ExecuteBuild();
            }
        };
    }

    [MenuItem("Kalaram/Build Exact Pillars and Arches")]
    public static string ExecuteBuild()
    {
        string logPath = @"C:\Users\PRASAD BORADE\.gemini\antigravity\brain\50f6dece-1829-4ffa-8dad-03444903a374\lowpoly_build_log.txt";
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== STARTING EXACT PILLAR AND LOW-POLY ARCH RECONSTRUCTION ===");

        // 1. Load source assets
        var pillarAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/mesh/Weathered_Stone_Pillar.fbx");
        var archAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/mesh/mandir-design.fbx");

        if (pillarAsset == null || archAsset == null)
        {
            string err = $"ERROR: Assets not found. Pillar: {pillarAsset != null}, Arch: {archAsset != null}";
            Debug.LogError(err);
            File.WriteAllText(logPath, err);
            return err;
        }

        // Materials
        Material stoneMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/plane-stone.mat");
        Material pillarMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/plane-stone_for_piller.mat");

        if (pillarMat != null)
        {
            pillarMat.SetFloat("_Cull", 0f); // Double-sided rendering
            pillarMat.doubleSidedGI = true;
            pillarMat.enableInstancing = true;
            EditorUtility.SetDirty(pillarMat);
            sb.AppendLine("Configured plane-stone_for_piller.mat (Cull=0, doubleSidedGI=true, enableInstancing=true)");
        }

        // 2. SABHA MANDAP
        sb.AppendLine("\n--- Building Sabha Mandap Colonnades ---");
        var mandap = GameObject.Find("KALARAM_ENVIRONMENT/SABHA_MANDAP");
        if (mandap == null)
        {
            Debug.LogError("SABHA_MANDAP not found!");
            return "ERROR: SABHA_MANDAP not found";
        }

        // Ensure clean empty Pillars parent
        Transform mandapPillarsParent = mandap.transform.Find("Pillars");
        if (mandapPillarsParent != null)
        {
            DestroyImmediate(mandapPillarsParent.gameObject);
        }
        var pillarsGo = new GameObject("Pillars");
        pillarsGo.transform.SetParent(mandap.transform, false);
        mandapPillarsParent = pillarsGo.transform;

        // Ensure clean empty Arches parent
        Transform mandapArchesParent = mandap.transform.Find("Arches");
        if (mandapArchesParent != null)
        {
            DestroyImmediate(mandapArchesParent.gameObject);
        }
        var archesGo = new GameObject("Arches");
        archesGo.transform.SetParent(mandap.transform, false);
        mandapArchesParent = archesGo.transform;

        // Column X coordinates (4 colonnades)
        float[] colX = new float[] { -5.779f, -1.886f, 2.005f, 5.898f };
        int numRows = 11;
        float startZ = -26.2920f;

        int mandapPillarCount = 0;
        int mandapArchCount = 0;

        // Spawn 11 rows of pillars and 10 bays of arches
        for (int r = 0; r < numRows; r++)
        {
            float z = startZ - (r * SPACING);
            for (int c = 0; c < colX.Length; c++)
            {
                var p = (GameObject)PrefabUtility.InstantiatePrefab(pillarAsset);
                p.name = $"Mandap_Pillar_R{r+1:D2}_C{c+1:D2}";
                p.transform.position = new Vector3(colX[c], MANDAP_FLOOR_Y, z);
                p.transform.rotation = Quaternion.identity;
                p.transform.localScale = PILLAR_SCALE;
                p.transform.SetParent(mandapPillarsParent, true);
                if (pillarMat != null) ApplyMaterial(p, pillarMat);
                mandapPillarCount++;

                // Spawn arch spanning along Z between row r and row r+1
                if (r < numRows - 1)
                {
                    float archZ = z - (SPACING * 0.5f);
                    var a = (GameObject)PrefabUtility.InstantiatePrefab(archAsset);
                    a.name = $"Mandap_Arch_Bay{r+1:D2}_C{c+1:D2}";
                    a.transform.position = new Vector3(colX[c], MANDAP_ARCH_Y, archZ);
                    a.transform.rotation = ARCH_ROT_Z;
                    a.transform.localScale = MANDIR_ARCH_SCALE;
                    a.transform.SetParent(mandapArchesParent, true);
                    if (pillarMat != null) ApplyMaterial(a, pillarMat);
                    mandapArchCount++;
                }
            }
        }
        sb.AppendLine($"Sabha Mandap: created {mandapPillarCount} pillars, {mandapArchCount} low-poly arches");

        // Align transverse beams with rows
        Transform beamsParent = mandap.transform.Find("Beams");
        if (beamsParent != null)
        {
            var oldBeams = new List<GameObject>();
            for (int i = 0; i < beamsParent.childCount; i++)
            {
                var ch = beamsParent.GetChild(i).gameObject;
                if (ch.name.Contains("Transverse") || ch.name.Contains("Rear") || ch.name.Contains("Front"))
                    oldBeams.Add(ch);
            }
            foreach (var b in oldBeams) DestroyImmediate(b);

            float beamWidthX = (colX[3] - colX[0]) + 2.0f; // 13.68m
            float centerX = (colX[0] + colX[3]) * 0.5f;

            for (int r = 0; r < numRows; r++)
            {
                float z = startZ - (r * SPACING);
                string bName = (r == 0) ? "Rear_Beam_West" : (r == numRows - 1) ? "Front_Beam_East" : $"Transverse_Beam_R{r+1:D2}";
                var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
                b.name = bName;
                b.transform.position = new Vector3(centerX, 5.4689f, z);
                b.transform.localScale = new Vector3(beamWidthX, 0.4378f, 0.7300f);
                b.transform.SetParent(beamsParent, true);
                if (stoneMat != null) b.GetComponent<Renderer>().sharedMaterial = stoneMat;
            }
            sb.AppendLine($"Aligned {numRows} transverse beams directly above pillar rows.");
        }

        // 3. NORTH CORRIDOR
        sb.AppendLine("\n--- Building North Corridor Colonnades ---");
        var northCorr = GameObject.Find("KALARAM_ENVIRONMENT/PILLARED_CORRIDORS/North_Corridor");
        if (northCorr != null)
        {
            var oldCols = new List<GameObject>();
            for (int i = 0; i < northCorr.transform.childCount; i++)
            {
                var ch = northCorr.transform.GetChild(i).gameObject;
                if (ch.name.Contains("_Col_") || ch.name.Contains("_Arch_") || ch.name.Contains("Pillar") || ch.name.Contains("Arch"))
                    oldCols.Add(ch);
            }
            foreach (var c in oldCols) DestroyImmediate(c);

            float corrX = 19.694f;
            int nWestCount = BuildLinearColonnade(northCorr.transform, "North_Corr_West", pillarAsset, archAsset,
                corrX, CORR_FLOOR_Y, 34.886f, -1.0f, 15, true, pillarMat);
            int nEastCount = BuildLinearColonnade(northCorr.transform, "North_Corr_East", pillarAsset, archAsset,
                corrX, CORR_FLOOR_Y, -23.498f, -1.0f, 15, true, pillarMat);

            sb.AppendLine($"North Corridor: West wing ({nWestCount} pillars, {nWestCount - 1} arches), East wing ({nEastCount} pillars, {nEastCount - 1} arches)");
        }

        // 4. SOUTH CORRIDOR
        sb.AppendLine("\n--- Building South Corridor Colonnades ---");
        var southCorr = GameObject.Find("KALARAM_ENVIRONMENT/PILLARED_CORRIDORS/South_Corridor");
        if (southCorr != null)
        {
            var oldCols = new List<GameObject>();
            for (int i = 0; i < southCorr.transform.childCount; i++)
            {
                var ch = southCorr.transform.GetChild(i).gameObject;
                if (ch.name.Contains("_Col_") || ch.name.Contains("_Arch_") || ch.name.Contains("Pillar") || ch.name.Contains("Arch"))
                    oldCols.Add(ch);
            }
            foreach (var c in oldCols) DestroyImmediate(c);

            float corrX = -19.575f;
            int sWestCount = BuildLinearColonnade(southCorr.transform, "South_Corr_West", pillarAsset, archAsset,
                corrX, CORR_FLOOR_Y, 34.886f, -1.0f, 15, true, pillarMat);
            int sEastCount = BuildLinearColonnade(southCorr.transform, "South_Corr_East", pillarAsset, archAsset,
                corrX, CORR_FLOOR_Y, -23.498f, -1.0f, 15, true, pillarMat);

            sb.AppendLine($"South Corridor: West wing ({sWestCount} pillars, {sWestCount - 1} arches), East wing ({sEastCount} pillars, {sEastCount - 1} arches)");
        }

        // 5. WEST CORRIDOR (runs along X)
        sb.AppendLine("\n--- Building West Corridor Colonnades ---");
        var westCorr = GameObject.Find("KALARAM_ENVIRONMENT/PILLARED_CORRIDORS/West_Corridor");
        if (westCorr != null)
        {
            var oldCols = new List<GameObject>();
            for (int i = 0; i < westCorr.transform.childCount; i++)
            {
                var ch = westCorr.transform.GetChild(i).gameObject;
                if (ch.name.Contains("_Col_") || ch.name.Contains("_Arch_") || ch.name.Contains("Pillar") || ch.name.Contains("Arch"))
                    oldCols.Add(ch);
            }
            foreach (var c in oldCols) DestroyImmediate(c);

            float corrZ = 34.577f;
            int wSouthCount = BuildLinearColonnadeAlongX(westCorr.transform, "West_Corr_South", pillarAsset, archAsset,
                -17.456f, CORR_FLOOR_Y, corrZ, +1.0f, 5, pillarMat);
            int wNorthCount = BuildLinearColonnadeAlongX(westCorr.transform, "West_Corr_North", pillarAsset, archAsset,
                5.447f, CORR_FLOOR_Y, corrZ, +1.0f, 5, pillarMat);

            sb.AppendLine($"West Corridor: South wing ({wSouthCount} pillars, {wSouthCount - 1} arches), North wing ({wNorthCount} pillars, {wNorthCount - 1} arches)");
        }

        // 6. HANUMAN SHRINE
        sb.AppendLine("\n--- Updating Hanuman Shrine ---");
        var hanuman = GameObject.Find("KALARAM_ENVIRONMENT/SMALL_SHRINES_AND_STRUCTURES/Hanuman_Shrine");
        if (hanuman != null)
        {
            // Restore Hanuman_Architrave cube if it was modified
            var oldArchitrave = hanuman.transform.Find("Hanuman_Architrave");
            if (oldArchitrave != null)
            {
                DestroyImmediate(oldArchitrave.gameObject);
            }
            var architraveCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            architraveCube.name = "Hanuman_Architrave";
            architraveCube.transform.position = new Vector3(25.0f, 0.8860f + 2.7f, 7.5f);
            architraveCube.transform.localScale = new Vector3(2.5f, 0.2f, 2.5f);
            architraveCube.transform.SetParent(hanuman.transform, true);
            if (stoneMat != null) architraveCube.GetComponent<Renderer>().sharedMaterial = stoneMat;

            // Pillars
            var oldPillars = new List<GameObject>();
            for (int i = 0; i < hanuman.transform.childCount; i++)
            {
                var ch = hanuman.transform.GetChild(i).gameObject;
                if (ch.name.Contains("Pillar")) oldPillars.Add(ch);
            }
            foreach (var p in oldPillars) DestroyImmediate(p);

            Vector3[] hPos = new Vector3[] {
                new Vector3(25.0f - 0.9f, 0.8860f + 0.6f, 7.5f - 0.9f),
                new Vector3(25.0f + 0.9f, 0.8860f + 0.6f, 7.5f - 0.9f),
                new Vector3(25.0f - 0.9f, 0.8860f + 0.6f, 7.5f + 0.9f),
                new Vector3(25.0f + 0.9f, 0.8860f + 0.6f, 7.5f + 0.9f)
            };

            for (int i = 0; i < 4; i++)
            {
                var p = (GameObject)PrefabUtility.InstantiatePrefab(pillarAsset);
                p.name = $"Hanuman_Pillar_{i/2}_{i%2}";
                p.transform.position = hPos[i];
                p.transform.rotation = Quaternion.identity;
                p.transform.localScale = new Vector3(0.55f, 0.60f, 0.55f);
                p.transform.SetParent(hanuman.transform, true);
                if (pillarMat != null) ApplyMaterial(p, pillarMat);
            }
            sb.AppendLine("Hanuman Shrine: Restored Hanuman_Architrave and placed 4 Weathered_Stone_Pillars.");
        }

        // 7. CLEAN UP ROOT EXEMPLARS
        var rootP1 = GameObject.Find("Weathered_Stone_Pillar");
        if (rootP1 != null && rootP1.transform.parent == null) DestroyImmediate(rootP1);

        var rootP2 = GameObject.Find("Weathered_Stone_Pillar (1)");
        if (rootP2 != null && rootP2.transform.parent == null) DestroyImmediate(rootP2);

        var rootArch1 = GameObject.Find("piller_inner_design");
        if (rootArch1 != null && rootArch1.transform.parent == null) DestroyImmediate(rootArch1);

        var rootArch2 = GameObject.Find("mandir-design");
        if (rootArch2 != null && rootArch2.transform.parent == null) DestroyImmediate(rootArch2);

        sb.AppendLine("Cleaned up root exemplar objects.");

        // 8. Mark dirty & save
        var activeScene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        bool saved = EditorSceneManager.SaveScene(activeScene);
        sb.AppendLine($"\nScene '{activeScene.name}' saved: {saved}");

        // 9. Render verification shots
        RenderVerificationShots(sb);

        sb.AppendLine("=== RECONSTRUCTION COMPLETE ===");
        File.WriteAllText(logPath, sb.ToString());
        Debug.Log(sb.ToString());
        return sb.ToString();
    }

    private static void ApplyMaterial(GameObject go, Material mat)
    {
        if (go == null || mat == null) return;
        var rends = go.GetComponentsInChildren<Renderer>(true);
        foreach (var r in rends) r.sharedMaterial = mat;
    }

    private static int BuildLinearColonnade(Transform parent, string prefix, GameObject pillarAsset, GameObject archAsset,
        float lineX, float floorY, float startZ, float dirZ, int count, bool addArches, Material pillarMat)
    {
        for (int i = 0; i < count; i++)
        {
            float z = startZ + (i * dirZ * SPACING);
            var p = (GameObject)PrefabUtility.InstantiatePrefab(pillarAsset);
            p.name = $"{prefix}_Pillar_{i+1:D2}";
            p.transform.position = new Vector3(lineX, floorY, z);
            p.transform.rotation = Quaternion.identity;
            p.transform.localScale = PILLAR_SCALE;
            p.transform.SetParent(parent, true);
            if (pillarMat != null) ApplyMaterial(p, pillarMat);

            if (addArches && i < count - 1)
            {
                float archZ = z + (dirZ * SPACING * 0.5f);
                var a = (GameObject)PrefabUtility.InstantiatePrefab(archAsset);
                a.name = $"{prefix}_Arch_{i+1:D2}";
                a.transform.position = new Vector3(lineX, floorY + (MANDAP_ARCH_Y - MANDAP_FLOOR_Y), archZ);
                a.transform.rotation = ARCH_ROT_Z;
                a.transform.localScale = MANDIR_ARCH_SCALE;
                a.transform.SetParent(parent, true);
                if (pillarMat != null) ApplyMaterial(a, pillarMat);
            }
        }
        return count;
    }

    private static int BuildLinearColonnadeAlongX(Transform parent, string prefix, GameObject pillarAsset, GameObject archAsset,
        float startX, float floorY, float lineZ, float dirX, int count, Material pillarMat)
    {
        for (int i = 0; i < count; i++)
        {
            float x = startX + (i * dirX * SPACING);
            var p = (GameObject)PrefabUtility.InstantiatePrefab(pillarAsset);
            p.name = $"{prefix}_Pillar_{i+1:D2}";
            p.transform.position = new Vector3(x, floorY, lineZ);
            p.transform.rotation = Quaternion.identity;
            p.transform.localScale = PILLAR_SCALE;
            p.transform.SetParent(parent, true);
            if (pillarMat != null) ApplyMaterial(p, pillarMat);

            if (i < count - 1)
            {
                float archX = x + (dirX * SPACING * 0.5f);
                var a = (GameObject)PrefabUtility.InstantiatePrefab(archAsset);
                a.name = $"{prefix}_Arch_{i+1:D2}";
                a.transform.position = new Vector3(archX, floorY + (MANDAP_ARCH_Y - MANDAP_FLOOR_Y), lineZ);
                a.transform.rotation = ARCH_ROT_X;
                a.transform.localScale = MANDIR_ARCH_SCALE;
                a.transform.SetParent(parent, true);
                if (pillarMat != null) ApplyMaterial(a, pillarMat);
            }
        }
        return count;
    }

    private static void RenderVerificationShots(System.Text.StringBuilder sb)
    {
        string artifactDir = @"C:\Users\PRASAD BORADE\.gemini\antigravity\brain\50f6dece-1829-4ffa-8dad-03444903a374";

        var camGo = new GameObject("VerifyCamLowPoly");
        var cam = camGo.AddComponent<Camera>();
        cam.fieldOfView = 55f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 300f;

        var shots = new (string filename, Vector3 pos, Vector3 target)[]
        {
            // 1. Sabha Mandap arch closeup (looking at the first bay between R01 and R02 at Column 4)
            ("mandap_arch_closeup_lowpoly.png", new Vector3(5.898f, 2.5f, -24.5f), new Vector3(5.898f, 3.2f, -27.8f)),
            // 2. Sabha Mandap central interior nave
            ("mandap_interior_nave_lowpoly.png", new Vector3(0.06f, 2.2f, -23.0f), new Vector3(0.06f, 2.6f, -38.0f)),
            // 3. Sabha Mandap exterior facade looking from East
            ("mandap_facade_lowpoly.png", new Vector3(0.06f, 4.0f, -62.0f), new Vector3(0.06f, 4.0f, -40.0f)),
            // 4. Courtyard towards North Corridor colonnade
            ("courtyard_north_corridor_lowpoly.png", new Vector3(12.0f, 3.5f, 0.0f), new Vector3(19.694f, 4.5f, 5.0f)),
            // 5. Compound aerial overview
            ("compound_aerial_lowpoly.png", new Vector3(0.0f, 45.0f, -55.0f), new Vector3(0.0f, 0.0f, 0.0f))
        };

        var rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;

        foreach (var shot in shots)
        {
            camGo.transform.position = shot.pos;
            camGo.transform.LookAt(shot.target);
            cam.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            byte[] bytes = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);

            string outPath = Path.Combine(artifactDir, shot.filename);
            File.WriteAllBytes(outPath, bytes);
            sb.AppendLine($"Captured screenshot: {shot.filename}");
        }

        cam.targetTexture = null;
        RenderTexture.active = null;
        UnityEngine.Object.DestroyImmediate(rt);
        UnityEngine.Object.DestroyImmediate(camGo);
    }
}
