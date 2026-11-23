var myCol = GameObject.Find("my_North_Corr_West_Col_5");
var origCol = GameObject.Find("North_Corr_West_Col_5");

// Let's check how the user created my_North_Corr_West_Col_5
// What is its prefab instance?
var prefabType = PrefabUtility.GetPrefabAssetType(myCol);
var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(myCol);

return $"PrefabType: {prefabType}, PrefabPath: '{prefabPath}'\n" +
       $"myCol Pos: {myCol.transform.position}, myCol LocalPos: {myCol.transform.localPosition}\n" +
       $"origCol Pos: {origCol.transform.position}, origCol LocalPos: {origCol.transform.localPosition}";
