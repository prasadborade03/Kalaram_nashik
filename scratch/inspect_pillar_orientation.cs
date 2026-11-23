var myCol = GameObject.Find("my_North_Corr_West_Col_5");
var mf = myCol.GetComponentInChildren<MeshFilter>();
var mesh = mf.sharedMesh;
var verts = mesh.vertices;

// Find min/max in local space of Object_4
Vector3 min = new Vector3(999,999,999);
Vector3 max = new Vector3(-999,-999,-999);
foreach(var v in verts) {
    min = Vector3.Min(min, v);
    max = Vector3.Max(max, v);
}

return $"Object_4 local bounds: min={min}, max={max}, size={max - min}\n" +
       $"Object_4 world rotation: {mf.transform.eulerAngles}\n" +
       $"Cube.002_1 world rotation: {mf.transform.parent.eulerAngles}\n" +
       $"my_North_Corr_West_Col_5 world rotation: {myCol.transform.eulerAngles}";
