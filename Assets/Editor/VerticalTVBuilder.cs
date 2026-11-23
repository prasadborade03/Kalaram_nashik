using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Video;

public static class VerticalTVBuilder
{
    private const string KIOSK_NAME = "Vertical_TV_Kiosk";

    [MenuItem("Kalaram/Build Vertical TV")]
    public static void BuildTV()
    {
        // 1. Check existing TV position if user modified it
        Vector3 targetPos = new Vector3(6.058f, 2.0034f, -13.34217f);
        Quaternion targetRot = Quaternion.identity;

        GameObject existingKiosk = GameObject.Find(KIOSK_NAME);
        if (existingKiosk != null)
        {
            targetPos = existingKiosk.transform.position;
            targetRot = existingKiosk.transform.rotation;
            Undo.DestroyObjectImmediate(existingKiosk);
        }

        GameObject oldCube = GameObject.Find("Cube");
        if (oldCube != null)
        {
            if (Vector3.Distance(oldCube.transform.position, new Vector3(6.058f, 1.602f, -13.34217f)) < 1.0f)
            {
                Undo.DestroyObjectImmediate(oldCube);
            }
        }

        // 2. Load Assets
        Material matScreen = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_VerticalTVScreen.mat");
        Material matFrame = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_TVFrame.mat");
        Material matGold = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_TVGoldTrim.mat");
        Texture2D texThumb = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/texture/RamNaam_Thumbnail.png");
        RenderTexture rtVideo = AssetDatabase.LoadAssetAtPath<RenderTexture>("Assets/texture/RT_VerticalVideo.renderTexture");
        Sprite spritePlay = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/texture/UI_PlayIcon_OrangePink.png");
        Sprite spritePill = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/texture/UI_Instruction_Pill.png");

        // 3. Create Root Kiosk GameObject
        GameObject kioskRoot = new GameObject(KIOSK_NAME);
        kioskRoot.transform.position = targetPos;
        kioskRoot.transform.rotation = targetRot;
        Undo.RegisterCreatedObjectUndo(kioskRoot, "Create " + KIOSK_NAME);

        // Add components to root
        var tvScreen = kioskRoot.AddComponent<VerticalTVScreen>();
        var vp = kioskRoot.GetComponent<VideoPlayer>();
        var audioSource = kioskRoot.AddComponent<AudioSource>();

        // Physical collider covering the entire stand and TV body
        var bodyCol = kioskRoot.AddComponent<BoxCollider>();
        bodyCol.size = new Vector3(0.92f, 1.76f, 0.45f);
        bodyCol.center = new Vector3(0f, -0.06f, 0f);

        // 4. Construct TV Screen & Housing (Exact 9:16 Aspect Ratio)
        // Screen: 0.81m width x 1.44m height (0.81 / 1.44 = 9/16)
        GameObject tvBody = new GameObject("TV_Housing");
        tvBody.transform.SetParent(kioskRoot.transform, false);

        // Main Bezel / Frame
        GameObject bezel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bezel.name = "TV_Bezel";
        bezel.transform.SetParent(tvBody.transform, false);
        bezel.transform.localPosition = Vector3.zero;
        bezel.transform.localScale = new Vector3(0.89f, 1.52f, 0.08f);
        if (matFrame != null) bezel.GetComponent<Renderer>().sharedMaterial = matFrame;
        Object.DestroyImmediate(bezel.GetComponent<Collider>());

        // Gold Trim Border Rim
        GameObject goldTrim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        goldTrim.name = "Gold_Accent_Trim";
        goldTrim.transform.SetParent(tvBody.transform, false);
        goldTrim.transform.localPosition = Vector3.zero;
        goldTrim.transform.localScale = new Vector3(0.92f, 1.55f, 0.02f);
        if (matGold != null) goldTrim.GetComponent<Renderer>().sharedMaterial = matGold;
        Object.DestroyImmediate(goldTrim.GetComponent<Collider>());

        // Front Screen Quad (faces -Z towards south walkway)
        GameObject frontScreen = GameObject.CreatePrimitive(PrimitiveType.Quad);
        frontScreen.name = "Front_Screen_9x16";
        frontScreen.transform.SetParent(tvBody.transform, false);
        frontScreen.transform.localPosition = new Vector3(0f, 0f, -0.0415f);
        frontScreen.transform.localRotation = Quaternion.identity; // Normal points -Z
        frontScreen.transform.localScale = new Vector3(0.81f, 1.44f, 1.0f); // 9:16 Aspect Ratio
        if (matScreen != null) frontScreen.GetComponent<Renderer>().sharedMaterial = matScreen;
        Object.DestroyImmediate(frontScreen.GetComponent<Collider>());

        // Back Screen Quad (faces +Z towards north courtyard)
        GameObject backScreen = GameObject.CreatePrimitive(PrimitiveType.Quad);
        backScreen.name = "Back_Screen_9x16";
        backScreen.transform.SetParent(tvBody.transform, false);
        backScreen.transform.localPosition = new Vector3(0f, 0f, 0.0415f);
        backScreen.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // Normal points +Z
        backScreen.transform.localScale = new Vector3(0.81f, 1.44f, 1.0f); // 9:16 Aspect Ratio
        if (matScreen != null) backScreen.GetComponent<Renderer>().sharedMaterial = matScreen;
        Object.DestroyImmediate(backScreen.GetComponent<Collider>());

        // Ambient Screen Glow Point Light
        GameObject glowLightObj = new GameObject("Screen_Glow_Light");
        glowLightObj.transform.SetParent(tvBody.transform, false);
        glowLightObj.transform.localPosition = new Vector3(0f, 0f, -0.35f);
        var glowLight = glowLightObj.AddComponent<Light>();
        glowLight.type = LightType.Point;
        glowLight.color = new Color(1.0f, 0.72f, 0.42f);
        glowLight.range = 3.2f;
        glowLight.intensity = 0.65f;

        // 5. Stand / Pedestal Support
        GameObject standGroup = new GameObject("Stand_Support");
        standGroup.transform.SetParent(kioskRoot.transform, false);

        GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cube);
        neck.name = "Stand_Neck";
        neck.transform.SetParent(standGroup.transform, false);
        neck.transform.localPosition = new Vector3(0f, -0.78f, 0f);
        neck.transform.localScale = new Vector3(0.18f, 0.12f, 0.12f);
        if (matFrame != null) neck.GetComponent<Renderer>().sharedMaterial = matFrame;
        Object.DestroyImmediate(neck.GetComponent<Collider>());

