using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ReplaceEnvironmentColumns : EditorWindow
{
    [MenuItem("Kalaram/Replace Columns with User Pillars")]
    public static string ExecuteReplacement()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== STARTING COLUMN REPLACEMENT PROCESS ===");

        // 1. Locate user's template pillar
        var userCol = GameObject.Find("my_North_Corr_West_Col_5");
        if (userCol == null)
        {
            string err = "ERROR: 'my_North_Corr_West_Col_5' not found in scene!";
            Debug.LogError(err);
            return err;
        }

        // Base center of userCol measured in world space
        var baseCenter = new Vector3(19.844f, 2.020f, 15.071f);
        var cloneUserCol = UnityEngine.Object.Instantiate(userCol);

        // 2. Create clean template with pivot at base center (0, 0, 0 local)
        var template = new GameObject("_CleanPillarTemplate");
        template.transform.position = baseCenter;
        template.transform.rotation = Quaternion.identity;
        template.transform.localScale = Vector3.one;

        while (cloneUserCol.transform.childCount > 0)
        {
            var child = cloneUserCol.transform.GetChild(0);
            child.SetParent(template.transform, true);
        }
        UnityEngine.Object.DestroyImmediate(cloneUserCol);

        sb.AppendLine("Clean template created at " + template.transform.position);

        int countNorth = 0;
        int countSouth = 0;
        int countWest = 0;
        int countMandap = 0;
        int countMandapCapitals = 0;
        int countHanuman = 0;

        // 3. Replace North Corridor Columns (20 columns)
        // Corbel bracket faces into courtyard (-X) -> rotY = 0
        var northCorr = GameObject.Find("KALARAM_ENVIRONMENT/PILLARED_CORRIDORS/North_Corridor");
        if (northCorr != null)
        {
            var cols = new List<GameObject>();
            for (int i = 0; i < northCorr.transform.childCount; i++)
            {
                var ch = northCorr.transform.GetChild(i).gameObject;
                if (ch.name.Contains("_Col_")) cols.Add(ch);
            }
            foreach (var col in cols)
            {
                var p = UnityEngine.Object.Instantiate(template);
                p.name = col.name;
                p.transform.position = new Vector3(col.transform.position.x, 2.020f, col.transform.position.z);
                p.transform.rotation = Quaternion.Euler(0, 0f, 0);
                p.transform.localScale = Vector3.one;
                p.transform.SetParent(northCorr.transform, true);

                UnityEngine.Object.DestroyImmediate(col);
                countNorth++;
            }
        }
        sb.AppendLine($"North Corridor: replaced {countNorth} columns (rotY = 0 deg, scale = 1.0)");

        // 4. Replace South Corridor Columns (20 columns)
        // Corbel bracket faces into courtyard (+X) -> rotY = 180
        var southCorr = GameObject.Find("KALARAM_ENVIRONMENT/PILLARED_CORRIDORS/South_Corridor");
        if (southCorr != null)
        {
            var cols = new List<GameObject>();
            for (int i = 0; i < southCorr.transform.childCount; i++)
            {
                var ch = southCorr.transform.GetChild(i).gameObject;
                if (ch.name.Contains("_Col_")) cols.Add(ch);
            }
            foreach (var col in cols)
            {
                var p = UnityEngine.Object.Instantiate(template);
                p.name = col.name;
                p.transform.position = new Vector3(col.transform.position.x, 2.020f, col.transform.position.z);
                p.transform.rotation = Quaternion.Euler(0, 180f, 0);
                p.transform.localScale = Vector3.one;
                p.transform.SetParent(southCorr.transform, true);

                UnityEngine.Object.DestroyImmediate(col);
                countSouth++;
            }
        }
        sb.AppendLine($"South Corridor: replaced {countSouth} columns (rotY = 180 deg, scale = 1.0)");

        // 5. Replace West Corridor Columns (8 columns)
        // Corbel bracket faces into courtyard (-Z) -> rotY = 270
        var westCorr = GameObject.Find("KALARAM_ENVIRONMENT/PILLARED_CORRIDORS/West_Corridor");
        if (westCorr != null)
        {
            var cols = new List<GameObject>();
            for (int i = 0; i < westCorr.transform.childCount; i++)
            {
                var ch = westCorr.transform.GetChild(i).gameObject;
                if (ch.name.Contains("_Col_")) cols.Add(ch);
            }
            foreach (var col in cols)
            {
                var p = UnityEngine.Object.Instantiate(template);
                p.name = col.name;
                p.transform.position = new Vector3(col.transform.position.x, 2.020f, col.transform.position.z);
                p.transform.rotation = Quaternion.Euler(0, 270f, 0);
                p.transform.localScale = Vector3.one;
                p.transform.SetParent(westCorr.transform, true);

                UnityEngine.Object.DestroyImmediate(col);
                countWest++;
            }
        }
        sb.AppendLine($"West Corridor: replaced {countWest} columns (rotY = 270 deg, scale = 1.0)");

        // 6. Replace Sabha Mandap Pillars (40 pillars) & cleanup redundant cube capitals
        // Clearance: 4.681 - 0.740 = 3.941m. Scale = 3.941 / 4.811 = 0.81916f.
        // C01, C02 face inward (+X) -> rotY = 180. C03, C04 face inward (-X) -> rotY = 0.
        var mandapPillars = GameObject.Find("KALARAM_ENVIRONMENT/SABHA_MANDAP/Pillars");
        if (mandapPillars != null)
        {
            var pillars = new List<GameObject>();
            var capitals = new List<GameObject>();
            for (int i = 0; i < mandapPillars.transform.childCount; i++)
            {
                var ch = mandapPillars.transform.GetChild(i).gameObject;
                if (ch.name.StartsWith("Pillar_Capital_")) capitals.Add(ch);
                else if (ch.name.StartsWith("Pillar_R")) pillars.Add(ch);
            }

            float mandapScale = 3.941f / 4.811f; // 0.81916f
            float mandapFloorY = 0.740f;

            foreach (var pillar in pillars)
            {
                var p = UnityEngine.Object.Instantiate(template);
                p.name = pillar.name;
                p.transform.position = new Vector3(pillar.transform.position.x, mandapFloorY, pillar.transform.position.z);

                // Determine column column index C01..C04
                int colIdx = 1;
                if (pillar.name.EndsWith("C01")) colIdx = 1;
                else if (pillar.name.EndsWith("C02")) colIdx = 2;
                else if (pillar.name.EndsWith("C03")) colIdx = 3;
                else if (pillar.name.EndsWith("C04")) colIdx = 4;

                float rotY = (colIdx <= 2) ? 180f : 0f;
                p.transform.rotation = Quaternion.Euler(0, rotY, 0);
                p.transform.localScale = Vector3.one * mandapScale;
                p.transform.SetParent(mandapPillars.transform, true);

                UnityEngine.Object.DestroyImmediate(pillar);
                countMandap++;
            }

            foreach (var cap in capitals)
            {
                UnityEngine.Object.DestroyImmediate(cap);
                countMandapCapitals++;
            }
        }
        sb.AppendLine($"Sabha Mandap: replaced {countMandap} pillars (scale = 0.8192, inward rot) and removed {countMandapCapitals} redundant cube capitals");

        // 7. Replace Hanuman Shrine Pillars (4 pillars)
        // Clearance: 3.805 - 0.886 = 2.919m. Scale = 2.919 / 4.811 = 0.6067f.
        var hanumanShrine = GameObject.Find("KALARAM_ENVIRONMENT/SMALL_SHRINES_AND_STRUCTURES/Hanuman_Shrine");
        if (hanumanShrine != null)
        {
            var hPillars = new List<GameObject>();
            for (int i = 0; i < hanumanShrine.transform.childCount; i++)
            {
                var ch = hanumanShrine.transform.GetChild(i).gameObject;
                if (ch.name.StartsWith("Hanuman_Pillar_")) hPillars.Add(ch);
            }

            float hScale = 2.919f / 4.811f; // 0.6067f
            float hFloorY = 0.886f;

            foreach (var pillar in hPillars)
            {
                var p = UnityEngine.Object.Instantiate(template);
                p.name = pillar.name;
                p.transform.position = new Vector3(pillar.transform.position.x, hFloorY, pillar.transform.position.z);
                float rotY = (pillar.transform.position.x < 11.0f) ? 180f : 0f;
                p.transform.rotation = Quaternion.Euler(0, rotY, 0);
                p.transform.localScale = Vector3.one * hScale;
                p.transform.SetParent(hanumanShrine.transform, true);

                UnityEngine.Object.DestroyImmediate(pillar);
                countHanuman++;
            }
        }
        sb.AppendLine($"Hanuman Shrine: replaced {countHanuman} pillars (scale = 0.6067)");

        // 8. Cleanup template and original exemplar
        UnityEngine.Object.DestroyImmediate(template);
        UnityEngine.Object.DestroyImmediate(userCol);
        sb.AppendLine("Cleaned up template and exemplar 'my_North_Corr_West_Col_5' from scene root.");

        // 9. Mark active scene dirty & save
        var activeScene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        bool saved = EditorSceneManager.SaveScene(activeScene);
        sb.AppendLine($"Scene '{activeScene.name}' saved: {saved}");

        int totalReplaced = countNorth + countSouth + countWest + countMandap + countHanuman;
        sb.AppendLine($"=== TOTAL PILLARS REPLACED: {totalReplaced} ===");

        Debug.Log(sb.ToString());
        return sb.ToString();
    }
}
