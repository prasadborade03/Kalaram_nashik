var root = GameObject.Find("KALARAM_ENVIRONMENT");
var grp = GameObject.Find("BOUNDARY_WALLS");
var nw = GameObject.Find("North_Wall");

return $"root rot: {root.transform.eulerAngles}, grp rot: {grp.transform.eulerAngles}, nw rot: {nw.transform.eulerAngles}";
