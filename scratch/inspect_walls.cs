var grp = GameObject.Find("BOUNDARY_WALLS");
if (grp == null) return "No BOUNDARY_WALLS";

var sb = new System.Text.StringBuilder();
foreach(Transform child in grp.transform) {
    sb.AppendLine($"Group: {child.name}");
    foreach(Transform seg in child) {
        sb.AppendLine($"  {seg.name}: pos={seg.position}, scale={seg.localScale}");
        break; // just first
    }
}
return sb.ToString();
