var myCol = GameObject.Find("my_North_Corr_West_Col_5");
var origCol = GameObject.Find("North_Corr_West_Col_5");

var sb = new System.Text.StringBuilder();

System.Action<GameObject> DumpDetails = (go) => {
    if (go == null) { sb.AppendLine("NULL"); return; }
    sb.AppendLine($"Name: {go.name}, Pos: {go.transform.position}, Rot: {go.transform.eulerAngles}, LocalScale: {go.transform.localScale}");
    var rends = go.GetComponentsInChildren<Renderer>(true);
    sb.AppendLine($"Renderers count: {rends.Length}");
    Bounds b = new Bounds(go.transform.position, Vector3.zero);
    bool first = true;
    foreach(var r in rends) {
        if (first) { b = r.bounds; first = false; }
        else { b.Encapsulate(r.bounds); }
        sb.AppendLine($"  - Child: {r.name}, Pos: {r.transform.position}, Bounds: {r.bounds.size}, Mat: {(r.sharedMaterial != null ? r.sharedMaterial.name : "null")}");
    }
    sb.AppendLine($"Total Bounds Center: {b.center}, Size: {b.size}");
};

sb.AppendLine("=== MY PILLAR ===");
DumpDetails(myCol);

sb.AppendLine("=== ORIGINAL PILLAR ===");
DumpDetails(origCol);

return sb.ToString();
