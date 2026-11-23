var myCol = GameObject.Find("my_North_Corr_West_Col_5");
var origCol = GameObject.Find("North_Corr_West_Col_5");

var myRends = myCol.GetComponentsInChildren<Renderer>();
var origRend = origCol.GetComponent<Renderer>();

var myBounds = myRends[0].bounds;
for(int i=1; i<myRends.Length; i++) myBounds.Encapsulate(myRends[i].bounds);

return $"Orig Pos: {origCol.transform.position}, Orig Bounds: center={origRend.bounds.center}, size={origRend.bounds.size}, scale={origCol.transform.localScale}, rot={origCol.transform.eulerAngles}\n" +
       $"My Pos: {myCol.transform.position}, My Bounds: center={myBounds.center}, size={myBounds.size}, scale={myCol.transform.localScale}, rot={myCol.transform.eulerAngles}\n" +
       $"Center offset (My - Orig): {myBounds.center - origRend.bounds.center}";
