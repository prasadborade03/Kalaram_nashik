var myCol = GameObject.Find("my_North_Corr_West_Col_5");
var origCol = GameObject.Find("North_Corr_West_Col_5");

// Let's check the local and world rotations of all components in myCol
var sb = new System.Text.StringBuilder();
foreach(Transform child in myCol.transform) {
    sb.AppendLine($"Child: {child.name}, WorldRot={child.eulerAngles}, LocalRot={child.localEulerAngles}");
    foreach(Transform grandChild in child) {
        sb.AppendLine($"  Grandchild: {grandChild.name}, WorldRot={grandChild.eulerAngles}, LocalRot={grandChild.localEulerAngles}");
    }
}
return sb.ToString();
