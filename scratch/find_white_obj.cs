var allObjects = GameObject.FindObjectsOfType<GameObject>();
var sb = new System.Text.StringBuilder();

foreach (var go in allObjects) {
    if (go.transform.position.x > 15f && go.transform.position.z > 5f) {
        var rend = go.GetComponent<Renderer>();
        string matName = rend != null && rend.sharedMaterial != null ? rend.sharedMaterial.name : "none";
        sb.AppendLine($"{go.name}: Pos={go.transform.position}, Scale={go.transform.localScale}, Mat={matName}");
    }
}

return sb.ToString();
