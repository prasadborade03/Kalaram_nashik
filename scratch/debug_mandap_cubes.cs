var mandap = GameObject.Find("SABHA_MANDAP");
if (mandap == null) return "No SABHA_MANDAP";

var sb = new System.Text.StringBuilder();
foreach(Transform child in mandap.transform) {
    sb.AppendLine($"Child group: {child.name}");
    int count = 0;
    foreach(Transform cube in child) {
        if (count < 3) sb.AppendLine($"  {cube.name}: pos={cube.position}, scale={cube.localScale}");
        count++;
    }
}
return sb.ToString();