        GameObject plinth = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plinth.name = "Base_Plinth";
        plinth.transform.SetParent(standGroup.transform, false);
        plinth.transform.localPosition = new Vector3(0f, -0.845f, 0f);
        plinth.transform.localScale = new Vector3(0.70f, 0.05f, 0.42f);
        if (matGold != null) plinth.GetComponent<Renderer>().sharedMaterial = matGold;
        Object.DestroyImmediate(plinth.GetComponent<Collider>());

        // 6. Floating VR-Style Play UI Button (Properly Scaled & Constrained)
        GameObject floatingUIObj = new GameObject("Floating_Play_Button");
        floatingUIObj.transform.SetParent(kioskRoot.transform, false);
        floatingUIObj.transform.localPosition = new Vector3(0f, 0f, -0.75f); // Floating in front of screen

        var playBtnScript = floatingUIObj.AddComponent<FloatingPlayButton>();

        // Collider for raycasting mouse / reticle
        var btnCollider = floatingUIObj.AddComponent<SphereCollider>();
        btnCollider.radius = 0.32f;
        btnCollider.isTrigger = true;

        // World Space Canvas (Scale is FIXED and strictly preserved)
        GameObject canvasObj = new GameObject("Canvas_VR");
        canvasObj.transform.SetParent(floatingUIObj.transform, false);
        canvasObj.transform.localPosition = Vector3.zero;
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = new Vector3(0.0035f, 0.0035f, 0.0035f); // ~0.55m in 3D world space

        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        var rectCanvas = canvasObj.GetComponent<RectTransform>();
        rectCanvas.sizeDelta = new Vector2(180f, 220f);

