var kalaramObj = GameObject.Find("kalaram_glb");
var rend = kalaramObj.GetComponentInChildren<Renderer>();
if (rend == null) return "No renderer";

var sb = new System.Text.StringBuilder();
foreach(var m in rend.sharedMaterials) {
    if (m != null) sb.AppendLine($"Mat: {m.name}, Shader: {m.shader.name}, Color: {m.color}");
}
return sb.ToString();
