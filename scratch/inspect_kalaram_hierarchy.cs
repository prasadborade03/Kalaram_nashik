var kalaramObj = GameObject.Find("kalaram_glb");
var sb = new System.Text.StringBuilder();

var stack = new System.Collections.Generic.Stack<Transform>();
stack.Push(kalaramObj.transform);

while(stack.Count > 0) {
    var t = stack.Pop();
    var rend = t.GetComponent<Renderer>();
    string rendInfo = rend != null ? $" [Bounds: {rend.bounds.center}, size: {rend.bounds.size}]" : "";
    sb.AppendLine($"{t.name}{rendInfo}");
    for (int i = 0; i < t.childCount; i++) {
        stack.Push(t.GetChild(i));
    }
}

return sb.ToString();