        // Internal Badge Container (What scales during animations)
        GameObject badgeContainer = new GameObject("Badge_Container");
        badgeContainer.transform.SetParent(canvasObj.transform, false);
        var rectBadge = badgeContainer.AddComponent<RectTransform>();
        rectBadge.anchorMin = new Vector2(0.5f, 0.5f);
        rectBadge.anchorMax = new Vector2(0.5f, 0.5f);
        rectBadge.pivot = new Vector2(0.5f, 0.5f);
        rectBadge.anchoredPosition = new Vector2(0f, 25f);
        rectBadge.sizeDelta = new Vector2(100f, 100f);

        // Play Badge Icon Image
        GameObject iconObj = new GameObject("Play_Icon_Badge");
        iconObj.transform.SetParent(badgeContainer.transform, false);
        var img = iconObj.AddComponent<Image>();
        img.sprite = spritePlay;
        img.color = new Color(1.0f, 0.44f, 0.22f, 0.95f);
        var rectIcon = iconObj.GetComponent<RectTransform>();
        rectIcon.anchorMin = Vector2.zero;
        rectIcon.anchorMax = Vector2.one;
        rectIcon.sizeDelta = Vector2.zero;

        // Instruction Pill (Dark rounded high-contrast backdrop)
        GameObject pillObj = new GameObject("Instruction_Pill");
        pillObj.transform.SetParent(canvasObj.transform, false);
        var pillImg = pillObj.AddComponent<Image>();
        pillImg.sprite = spritePill;
        pillImg.type = Image.Type.Sliced;
        pillImg.color = Color.white;
        var rectPill = pillObj.GetComponent<RectTransform>();
        rectPill.anchorMin = new Vector2(0.5f, 0.5f);
        rectPill.anchorMax = new Vector2(0.5f, 0.5f);
        rectPill.pivot = new Vector2(0.5f, 0.5f);
        rectPill.anchoredPosition = new Vector2(0f, -55f);
        rectPill.sizeDelta = new Vector2(165f, 48f);

        // Instruction Text (Clear, readable, high contrast)
        GameObject textObj = new GameObject("Instruction_Text");
        textObj.transform.SetParent(pillObj.transform, false);
        var txt = textObj.AddComponent<Text>();
        txt.text = "▶ CLICK TO PLAY\n<color=#FFA834>[ Left Click / E ]</color>";
        txt.fontSize = 11;
        txt.lineSpacing = 1.15f;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontStyle = FontStyle.Bold;
        txt.color = new Color(1f, 0.96f, 0.90f, 1f);
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.supportRichText = true;
        var rectText = textObj.GetComponent<RectTransform>();
        rectText.anchorMin = Vector2.zero;
        rectText.anchorMax = Vector2.one;
        rectText.sizeDelta = Vector2.zero;

        // Floating Glow Point Light (Peach-pink aesthetic, subtle range)
        GameObject floatGlowObj = new GameObject("Floating_Glow_Light");
        floatGlowObj.transform.SetParent(floatingUIObj.transform, false);
        floatGlowObj.transform.localPosition = new Vector3(0f, 0f, 0.05f);
        var floatLight = floatGlowObj.AddComponent<Light>();
        floatLight.type = LightType.Point;
        floatLight.color = new Color(1.0f, 0.65f, 0.55f);
        floatLight.range = 1.5f;
        floatLight.intensity = 0.7f;

        // Wire FloatingPlayButton fields via SerializedObject
        SerializedObject soPlayBtn = new SerializedObject(playBtnScript);
        soPlayBtn.FindProperty("tvScreen").objectReferenceValue = tvScreen;
        soPlayBtn.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
        soPlayBtn.FindProperty("badgeTransform").objectReferenceValue = rectBadge;
        soPlayBtn.FindProperty("buttonBackground").objectReferenceValue = img;
        soPlayBtn.FindProperty("instructionText").objectReferenceValue = txt;
        soPlayBtn.FindProperty("floatingGlowLight").objectReferenceValue = floatLight;
        soPlayBtn.ApplyModifiedProperties();

