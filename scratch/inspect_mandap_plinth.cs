var slab = GameObject.Find("Mandap_Plinth_Slab_1");
var rend = slab.GetComponent<Renderer>();

var pillar = GameObject.Find("Pillar_R01_C01");
var pRend = pillar.GetComponent<Renderer>();

return $"Mandap Plinth Slab Bounds: Min={rend.bounds.min}, Max={rend.bounds.max}, Center={rend.bounds.center}\n" +
       $"Pillar R01 C01 Bounds: Min={pRend.bounds.min}, Max={pRend.bounds.max}, Center={pRend.bounds.center}";
