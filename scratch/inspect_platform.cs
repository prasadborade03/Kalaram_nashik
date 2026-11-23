var plat = GameObject.Find("North_Corr_West_Platform");
var rend = plat.GetComponent<Renderer>();

return $"Platform Pos: {plat.transform.position}, Bounds: Min={rend.bounds.min}, Max={rend.bounds.max}, Center={rend.bounds.center}";
