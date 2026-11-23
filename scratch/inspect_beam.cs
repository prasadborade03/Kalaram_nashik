var beam = GameObject.Find("North_Corr_West_Beam");
var rend = beam.GetComponent<Renderer>();

return $"Beam Pos: {beam.transform.position}, Bounds: Min={rend.bounds.min}, Max={rend.bounds.max}, Center={rend.bounds.center}";
