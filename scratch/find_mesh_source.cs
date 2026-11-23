var myCol = GameObject.Find("my_North_Corr_West_Col_5");
var mf = myCol.GetComponentInChildren<MeshFilter>();
string assetPath = AssetDatabase.GetAssetPath(mf.sharedMesh);
return $"Mesh asset path: '{assetPath}', Mesh name: '{mf.sharedMesh.name}'";
