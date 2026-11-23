var myCol = GameObject.Find("my_North_Corr_West_Col_5");
var origCol = GameObject.Find("North_Corr_West_Col_5");

var rends = myCol.GetComponentsInChildren<Renderer>();
Bounds visualBounds = rends[0].bounds;
for(int i=1; i<rends.Length; i++) visualBounds.Encapsulate(rends[i].bounds);

var origRend = origCol.GetComponent<Renderer>();

return $"Visual Bounds Center: {visualBounds.center}\n" +
       $"Visual Bounds Size: {visualBounds.size}\n" +
       $"Orig Col Position: {origCol.transform.position}\n" +
       $"Orig Col Bounds Center: {origRend.bounds.center}\n" +
       $"Orig Col Bounds Size: {origRend.bounds.size}\n" +
       $"Offset (VisualCenter - OrigColPos): {visualBounds.center - origCol.transform.position}\n" +
       $"Offset (VisualCenter - OrigBoundsCenter): {visualBounds.center - origRend.bounds.center}";
