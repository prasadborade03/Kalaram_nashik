var myCol = GameObject.Find("my_North_Corr_West_Col_5");
var sb = new System.Text.StringBuilder();

var mfs = myCol.GetComponentsInChildren<MeshFilter>();
foreach(var mf in mfs) {
    var rend = mf.GetComponent<Renderer>();
    string mat = rend != null && rend.sharedMaterial != null ? rend.sharedMaterial.name : "null";
    sb.AppendLine($"Obj: {mf.name}, Mesh: {mf.sharedMesh.name}, Verts: {mf.sharedMesh.vertexCount}, Mat: {mat}, Center: {rend.bounds.center}, Size: {rend.bounds.size}");
}

return sb.ToString();
