var myCol = GameObject.Find("my_North_Corr_West_Col_5");
var sb = new System.Text.StringBuilder();

System.Action<Transform, int> Dump = null;
Dump = (t, depth) => {
    string indent = new string(' ', depth * 2);
    sb.AppendLine($"{indent}{t.name}: LocalPos={t.localPosition}, WorldPos={t.position}, LocalRot={t.localEulerAngles}, WorldRot={t.eulerAngles}, LocalScale={t.localScale}, LossyScale={t.lossyScale}");
    for(int i=0; i<t.childCount; i++) {
        Dump(t.GetChild(i), depth + 1);
    }
};

Dump(myCol.transform, 0);
return sb.ToString();
