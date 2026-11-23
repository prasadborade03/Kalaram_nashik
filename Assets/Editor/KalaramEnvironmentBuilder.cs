using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public static class KalaramEnvironmentBuilder
{
    private const string ROOT_NAME = "KALARAM_ENVIRONMENT";
    private const float GROUND_Y = 0.000f; // Determined from floor Renderer bounds

    // Provisional real-world dimensions
    private const float COMPOUND_X_MIN = -37.338f;
    private const float COMPOUND_X_MAX = 37.338f;
    private const float COMPOUND_Z_MIN = -16.002f;
    private const float COMPOUND_Z_MAX = 16.002f;
    private const float WALL_HEIGHT = 5.182f;
    private const float WALL_THICKNESS = 0.600f;

    // Sabha Mandap dimensions
    private const float MANDAP_LENGTH = 22.860f; // along X
    private const float MANDAP_WIDTH = 9.449f;   // along Z
    private const float MANDAP_HEIGHT = 3.658f;  // along Y
    private const float MANDAP_START_X = 13.400f; // in front of temple (+X)
    private const float MANDAP_CENTER_Z = 0.000f;

    [MenuItem("Kalaram/Build Environment")]
    public static string BuildEnvironment()
    {
        // 1. Verify existing scene elements (Invariants)
        GameObject floorObj = GameObject.Find("floor");
        if (floorObj == null) return "ERROR: 'floor' GameObject not found!";

        GameObject kalaramObj = GameObject.Find("kalaram_glb");
        if (kalaramObj == null) return "ERROR: 'kalaram_glb' GameObject not found!";

        // Record transforms before building to guarantee they are untouched
        Vector3 floorPosBefore = floorObj.transform.position;
        Vector3 floorScaleBefore = floorObj.transform.localScale;
        Quaternion floorRotBefore = floorObj.transform.rotation;

        Vector3 kalaramPosBefore = kalaramObj.transform.position;
        Vector3 kalaramScaleBefore = kalaramObj.transform.localScale;
        Quaternion kalaramRotBefore = kalaramObj.transform.rotation;

        // Try load basalt stone material
        Material stoneMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/floor1.mat");

        // 2. Remove existing KALARAM_ENVIRONMENT if present (clean idempotent rebuild)
        GameObject existingRoot = GameObject.Find(ROOT_NAME);
        if (existingRoot != null)
        {
            Undo.DestroyObjectImmediate(existingRoot);
        }

        // 3. Create root GameObject
        GameObject root = new GameObject(ROOT_NAME);
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;
        Undo.RegisterCreatedObjectUndo(root, "Create " + ROOT_NAME);

        int totalCubes = 0;

        // Helper function for creating primitive Cube GameObjects
        System.Func<string, Vector3, Vector3, Transform, GameObject> createCube = (name, pos, scale, parent) =>
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.parent = parent;
            cube.transform.position = pos;
            cube.transform.localScale = scale;
            cube.transform.rotation = Quaternion.identity;

            if (stoneMat != null)
            {
                var rend = cube.GetComponent<Renderer>();
                if (rend != null) rend.sharedMaterial = stoneMat;
            }

            totalCubes++;
            return cube;
        };

        // =========================================================================
        // GROUP 01 — BOUNDARY_WALLS
        // =========================================================================
        GameObject grpWalls = new GameObject("BOUNDARY_WALLS");
        grpWalls.transform.parent = root.transform;

        float wallCenterY = GROUND_Y + (WALL_HEIGHT / 2f);

        // North Wall (along Z = +16.002) - Opening between X = -4.0 and +4.0
        GameObject northWallGrp = new GameObject("North_Wall");
        northWallGrp.transform.parent = grpWalls.transform;
        {
            float zPos = COMPOUND_Z_MAX - (WALL_THICKNESS / 2f);
            // West section: X from -37.338 to -4.0 (length = 33.338)
            int segments = 4;
            float segLenW = 33.338f / segments;
            for (int i = 0; i < segments; i++)
            {
                float xCenter = COMPOUND_X_MIN + (i + 0.5f) * segLenW;
                createCube($"North_Wall_West_Seg_{i + 1}", new Vector3(xCenter, wallCenterY, zPos), new Vector3(segLenW, WALL_HEIGHT, WALL_THICKNESS), northWallGrp.transform);
                // Coping atop wall
                createCube($"North_Wall_West_Coping_{i + 1}", new Vector3(xCenter, GROUND_Y + WALL_HEIGHT + 0.1f, zPos), new Vector3(segLenW, 0.2f, WALL_THICKNESS + 0.2f), northWallGrp.transform);
            }
            // East section: X from +4.0 to +37.338 (length = 33.338)
            float segLenE = 33.338f / segments;
            for (int i = 0; i < segments; i++)
            {
                float xCenter = 4.0f + (i + 0.5f) * segLenE;
                createCube($"North_Wall_East_Seg_{i + 1}", new Vector3(xCenter, wallCenterY, zPos), new Vector3(segLenE, WALL_HEIGHT, WALL_THICKNESS), northWallGrp.transform);
                // Coping atop wall
                createCube($"North_Wall_East_Coping_{i + 1}", new Vector3(xCenter, GROUND_Y + WALL_HEIGHT + 0.1f, zPos), new Vector3(segLenE, 0.2f, WALL_THICKNESS + 0.2f), northWallGrp.transform);
            }
        }

        // South Wall (along Z = -16.002) - Opening between X = -4.0 and +4.0
        GameObject southWallGrp = new GameObject("South_Wall");
        southWallGrp.transform.parent = grpWalls.transform;
        {
            float zPos = COMPOUND_Z_MIN + (WALL_THICKNESS / 2f);
            int segments = 4;
            float segLenW = 33.338f / segments;
            for (int i = 0; i < segments; i++)
            {
                float xCenter = COMPOUND_X_MIN + (i + 0.5f) * segLenW;
                createCube($"South_Wall_West_Seg_{i + 1}", new Vector3(xCenter, wallCenterY, zPos), new Vector3(segLenW, WALL_HEIGHT, WALL_THICKNESS), southWallGrp.transform);
                createCube($"South_Wall_West_Coping_{i + 1}", new Vector3(xCenter, GROUND_Y + WALL_HEIGHT + 0.1f, zPos), new Vector3(segLenW, 0.2f, WALL_THICKNESS + 0.2f), southWallGrp.transform);
            }
            float segLenE = 33.338f / segments;
            for (int i = 0; i < segments; i++)
            {
                float xCenter = 4.0f + (i + 0.5f) * segLenE;
                createCube($"South_Wall_East_Seg_{i + 1}", new Vector3(xCenter, wallCenterY, zPos), new Vector3(segLenE, WALL_HEIGHT, WALL_THICKNESS), southWallGrp.transform);
                createCube($"South_Wall_East_Coping_{i + 1}", new Vector3(xCenter, GROUND_Y + WALL_HEIGHT + 0.1f, zPos), new Vector3(segLenE, 0.2f, WALL_THICKNESS + 0.2f), southWallGrp.transform);
            }
        }

        // East Wall (along X = +37.338) - Opening between Z = -3.5 and +3.5
        GameObject eastWallGrp = new GameObject("East_Wall");
        eastWallGrp.transform.parent = grpWalls.transform;
        {
            float xPos = COMPOUND_X_MAX - (WALL_THICKNESS / 2f);
            // South segment: Z from -16.002 to -3.5 (length = 12.502)
            int segments = 3;
            float segLenS = 12.502f / segments;
            for (int i = 0; i < segments; i++)
            {
                float zCenter = COMPOUND_Z_MIN + (i + 0.5f) * segLenS;
                createCube($"East_Wall_South_Seg_{i + 1}", new Vector3(xPos, wallCenterY, zCenter), new Vector3(WALL_THICKNESS, WALL_HEIGHT, segLenS), eastWallGrp.transform);
                createCube($"East_Wall_South_Coping_{i + 1}", new Vector3(xPos, GROUND_Y + WALL_HEIGHT + 0.1f, zCenter), new Vector3(WALL_THICKNESS + 0.2f, 0.2f, segLenS), eastWallGrp.transform);
            }
            // North segment: Z from +3.5 to +16.002 (length = 12.502)
            float segLenN = 12.502f / segments;
            for (int i = 0; i < segments; i++)
            {
                float zCenter = 3.5f + (i + 0.5f) * segLenN;
                createCube($"East_Wall_North_Seg_{i + 1}", new Vector3(xPos, wallCenterY, zCenter), new Vector3(WALL_THICKNESS, WALL_HEIGHT, segLenN), eastWallGrp.transform);
                createCube($"East_Wall_North_Coping_{i + 1}", new Vector3(xPos, GROUND_Y + WALL_HEIGHT + 0.1f, zCenter), new Vector3(WALL_THICKNESS + 0.2f, 0.2f, segLenN), eastWallGrp.transform);
            }
        }

        // West Wall (along X = -37.338) - Opening between Z = -3.0 and +3.0
        GameObject westWallGrp = new GameObject("West_Wall");
        westWallGrp.transform.parent = grpWalls.transform;
        {
            float xPos = COMPOUND_X_MIN + (WALL_THICKNESS / 2f);
            // South segment: Z from -16.002 to -3.0 (length = 13.002)
            int segments = 3;
            float segLenS = 13.002f / segments;
            for (int i = 0; i < segments; i++)
            {
                float zCenter = COMPOUND_Z_MIN + (i + 0.5f) * segLenS;
                createCube($"West_Wall_South_Seg_{i + 1}", new Vector3(xPos, wallCenterY, zCenter), new Vector3(WALL_THICKNESS, WALL_HEIGHT, segLenS), westWallGrp.transform);
                createCube($"West_Wall_South_Coping_{i + 1}", new Vector3(xPos, GROUND_Y + WALL_HEIGHT + 0.1f, zCenter), new Vector3(WALL_THICKNESS + 0.2f, 0.2f, segLenS), westWallGrp.transform);
            }
            // North segment: Z from +3.0 to +16.002 (length = 13.002)
            float segLenN = 13.002f / segments;
            for (int i = 0; i < segments; i++)
            {
                float zCenter = 3.0f + (i + 0.5f) * segLenN;
                createCube($"West_Wall_North_Seg_{i + 1}", new Vector3(xPos, wallCenterY, zCenter), new Vector3(WALL_THICKNESS, WALL_HEIGHT, segLenN), westWallGrp.transform);
                createCube($"West_Wall_North_Coping_{i + 1}", new Vector3(xPos, GROUND_Y + WALL_HEIGHT + 0.1f, zCenter), new Vector3(WALL_THICKNESS + 0.2f, 0.2f, segLenN), westWallGrp.transform);
            }
        }

        // =========================================================================
        // GROUP 02 — ENTRANCE_GATEWAYS
        // =========================================================================
        GameObject grpGateways = new GameObject("ENTRANCE_GATEWAYS");
        grpGateways.transform.parent = root.transform;

        // East Gateway (Grand Mahadwar / Nagarkhana entrance) at X = +37.338
        GameObject eastGate = new GameObject("East_Gateway");
        eastGate.transform.parent = grpGateways.transform;
        {
            float gx = COMPOUND_X_MAX - 0.3f;
            // Left & Right Gate Bastions / Bases
            createCube("East_Gate_Base_South", new Vector3(gx, GROUND_Y + 0.4f, -4.2f), new Vector3(1.6f, 0.8f, 1.4f), eastGate.transform);
            createCube("East_Gate_Base_North", new Vector3(gx, GROUND_Y + 0.4f, 4.2f), new Vector3(1.6f, 0.8f, 1.4f), eastGate.transform);
            // Flanking Gate Pillars / Towers
            createCube("East_Gate_Pillar_South", new Vector3(gx, GROUND_Y + 3.3f, -4.2f), new Vector3(1.4f, 5.0f, 1.2f), eastGate.transform);
            createCube("East_Gate_Pillar_North", new Vector3(gx, GROUND_Y + 3.3f, 4.2f), new Vector3(1.4f, 5.0f, 1.2f), eastGate.transform);
            // Lintel Architrave Beam spanning across Z = -3.5 to +3.5
            createCube("East_Gate_Beam_Lower", new Vector3(gx, GROUND_Y + 4.9f, 0f), new Vector3(1.4f, 0.6f, 7.2f), eastGate.transform);
            createCube("East_Gate_Beam_Upper", new Vector3(gx, GROUND_Y + 5.5f, 0f), new Vector3(1.6f, 0.6f, 7.6f), eastGate.transform);
            // Superstructure (Nagarkhana drum-chamber pavilion atop gateway)
            createCube("East_Gate_Nagarkhana_Pavilion", new Vector3(gx, GROUND_Y + 6.6f, 0f), new Vector3(2.0f, 1.6f, 6.0f), eastGate.transform);
            createCube("East_Gate_Nagarkhana_Roof", new Vector3(gx, GROUND_Y + 7.6f, 0f), new Vector3(2.4f, 0.4f, 6.4f), eastGate.transform);
            createCube("East_Gate_Crest_Detail", new Vector3(gx, GROUND_Y + 8.1f, 0f), new Vector3(1.0f, 0.6f, 3.0f), eastGate.transform);
            // Threshold Block
            createCube("East_Gate_Threshold", new Vector3(gx, GROUND_Y + 0.1f, 0f), new Vector3(1.2f, 0.2f, 6.8f), eastGate.transform);
        }

        // West Gateway at X = -37.338
        GameObject westGate = new GameObject("West_Gateway");
        westGate.transform.parent = grpGateways.transform;
        {
            float gx = COMPOUND_X_MIN + 0.3f;
            createCube("West_Gate_Base_South", new Vector3(gx, GROUND_Y + 0.4f, -3.6f), new Vector3(1.4f, 0.8f, 1.2f), westGate.transform);
            createCube("West_Gate_Base_North", new Vector3(gx, GROUND_Y + 0.4f, 3.6f), new Vector3(1.4f, 0.8f, 1.2f), westGate.transform);
            createCube("West_Gate_Pillar_South", new Vector3(gx, GROUND_Y + 3.1f, -3.6f), new Vector3(1.2f, 4.6f, 1.0f), westGate.transform);
            createCube("West_Gate_Pillar_North", new Vector3(gx, GROUND_Y + 3.1f, 3.6f), new Vector3(1.2f, 4.6f, 1.0f), westGate.transform);
            createCube("West_Gate_Beam", new Vector3(gx, GROUND_Y + 5.1f, 0f), new Vector3(1.4f, 0.6f, 6.2f), westGate.transform);
            createCube("West_Gate_Crest", new Vector3(gx, GROUND_Y + 5.9f, 0f), new Vector3(1.6f, 1.0f, 4.0f), westGate.transform);
            createCube("West_Gate_Threshold", new Vector3(gx, GROUND_Y + 0.1f, 0f), new Vector3(1.0f, 0.2f, 5.8f), westGate.transform);
        }

        // North Gateway at Z = +16.002
        GameObject northGate = new GameObject("North_Gateway");
        northGate.transform.parent = grpGateways.transform;
        {
            float gz = COMPOUND_Z_MAX - 0.3f;
            createCube("North_Gate_Base_West", new Vector3(-4.6f, GROUND_Y + 0.4f, gz), new Vector3(1.2f, 0.8f, 1.4f), northGate.transform);
            createCube("North_Gate_Base_East", new Vector3(4.6f, GROUND_Y + 0.4f, gz), new Vector3(1.2f, 0.8f, 1.4f), northGate.transform);
            createCube("North_Gate_Pillar_West", new Vector3(-4.6f, GROUND_Y + 3.1f, gz), new Vector3(1.0f, 4.6f, 1.2f), northGate.transform);
            createCube("North_Gate_Pillar_East", new Vector3(4.6f, GROUND_Y + 3.1f, gz), new Vector3(1.0f, 4.6f, 1.2f), northGate.transform);
            createCube("North_Gate_Beam", new Vector3(0f, GROUND_Y + 5.1f, gz), new Vector3(8.2f, 0.6f, 1.4f), northGate.transform);
            createCube("North_Gate_Crest", new Vector3(0f, GROUND_Y + 5.9f, gz), new Vector3(5.0f, 1.0f, 1.6f), northGate.transform);
            createCube("North_Gate_Threshold", new Vector3(0f, GROUND_Y + 0.1f, gz), new Vector3(7.8f, 0.2f, 1.0f), northGate.transform);
        }

        // South Gateway at Z = -16.002
        GameObject southGate = new GameObject("South_Gateway");
        southGate.transform.parent = grpGateways.transform;
        {
            float gz = COMPOUND_Z_MIN + 0.3f;
            createCube("South_Gate_Base_West", new Vector3(-4.6f, GROUND_Y + 0.4f, gz), new Vector3(1.2f, 0.8f, 1.4f), southGate.transform);
            createCube("South_Gate_Base_East", new Vector3(4.6f, GROUND_Y + 0.4f, gz), new Vector3(1.2f, 0.8f, 1.4f), southGate.transform);
            createCube("South_Gate_Pillar_West", new Vector3(-4.6f, GROUND_Y + 3.1f, gz), new Vector3(1.0f, 4.6f, 1.2f), southGate.transform);
            createCube("South_Gate_Pillar_East", new Vector3(4.6f, GROUND_Y + 3.1f, gz), new Vector3(1.0f, 4.6f, 1.2f), southGate.transform);
            createCube("South_Gate_Beam", new Vector3(0f, GROUND_Y + 5.1f, gz), new Vector3(8.2f, 0.6f, 1.4f), southGate.transform);
            createCube("South_Gate_Crest", new Vector3(0f, GROUND_Y + 5.9f, gz), new Vector3(5.0f, 1.0f, 1.6f), southGate.transform);
            createCube("South_Gate_Threshold", new Vector3(0f, GROUND_Y + 0.1f, gz), new Vector3(7.8f, 0.2f, 1.0f), southGate.transform);
        }

        // =========================================================================
        // GROUP 03 — SABHA_MANDAP
        // =========================================================================
        // Placed in front of main temple (East side, +X)
        // Length = 22.860, Width = 9.449, Height = 3.658
        // X from 13.400 to 36.260 (center X = 24.830)
        // Z from -4.7245 to +4.7245 (center Z = 0)
        GameObject grpMandap = new GameObject("SABHA_MANDAP");
        grpMandap.transform.parent = root.transform;

        float mandapCenterX = MANDAP_START_X + (MANDAP_LENGTH / 2f);
        float plinthHeight = 0.500f;
        float pillarHeight = 2.550f;
        float beamHeight = 0.300f;
        float roofHeight = 0.308f; // total = 0.500 + 2.550 + 0.300 + 0.308 = 3.658m!

        // 1. Floor Platform (Modular plinth blocks)
        GameObject mandapPlinthGrp = new GameObject("Floor_Platform");
        mandapPlinthGrp.transform.parent = grpMandap.transform;
        {
            // Divide platform into 4 modular plinth slabs along length
            int plinthSlices = 4;
            float sliceLen = MANDAP_LENGTH / plinthSlices;
            for (int i = 0; i < plinthSlices; i++)
            {
                float px = MANDAP_START_X + (i + 0.5f) * sliceLen;
                createCube($"Mandap_Plinth_Slab_{i + 1}", new Vector3(px, GROUND_Y + (plinthHeight / 2f), MANDAP_CENTER_Z), new Vector3(sliceLen, plinthHeight, MANDAP_WIDTH), mandapPlinthGrp.transform);
            }
            // Plinth molding / border curb
            createCube("Mandap_Plinth_Curb_North", new Vector3(mandapCenterX, GROUND_Y + plinthHeight + 0.05f, MANDAP_CENTER_Z + (MANDAP_WIDTH / 2f) - 0.15f), new Vector3(MANDAP_LENGTH, 0.1f, 0.3f), mandapPlinthGrp.transform);
            createCube("Mandap_Plinth_Curb_South", new Vector3(mandapCenterX, GROUND_Y + plinthHeight + 0.05f, MANDAP_CENTER_Z - (MANDAP_WIDTH / 2f) + 0.15f), new Vector3(MANDAP_LENGTH, 0.1f, 0.3f), mandapPlinthGrp.transform);
        }

        // 2. Pillars (40 independent stone pillars: 10 rows along X × 4 columns along Z)
        GameObject mandapPillarsGrp = new GameObject("Pillars");
        mandapPillarsGrp.transform.parent = grpMandap.transform;

        float pillarCenterY = GROUND_Y + plinthHeight + (pillarHeight / 2f);
        float[] zCols = new float[] { -4.000f, -1.333f, 1.333f, 4.000f }; // 4 columns across width
        int numRows = 10;
        float rowSpacing = (MANDAP_LENGTH - 1.6f) / (numRows - 1); // evenly spaced across length

        for (int r = 0; r < numRows; r++)
        {
            float px = MANDAP_START_X + 0.8f + (r * rowSpacing);
            for (int c = 0; c < 4; c++)
            {
                float pz = zCols[c];
                // Pillar shaft
                createCube($"Pillar_R{(r + 1):D2}_C{(c + 1):D2}", new Vector3(px, pillarCenterY, pz), new Vector3(0.45f, pillarHeight, 0.45f), mandapPillarsGrp.transform);
                // Pillar capital / bracket
                createCube($"Pillar_Capital_R{(r + 1):D2}_C{(c + 1):D2}", new Vector3(px, GROUND_Y + plinthHeight + pillarHeight + 0.075f, pz), new Vector3(0.70f, 0.15f, 0.70f), mandapPillarsGrp.transform);
            }
        }

        // 3. Beams (Front, Rear, Side, and Intermediate Architrave Beams)
        GameObject mandapBeamsGrp = new GameObject("Beams");
        mandapBeamsGrp.transform.parent = grpMandap.transform;

        float beamCenterY = GROUND_Y + plinthHeight + pillarHeight + 0.15f + (beamHeight / 2f);
        {
            // 4 Longitudinal Beams running along X atop the 4 pillar columns
            for (int c = 0; c < 4; c++)
            {
                float pz = zCols[c];
                string beamName = (c == 0) ? "Side_Beam_South" : (c == 3) ? "Side_Beam_North" : $"Longitudinal_Beam_C{c + 1}";
                createCube(beamName, new Vector3(mandapCenterX, beamCenterY, pz), new Vector3(MANDAP_LENGTH, beamHeight, 0.50f), mandapBeamsGrp.transform);
            }

            // 10 Transverse Cross Beams running along Z atop each pillar row
            for (int r = 0; r < numRows; r++)
            {
                float px = MANDAP_START_X + 0.8f + (r * rowSpacing);
                string beamName = (r == 0) ? "Rear_Beam_West" : (r == numRows - 1) ? "Front_Beam_East" : $"Transverse_Beam_R{r + 1}";
                createCube(beamName, new Vector3(px, beamCenterY, MANDAP_CENTER_Z), new Vector3(0.50f, beamHeight, MANDAP_WIDTH), mandapBeamsGrp.transform);
            }
        }

        // 4. Roof Structure & Roof Border Details
        GameObject mandapRoofGrp = new GameObject("Roof_Structure");
        mandapRoofGrp.transform.parent = grpMandap.transform;

        float roofCenterY = GROUND_Y + plinthHeight + pillarHeight + 0.15f + beamHeight + (roofHeight / 2f);
        {
            // Modular flat stone roof slabs covering entire 22.860 × 9.449 m
            int roofSlabs = 4;
            float slabLen = MANDAP_LENGTH / roofSlabs;
            for (int i = 0; i < roofSlabs; i++)
            {
                float rx = MANDAP_START_X + (i + 0.5f) * slabLen;
                createCube($"Mandap_Roof_Slab_{i + 1}", new Vector3(rx, roofCenterY, MANDAP_CENTER_Z), new Vector3(slabLen, roofHeight, MANDAP_WIDTH), mandapRoofGrp.transform);
            }

            // Roof border details / Projecting Cornice (Chhajja / eaves overhang)
            float eavesY = GROUND_Y + MANDAP_HEIGHT;
            createCube("Mandap_Cornice_North", new Vector3(mandapCenterX, eavesY, MANDAP_CENTER_Z + (MANDAP_WIDTH / 2f) + 0.25f), new Vector3(MANDAP_LENGTH + 0.8f, 0.15f, 0.5f), mandapRoofGrp.transform);
            createCube("Mandap_Cornice_South", new Vector3(mandapCenterX, eavesY, MANDAP_CENTER_Z - (MANDAP_WIDTH / 2f) - 0.25f), new Vector3(MANDAP_LENGTH + 0.8f, 0.15f, 0.5f), mandapRoofGrp.transform);
            createCube("Mandap_Cornice_East", new Vector3(MANDAP_START_X + MANDAP_LENGTH + 0.25f, eavesY, MANDAP_CENTER_Z), new Vector3(0.5f, 0.15f, MANDAP_WIDTH + 0.8f), mandapRoofGrp.transform);
            createCube("Mandap_Cornice_West", new Vector3(MANDAP_START_X - 0.25f, eavesY, MANDAP_CENTER_Z), new Vector3(0.5f, 0.15f, MANDAP_WIDTH + 0.8f), mandapRoofGrp.transform);

            // Parapet stepped crest blocks along perimeter
            createCube("Mandap_Parapet_East", new Vector3(MANDAP_START_X + MANDAP_LENGTH - 0.2f, eavesY + 0.25f, MANDAP_CENTER_Z), new Vector3(0.4f, 0.35f, MANDAP_WIDTH), mandapRoofGrp.transform);
            createCube("Mandap_Parapet_West", new Vector3(MANDAP_START_X + 0.2f, eavesY + 0.25f, MANDAP_CENTER_Z), new Vector3(0.4f, 0.35f, MANDAP_WIDTH), mandapRoofGrp.transform);
        }

        // Note: The front-facing side (at X = 36.26m) has 4 pillars at Z = -4.0, -1.33, +1.33, +4.0.
        // Between them are THREE CLEAR, FULLY OPEN BAYS (Openings):
        // Bay 1: Z = [-4.0 to -1.33] (South Bay)
        // Bay 2: Z = [-1.33 to +1.33] (Center Royal Bay)
        // Bay 3: Z = [+1.33 to +4.0] (North Bay)
        // NO solid front wall exists, preserving the 3 clearly visible openings!

        // =========================================================================
        // GROUP 04 — COURTYARD
        // =========================================================================
        GameObject grpCourtyard = new GameObject("COURTYARD");
        grpCourtyard.transform.parent = root.transform;

        float pavingThickness = 0.04f;
        float pavingY = GROUND_Y + (pavingThickness / 2f);

        // 1. North Courtyard Paving (Between temple/mandap and North corridor)
        // X from -34.0 to +34.0, Z from +5.2 to +13.0 (width in Z = 7.8m)
        GameObject northCourtGrp = new GameObject("North_Courtyard_Zone");
        northCourtGrp.transform.parent = grpCourtyard.transform;
        {
            int xDivs = 10;
            float slabLen = 68.0f / xDivs; // 6.8m each
            for (int i = 0; i < xDivs; i++)
            {
                float cx = -34.0f + (i + 0.5f) * slabLen;
                createCube($"Paving_North_Zone_{i + 1}", new Vector3(cx, pavingY, 9.1f), new Vector3(slabLen - 0.05f, pavingThickness, 7.6f), northCourtGrp.transform);
            }
        }

        // 2. South Courtyard Paving (Between temple/mandap and South corridor)
        // X from -34.0 to +34.0, Z from -13.0 to -5.2 (width in Z = 7.8m)
        GameObject southCourtGrp = new GameObject("South_Courtyard_Zone");
        southCourtGrp.transform.parent = grpCourtyard.transform;
        {
            int xDivs = 10;
            float slabLen = 68.0f / xDivs;
            for (int i = 0; i < xDivs; i++)
            {
                float cx = -34.0f + (i + 0.5f) * slabLen;
                createCube($"Paving_South_Zone_{i + 1}", new Vector3(cx, pavingY, -9.1f), new Vector3(slabLen - 0.05f, pavingThickness, 7.6f), southCourtGrp.transform);
            }
        }

        // 3. West Courtyard Paving (Behind main temple)
        // X from -34.0 to -13.5, Z from -5.0 to +5.0 (width in Z = 10.0m, length in X = 20.5m)
        GameObject westCourtGrp = new GameObject("West_Courtyard_Zone");
        westCourtGrp.transform.parent = grpCourtyard.transform;
        {
            int xDivs = 4;
            float slabLen = 20.5f / xDivs;
            for (int i = 0; i < xDivs; i++)
            {
                float cx = -34.0f + (i + 0.5f) * slabLen;
                createCube($"Paving_West_Zone_{i + 1}", new Vector3(cx, pavingY, 0f), new Vector3(slabLen - 0.05f, pavingThickness, 9.8f), westCourtGrp.transform);
            }
        }

        // 4. East Gateway Courtyard Plaza (Between Sabha Mandap front and East Gateway)
        // X from 36.3m to 37.0m, Z from -4.5 to +4.5
        GameObject eastCourtGrp = new GameObject("East_Courtyard_Zone");
        eastCourtGrp.transform.parent = grpCourtyard.transform;
        {
            createCube("Paving_East_Threshold_1", new Vector3(36.65f, pavingY, -2.5f), new Vector3(0.65f, pavingThickness, 4.0f), eastCourtGrp.transform);
            createCube("Paving_East_Threshold_2", new Vector3(36.65f, pavingY, 2.5f), new Vector3(0.65f, pavingThickness, 4.0f), eastCourtGrp.transform);
        }

        // =========================================================================
        // GROUP 05 — PILLARED_CORRIDORS
        // =========================================================================
        GameObject grpCorridors = new GameObject("PILLARED_CORRIDORS");
        grpCorridors.transform.parent = root.transform;

        float corrPlatHeight = 0.35f;
        float corrColHeight = 3.00f;
        float corrBeamHeight = 0.30f;
        float corrDepth = 2.2f;

        // North Corridor (along inner North wall: Z from 13.5 to 15.7)
        // Broken at gate (X = -4.0 to +4.0)
        GameObject northCorrGrp = new GameObject("North_Corridor");
        northCorrGrp.transform.parent = grpCorridors.transform;
        {
            float corrZ = COMPOUND_Z_MAX - (WALL_THICKNESS + (corrDepth / 2f));
            float colZ = corrZ - (corrDepth / 2f) + 0.25f;

            // West wing: X from -36.5 to -4.5 (length = 32.0m)
            createCube("North_Corr_West_Platform", new Vector3(-20.5f, GROUND_Y + (corrPlatHeight / 2f), corrZ), new Vector3(32.0f, corrPlatHeight, corrDepth), northCorrGrp.transform);
            createCube("North_Corr_West_Beam", new Vector3(-20.5f, GROUND_Y + corrPlatHeight + corrColHeight + (corrBeamHeight / 2f), colZ), new Vector3(32.0f, corrBeamHeight, 0.45f), northCorrGrp.transform);
            createCube("North_Corr_West_Roof", new Vector3(-20.5f, GROUND_Y + corrPlatHeight + corrColHeight + corrBeamHeight + 0.1f, corrZ), new Vector3(32.0f, 0.2f, corrDepth), northCorrGrp.transform);

            // Columns along West wing (10 columns spaced ~3.2m)
            for (int i = 0; i < 10; i++)
            {
                float colX = -35.0f + (i * 3.4f);
                createCube($"North_Corr_West_Col_{i + 1}", new Vector3(colX, GROUND_Y + corrPlatHeight + (corrColHeight / 2f), colZ), new Vector3(0.4f, corrColHeight, 0.4f), northCorrGrp.transform);
            }

            // East wing: X from +4.5 to +36.5 (length = 32.0m)
            createCube("North_Corr_East_Platform", new Vector3(20.5f, GROUND_Y + (corrPlatHeight / 2f), corrZ), new Vector3(32.0f, corrPlatHeight, corrDepth), northCorrGrp.transform);
            createCube("North_Corr_East_Beam", new Vector3(20.5f, GROUND_Y + corrPlatHeight + corrColHeight + (corrBeamHeight / 2f), colZ), new Vector3(32.0f, corrBeamHeight, 0.45f), northCorrGrp.transform);
            createCube("North_Corr_East_Roof", new Vector3(20.5f, GROUND_Y + corrPlatHeight + corrColHeight + corrBeamHeight + 0.1f, corrZ), new Vector3(32.0f, 0.2f, corrDepth), northCorrGrp.transform);

            for (int i = 0; i < 10; i++)
            {
                float colX = 5.0f + (i * 3.4f);
                createCube($"North_Corr_East_Col_{i + 1}", new Vector3(colX, GROUND_Y + corrPlatHeight + (corrColHeight / 2f), colZ), new Vector3(0.4f, corrColHeight, 0.4f), northCorrGrp.transform);
            }
        }

        // South Corridor (along inner South wall: Z from -15.7 to -13.5)
        GameObject southCorrGrp = new GameObject("South_Corridor");
        southCorrGrp.transform.parent = grpCorridors.transform;
        {
            float corrZ = COMPOUND_Z_MIN + (WALL_THICKNESS + (corrDepth / 2f));
            float colZ = corrZ + (corrDepth / 2f) - 0.25f;

            // West wing: X from -36.5 to -4.5
            createCube("South_Corr_West_Platform", new Vector3(-20.5f, GROUND_Y + (corrPlatHeight / 2f), corrZ), new Vector3(32.0f, corrPlatHeight, corrDepth), southCorrGrp.transform);
            createCube("South_Corr_West_Beam", new Vector3(-20.5f, GROUND_Y + corrPlatHeight + corrColHeight + (corrBeamHeight / 2f), colZ), new Vector3(32.0f, corrBeamHeight, 0.45f), southCorrGrp.transform);
            createCube("South_Corr_West_Roof", new Vector3(-20.5f, GROUND_Y + corrPlatHeight + corrColHeight + corrBeamHeight + 0.1f, corrZ), new Vector3(32.0f, 0.2f, corrDepth), southCorrGrp.transform);

            for (int i = 0; i < 10; i++)
            {
                float colX = -35.0f + (i * 3.4f);
                createCube($"South_Corr_West_Col_{i + 1}", new Vector3(colX, GROUND_Y + corrPlatHeight + (corrColHeight / 2f), colZ), new Vector3(0.4f, corrColHeight, 0.4f), southCorrGrp.transform);
            }

            // East wing: X from +4.5 to +36.5
            createCube("South_Corr_East_Platform", new Vector3(20.5f, GROUND_Y + (corrPlatHeight / 2f), corrZ), new Vector3(32.0f, corrPlatHeight, corrDepth), southCorrGrp.transform);
            createCube("South_Corr_East_Beam", new Vector3(20.5f, GROUND_Y + corrPlatHeight + corrColHeight + (corrBeamHeight / 2f), colZ), new Vector3(32.0f, corrBeamHeight, 0.45f), southCorrGrp.transform);
            createCube("South_Corr_East_Roof", new Vector3(20.5f, GROUND_Y + corrPlatHeight + corrColHeight + corrBeamHeight + 0.1f, corrZ), new Vector3(32.0f, 0.2f, corrDepth), southCorrGrp.transform);

            for (int i = 0; i < 10; i++)
            {
                float colX = 5.0f + (i * 3.4f);
                createCube($"South_Corr_East_Col_{i + 1}", new Vector3(colX, GROUND_Y + corrPlatHeight + (corrColHeight / 2f), colZ), new Vector3(0.4f, corrColHeight, 0.4f), southCorrGrp.transform);
            }
        }

        // West Corridor (along inner West wall: X from -36.7 to -34.5)
        GameObject westCorrGrp = new GameObject("West_Corridor");
        westCorrGrp.transform.parent = grpCorridors.transform;
        {
            float corrX = COMPOUND_X_MIN + (WALL_THICKNESS + (corrDepth / 2f));
            float colX = corrX + (corrDepth / 2f) - 0.25f;

            // South wing: Z from -13.5 to -3.5 (length = 10.0m)
            createCube("West_Corr_South_Platform", new Vector3(corrX, GROUND_Y + (corrPlatHeight / 2f), -8.5f), new Vector3(corrDepth, corrPlatHeight, 10.0f), westCorrGrp.transform);
            createCube("West_Corr_South_Beam", new Vector3(colX, GROUND_Y + corrPlatHeight + corrColHeight + (corrBeamHeight / 2f), -8.5f), new Vector3(0.45f, corrBeamHeight, 10.0f), westCorrGrp.transform);
            createCube("West_Corr_South_Roof", new Vector3(corrX, GROUND_Y + corrPlatHeight + corrColHeight + corrBeamHeight + 0.1f, -8.5f), new Vector3(corrDepth, 0.2f, 10.0f), westCorrGrp.transform);

            for (int i = 0; i < 4; i++)
            {
                float colZ = -12.0f + (i * 2.5f);
                createCube($"West_Corr_South_Col_{i + 1}", new Vector3(colX, GROUND_Y + corrPlatHeight + (corrColHeight / 2f), colZ), new Vector3(0.4f, corrColHeight, 0.4f), westCorrGrp.transform);
            }

            // North wing: Z from +3.5 to +13.5 (length = 10.0m)
            createCube("West_Corr_North_Platform", new Vector3(corrX, GROUND_Y + (corrPlatHeight / 2f), 8.5f), new Vector3(corrDepth, corrPlatHeight, 10.0f), westCorrGrp.transform);
            createCube("West_Corr_North_Beam", new Vector3(colX, GROUND_Y + corrPlatHeight + corrColHeight + (corrBeamHeight / 2f), 8.5f), new Vector3(0.45f, corrBeamHeight, 10.0f), westCorrGrp.transform);
            createCube("West_Corr_North_Roof", new Vector3(corrX, GROUND_Y + corrPlatHeight + corrColHeight + corrBeamHeight + 0.1f, 8.5f), new Vector3(corrDepth, 0.2f, 10.0f), westCorrGrp.transform);

            for (int i = 0; i < 4; i++)
            {
                float colZ = 4.5f + (i * 2.5f);
                createCube($"West_Corr_North_Col_{i + 1}", new Vector3(colX, GROUND_Y + corrPlatHeight + (corrColHeight / 2f), colZ), new Vector3(0.4f, corrColHeight, 0.4f), westCorrGrp.transform);
            }
        }

        // =========================================================================
        // GROUP 06 — SMALL_SHRINES_AND_STRUCTURES
        // =========================================================================
        GameObject grpShrines = new GameObject("SMALL_SHRINES_AND_STRUCTURES");
        grpShrines.transform.parent = root.transform;

        // 1. Hanuman Shrine (In front courtyard precinct, facing temple)
        GameObject hanumanShrine = new GameObject("Hanuman_Shrine");
        hanumanShrine.transform.parent = grpShrines.transform;
        {
            Vector3 center = new Vector3(25.0f, 0f, 7.5f);
            // Stepped Plinth Base
            createCube("Hanuman_Plinth_Lower", new Vector3(center.x, GROUND_Y + 0.2f, center.z), new Vector3(3.0f, 0.4f, 3.0f), hanumanShrine.transform);
            createCube("Hanuman_Plinth_Upper", new Vector3(center.x, GROUND_Y + 0.5f, center.z), new Vector3(2.4f, 0.2f, 2.4f), hanumanShrine.transform);
            // 4 Corner Pillars
            float[] offsets = new float[] { -0.9f, 0.9f };
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    createCube($"Hanuman_Pillar_{i}_{j}", new Vector3(center.x + offsets[i], GROUND_Y + 1.6f, center.z + offsets[j]), new Vector3(0.3f, 2.0f, 0.3f), hanumanShrine.transform);
                }
            }
            // Inner Sanctum Cella box
            createCube("Hanuman_Cella", new Vector3(center.x, GROUND_Y + 1.5f, center.z), new Vector3(1.2f, 1.8f, 1.2f), hanumanShrine.transform);
            // Lintel Architrave
            createCube("Hanuman_Architrave", new Vector3(center.x, GROUND_Y + 2.7f, center.z), new Vector3(2.5f, 0.2f, 2.5f), hanumanShrine.transform);
            // Stepped Pyramidal Shikhara Roof
            createCube("Hanuman_Roof_Tier1", new Vector3(center.x, GROUND_Y + 2.95f, center.z), new Vector3(2.2f, 0.3f, 2.2f), hanumanShrine.transform);
            createCube("Hanuman_Roof_Tier2", new Vector3(center.x, GROUND_Y + 3.3f, center.z), new Vector3(1.6f, 0.4f, 1.6f), hanumanShrine.transform);
            createCube("Hanuman_Roof_Finial", new Vector3(center.x, GROUND_Y + 3.65f, center.z), new Vector3(0.6f, 0.3f, 0.6f), hanumanShrine.transform);
        }

        // 2. Ganapati Shrine (North-West courtyard)
        GameObject ganapatiShrine = new GameObject("Ganapati_Shrine_NW");
        ganapatiShrine.transform.parent = grpShrines.transform;
        {
            Vector3 center = new Vector3(-22.0f, 0f, 8.5f);
            createCube("Ganapati_Plinth", new Vector3(center.x, GROUND_Y + 0.25f, center.z), new Vector3(2.4f, 0.5f, 2.4f), ganapatiShrine.transform);
            createCube("Ganapati_Sanctum", new Vector3(center.x, GROUND_Y + 1.35f, center.z), new Vector3(1.5f, 1.7f, 1.5f), ganapatiShrine.transform);
            createCube("Ganapati_Roof_Tier1", new Vector3(center.x, GROUND_Y + 2.4f, center.z), new Vector3(1.8f, 0.4f, 1.8f), ganapatiShrine.transform);
            createCube("Ganapati_Roof_Spire", new Vector3(center.x, GROUND_Y + 2.85f, center.z), new Vector3(1.0f, 0.5f, 1.0f), ganapatiShrine.transform);
        }

        // 3. Surya / Shiva Shrine (South-West courtyard)
        GameObject suryaShrine = new GameObject("Surya_Shiva_Shrine_SW");
        suryaShrine.transform.parent = grpShrines.transform;
        {
            Vector3 center = new Vector3(-22.0f, 0f, -8.5f);
            createCube("Surya_Plinth", new Vector3(center.x, GROUND_Y + 0.25f, center.z), new Vector3(2.4f, 0.5f, 2.4f), suryaShrine.transform);
            createCube("Surya_Sanctum", new Vector3(center.x, GROUND_Y + 1.35f, center.z), new Vector3(1.5f, 1.7f, 1.5f), suryaShrine.transform);
            createCube("Surya_Roof_Tier1", new Vector3(center.x, GROUND_Y + 2.4f, center.z), new Vector3(1.8f, 0.4f, 1.8f), suryaShrine.transform);
            createCube("Surya_Roof_Spire", new Vector3(center.x, GROUND_Y + 2.85f, center.z), new Vector3(1.0f, 0.5f, 1.0f), suryaShrine.transform);
        }

        // 4. Deepstambha (Lamp Tower Pedestals)
        GameObject deepstambhaGrp = new GameObject("Deepstambha_Platforms");
        deepstambhaGrp.transform.parent = grpShrines.transform;
        {
            // North Deepstambha
            Vector3 d1 = new Vector3(18.0f, 0f, 8.0f);
            createCube("Deepstambha_North_Base", new Vector3(d1.x, GROUND_Y + 0.3f, d1.z), new Vector3(1.6f, 0.6f, 1.6f), deepstambhaGrp.transform);
            createCube("Deepstambha_North_Shaft", new Vector3(d1.x, GROUND_Y + 1.6f, d1.z), new Vector3(1.0f, 2.0f, 1.0f), deepstambhaGrp.transform);
            createCube("Deepstambha_North_Cap", new Vector3(d1.x, GROUND_Y + 2.8f, d1.z), new Vector3(1.4f, 0.4f, 1.4f), deepstambhaGrp.transform);

            // South Deepstambha
            Vector3 d2 = new Vector3(18.0f, 0f, -8.0f);
            createCube("Deepstambha_South_Base", new Vector3(d2.x, GROUND_Y + 0.3f, d2.z), new Vector3(1.6f, 0.6f, 1.6f), deepstambhaGrp.transform);
            createCube("Deepstambha_South_Shaft", new Vector3(d2.x, GROUND_Y + 1.6f, d2.z), new Vector3(1.0f, 2.0f, 1.0f), deepstambhaGrp.transform);
            createCube("Deepstambha_South_Cap", new Vector3(d2.x, GROUND_Y + 2.8f, d2.z), new Vector3(1.4f, 0.4f, 1.4f), deepstambhaGrp.transform);
        }

        // 5. Tulsi Vrindavan Altar
        GameObject tulsiAltar = new GameObject("Tulsi_Vrindavan");
        tulsiAltar.transform.parent = grpShrines.transform;
        {
            Vector3 center = new Vector3(25.0f, 0f, -7.5f);
            createCube("Tulsi_Base", new Vector3(center.x, GROUND_Y + 0.2f, center.z), new Vector3(1.5f, 0.4f, 1.5f), tulsiAltar.transform);
            createCube("Tulsi_Pedestal", new Vector3(center.x, GROUND_Y + 0.8f, center.z), new Vector3(1.0f, 0.8f, 1.0f), tulsiAltar.transform);
            createCube("Tulsi_Pot", new Vector3(center.x, GROUND_Y + 1.4f, center.z), new Vector3(0.8f, 0.4f, 0.8f), tulsiAltar.transform);
        }

        // =========================================================================
        // GROUP 07 — STEPS_AND_PLATFORMS
        // =========================================================================
        GameObject grpSteps = new GameObject("STEPS_AND_PLATFORMS");
        grpSteps.transform.parent = root.transform;

        // 1. Sabha Mandap Front Steps (at the 3 front openings, East side at X = 36.26m)
        GameObject mandapStepsGrp = new GameObject("Sabha_Mandap_Steps");
        mandapStepsGrp.transform.parent = grpSteps.transform;
        {
            float stepStartX = MANDAP_START_X + MANDAP_LENGTH;
            // 3 flights of steps corresponding to the 3 front openings:
            float[] frontBayZ = new float[] { -2.666f, 0.000f, 2.666f };
            string[] bayNames = new string[] { "South_Bay", "Center_Bay", "North_Bay" };

            for (int b = 0; b < 3; b++)
            {
                float bz = frontBayZ[b];
                // 3 stepped treads leading up from Y=0 to 0.50m
                createCube($"Mandap_Step_1_{bayNames[b]}", new Vector3(stepStartX + 0.5f, GROUND_Y + 0.08f, bz), new Vector3(0.35f, 0.16f, 2.0f), mandapStepsGrp.transform);
                createCube($"Mandap_Step_2_{bayNames[b]}", new Vector3(stepStartX + 0.3f, GROUND_Y + 0.24f, bz), new Vector3(0.35f, 0.32f, 2.0f), mandapStepsGrp.transform);
                createCube($"Mandap_Step_3_{bayNames[b]}", new Vector3(stepStartX + 0.1f, GROUND_Y + 0.40f, bz), new Vector3(0.35f, 0.48f, 2.0f), mandapStepsGrp.transform);
            }

            // Connection platform between Sabha Mandap and Main Temple entrance porch
            // Bridging X = 12.88 to 13.40 (gap of 0.52m)
            createCube("Temple_Mandap_Connection_Platform", new Vector3(13.14f, GROUND_Y + 0.25f, 0f), new Vector3(0.52f, 0.50f, 3.6f), mandapStepsGrp.transform);
            createCube("Temple_Mandap_Step_North", new Vector3(13.14f, GROUND_Y + 0.12f, 2.1f), new Vector3(0.52f, 0.24f, 0.6f), mandapStepsGrp.transform);
            createCube("Temple_Mandap_Step_South", new Vector3(13.14f, GROUND_Y + 0.12f, -2.1f), new Vector3(0.52f, 0.24f, 0.6f), mandapStepsGrp.transform);
        }

        // 2. Gateway Approach Steps
        GameObject gatewayStepsGrp = new GameObject("Gateway_Steps");
        gatewayStepsGrp.transform.parent = grpSteps.transform;
        {
            // East Gateway Steps (outside & inside threshold)
            createCube("East_Gate_Step_Inner", new Vector3(COMPOUND_X_MAX - 1.2f, GROUND_Y + 0.08f, 0f), new Vector3(0.6f, 0.16f, 5.0f), gatewayStepsGrp.transform);
            createCube("East_Gate_Step_Outer", new Vector3(COMPOUND_X_MAX + 0.5f, GROUND_Y + 0.08f, 0f), new Vector3(0.6f, 0.16f, 5.0f), gatewayStepsGrp.transform);

            // West Gateway Steps
            createCube("West_Gate_Step_Inner", new Vector3(COMPOUND_X_MIN + 1.2f, GROUND_Y + 0.08f, 0f), new Vector3(0.6f, 0.16f, 4.0f), gatewayStepsGrp.transform);
            createCube("West_Gate_Step_Outer", new Vector3(COMPOUND_X_MIN - 0.5f, GROUND_Y + 0.08f, 0f), new Vector3(0.6f, 0.16f, 4.0f), gatewayStepsGrp.transform);

            // North Gateway Steps
            createCube("North_Gate_Step_Inner", new Vector3(0f, GROUND_Y + 0.08f, COMPOUND_Z_MAX - 1.2f), new Vector3(5.0f, 0.16f, 0.6f), gatewayStepsGrp.transform);
            createCube("North_Gate_Step_Outer", new Vector3(0f, GROUND_Y + 0.08f, COMPOUND_Z_MAX + 0.5f), new Vector3(5.0f, 0.16f, 0.6f), gatewayStepsGrp.transform);

            // South Gateway Steps
            createCube("South_Gate_Step_Inner", new Vector3(0f, GROUND_Y + 0.08f, COMPOUND_Z_MIN + 1.2f), new Vector3(5.0f, 0.16f, 0.6f), gatewayStepsGrp.transform);
            createCube("South_Gate_Step_Outer", new Vector3(0f, GROUND_Y + 0.08f, COMPOUND_Z_MIN - 0.5f), new Vector3(5.0f, 0.16f, 0.6f), gatewayStepsGrp.transform);
        }

        // 3. Circumambulatory Walkway Platforms (Pradakshina stone platforms)
        GameObject pradakshinaGrp = new GameObject("Pradakshina_Platforms");
        pradakshinaGrp.transform.parent = grpSteps.transform;
        {
            // Raised stone stone platform around temple periphery
            createCube("Pradakshina_North", new Vector3(0f, GROUND_Y + 0.10f, 13.0f), new Vector3(26.0f, 0.20f, 1.2f), pradakshinaGrp.transform);
            createCube("Pradakshina_South", new Vector3(0f, GROUND_Y + 0.10f, -13.0f), new Vector3(26.0f, 0.20f, 1.2f), pradakshinaGrp.transform);
            createCube("Pradakshina_West", new Vector3(-13.5f, GROUND_Y + 0.10f, 0f), new Vector3(1.2f, 0.20f, 25.0f), pradakshinaGrp.transform);
        }

        // =========================================================================
        // FINAL VERIFICATION & SCENE SAVE
        // =========================================================================
        // Assert strict invariants: floor and kalaram_glb must not have moved
        bool floorUntouched = (floorObj.transform.position == floorPosBefore) &&
                              (floorObj.transform.localScale == floorScaleBefore) &&
                              (floorObj.transform.rotation == floorRotBefore);

        bool kalaramUntouched = (kalaramObj.transform.position == kalaramPosBefore) &&
                                (kalaramObj.transform.localScale == kalaramScaleBefore) &&
                                (kalaramObj.transform.rotation == kalaramRotBefore);

        if (!floorUntouched || !kalaramUntouched)
        {
            return "FATAL ERROR: Strict rule violated! floor or kalaram_glb was modified!";
        }

        // Mark scene dirty and save
        var activeScene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);
        bool saved = EditorSceneManager.SaveScene(activeScene);

        return $"SUCCESS: Built KALARAM_ENVIRONMENT with {totalCubes} primitive Cube GameObjects in 7 architectural groups!\n" +
               $"Scene saved: {saved}\n" +
               $"Floor Untouched: {floorUntouched}\n" +
               $"Kalaram Untouched: {kalaramUntouched}\n" +
               $"Root: {root.name} with child groups:\n" +
               $"- {grpWalls.name} ({grpWalls.transform.childCount} children)\n" +
               $"- {grpGateways.name} ({grpGateways.transform.childCount} children)\n" +
               $"- {grpMandap.name} ({grpMandap.transform.childCount} children, 40 pillars, 3 front openings)\n" +
               $"- {grpCourtyard.name} ({grpCourtyard.transform.childCount} children)\n" +
               $"- {grpCorridors.name} ({grpCorridors.transform.childCount} children)\n" +
               $"- {grpShrines.name} ({grpShrines.transform.childCount} children)\n" +
               $"- {grpSteps.name} ({grpSteps.transform.childCount} children)";
    }
}
