var root = GameObject.Find("KALARAM_ENVIRONMENT");
var all = root.GetComponentsInChildren<Transform>(true);
var sb = new System.Text.StringBuilder();

int colCount = 0;
int pillarCount = 0;
var list = new System.Collections.Generic.List<string>();

foreach(var t in all) {
    if (t.name.Contains("Col_") || t.name.StartsWith("Pillar_") || t.name.Contains("_Col")) {
        list.Add($"{t.name} (Parent: {t.parent.name}, Pos: {t.position}, Scale: {t.localScale}, Rot: {t.eulerAngles})");
    }
}

sb.AppendLine($"Total matching: {list.Count}");
foreach(var item in list) {
    sb.AppendLine(item);
}

return sb.ToString();
