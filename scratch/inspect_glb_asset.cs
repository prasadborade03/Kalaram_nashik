var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/mesh/hindu_temple_pillar.glb");
if (prefab == null) return "Prefab null";

var sb = new System.Text.StringBuilder();

System.Action<Transform, int> Dump = null;
Dump = (t, depth) => {
    string indent = new string(' ', depth * 2);
    sb.AppendLine($"{indent}{t.name}: Pos={t.localPosition}, Rot={t.localEulerAngles}, Scale={t.localScale}");
    for(int i=0; i<t.childCount; i++) Dump(t.GetChild(i), depth + 1);
};

Dump(prefab.transform, 0);
return sb.ToString();
