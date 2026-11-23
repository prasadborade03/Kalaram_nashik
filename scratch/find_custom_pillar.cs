var sceneObj = GameObject.Find("my_North_Corr_West_Col_5");
string sceneInfo = sceneObj != null 
    ? $"Found in Scene: Pos={sceneObj.transform.position}, Rot={sceneObj.transform.eulerAngles}, Scale={sceneObj.transform.localScale}, Components={string.Join(", ", System.Array.ConvertAll(sceneObj.GetComponents<Component>(), c => c.GetType().Name))}" 
    : "Not found in active scene by exact name";

// Also search in scene case-insensitively or partial match
string partialScene = "";
if (sceneObj == null) {
    var allGo = GameObject.FindObjectsOfType<GameObject>();
    foreach(var go in allGo) {
        if (go.name.ToLower().Contains("north_corr_west_col_5") || go.name.ToLower().Contains("piller") || go.name.ToLower().Contains("pillar")) {
            partialScene += $"{go.name} at {go.transform.position}; ";
        }
    }
}

// Search in AssetDatabase
string[] guids = AssetDatabase.FindAssets("my_North_Corr_West_Col_5");
string assetInfo = $"Assets found: {guids.Length}";
foreach(var g in guids) {
    assetInfo += "\n" + AssetDatabase.GUIDToAssetPath(g);
}

return $"{sceneInfo}\nPartial Scene Matches: {partialScene}\nAsset info: {assetInfo}";
