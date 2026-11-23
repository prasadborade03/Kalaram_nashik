var root = GameObject.Find("KALARAM_ENVIRONMENT");
return $"root parent: {(root.transform.parent != null ? root.transform.parent.name : "null")}, localRot: {root.transform.localEulerAngles}, rot: {root.transform.eulerAngles}";
