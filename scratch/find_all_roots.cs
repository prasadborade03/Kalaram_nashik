var all = GameObject.FindObjectsOfType<GameObject>();
var sb = new System.Text.StringBuilder();
foreach(var go in all) {
    if (go.name == "KALARAM_ENVIRONMENT") {
        sb.AppendLine($"Pos={go.transform.position}, Rot={go.transform.eulerAngles}, Children={go.transform.childCount}");
    }
}
return sb.ToString();
