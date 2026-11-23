var mandap = GameObject.Find("SABHA_MANDAP");
if (mandap == null) return "Mandap not found";

var rends = mandap.GetComponentsInChildren<Renderer>();
var bounds = new Bounds(Vector3.zero, Vector3.zero);
if (rends.Length > 0) {
    bounds = rends[0].bounds;
    for(int i=1; i<rends.Length; i++) bounds.Encapsulate(rends[i].bounds);
}

return $"Mandap active: {mandap.activeInHierarchy}, Renderers: {rends.Length}, Bounds: Min={bounds.min}, Max={bounds.max}, Center={bounds.center}, Size={bounds.size}";