        // 7. Trigger Interaction Zone
        GameObject triggerObj = new GameObject("Interaction_Zone");
        triggerObj.transform.SetParent(kioskRoot.transform, false);

        var zoneScript = triggerObj.AddComponent<TVInteractionZone>();
        SerializedObject soZone = new SerializedObject(zoneScript);
        soZone.FindProperty("playButton").objectReferenceValue = playBtnScript;
        soZone.FindProperty("playButtonCollider").objectReferenceValue = btnCollider;
        soZone.FindProperty("tvScreen").objectReferenceValue = tvScreen;
        soZone.FindProperty("maxInteractDistance").floatValue = 6.5f;
        soZone.ApplyModifiedProperties();

        // 8. Wire VerticalTVScreen fields via SerializedObject
        SerializedObject soTV = new SerializedObject(tvScreen);
        soTV.FindProperty("videoStreamingFileName").stringValue = "RamNaam108.mp4";
        soTV.FindProperty("absoluteFallbackPath").stringValue = @"C:\Users\PRASAD BORADE\Downloads\Ram Naam 108 Times in 1 Minute _ राम नाम जप _ Ram Naam Jap with Hanuman Ji _ The Creator Room_1080p.mp4";
        soTV.FindProperty("videoRenderTexture").objectReferenceValue = rtVideo;
        soTV.FindProperty("thumbnailTexture").objectReferenceValue = texThumb;
        soTV.FindProperty("screenMaterial").objectReferenceValue = matScreen;
        soTV.FindProperty("screenGlowLight").objectReferenceValue = glowLight;
        soTV.FindProperty("tvAudioSource").objectReferenceValue = audioSource;
        soTV.FindProperty("playOnlyOnce").boolValue = true;

        var renderersProp = soTV.FindProperty("screenMeshRenderers");
        renderersProp.arraySize = 2;
        renderersProp.GetArrayElementAtIndex(0).objectReferenceValue = frontScreen.GetComponent<Renderer>();
        renderersProp.GetArrayElementAtIndex(1).objectReferenceValue = backScreen.GetComponent<Renderer>();
        soTV.ApplyModifiedProperties();

        // Configure VideoPlayer directly
        SerializedObject soVP = new SerializedObject(vp);
        soVP.FindProperty("m_DataSource").intValue = 1; // VideoSource.Url
        soVP.FindProperty("m_Url").stringValue = System.IO.Path.Combine(Application.streamingAssetsPath, "RamNaam108.mp4");
        soVP.FindProperty("m_TargetTexture").objectReferenceValue = rtVideo;
        soVP.FindProperty("m_RenderMode").intValue = 2; // RenderTexture
        soVP.FindProperty("m_PlayOnAwake").boolValue = false;
        soVP.FindProperty("m_Looping").boolValue = false;
        soVP.FindProperty("m_AudioOutputMode").intValue = 1; // AudioSource
        var audioSourcesProp = soVP.FindProperty("m_TargetAudioSources");
        audioSourcesProp.arraySize = 1;
        audioSourcesProp.GetArrayElementAtIndex(0).objectReferenceValue = audioSource;
        soVP.ApplyModifiedProperties();

        // 9. Ensure WalkthroughCrosshair is on Player
        var player = GameObject.FindWithTag("Player");
        if (player != null && player.GetComponent<WalkthroughCrosshair>() == null)
        {
            player.AddComponent<WalkthroughCrosshair>();
            EditorUtility.SetDirty(player);
        }

        // Mark scene dirty and save
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(kioskRoot.scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log($"[VerticalTVBuilder] Successfully created {KIOSK_NAME} at {targetPos} with clean VR UI!");
    }
}
