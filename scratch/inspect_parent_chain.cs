var col = GameObject.Find("North_Corr_West_Col_5");
var sb = new System.Text.StringBuilder();

Transform curr = col.transform;
while(curr != null) {
    sb.AppendLine($"{curr.name}: Pos={curr.position}, LocalPos={curr.localPosition}, Rot={curr.eulerAngles}, LocalRot={curr.localEulerAngles}, Scale={curr.localScale}");
    curr = curr.parent;
}

return sb.ToString();
