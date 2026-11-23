var root = GameObject.Find("KALARAM_ENVIRONMENT");
if (root == null) return "null";
return $"Name: {root.name}, Pos: {root.transform.position}, Rot: {root.transform.eulerAngles}, LocalRot: {root.transform.localEulerAngles}, ChildCount: {root.transform.childCount}";
