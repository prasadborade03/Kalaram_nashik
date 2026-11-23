var myCol = GameObject.Find("my_North_Corr_West_Col_5");
// Let's inspect the capital Object_6
var cap = myCol.transform.Find("Cylinder_2/Object_6");
if (cap == null) return "Object_6 not found";

var rend = cap.GetComponent<Renderer>();
return $"Capital Object_6 bounds: center={rend.bounds.center}, size={rend.bounds.size}\n" +
       $"World rot: {cap.transform.eulerAngles}, Local rot: {cap.transform.localEulerAngles}";
