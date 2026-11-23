var myCol = GameObject.Find("my_North_Corr_West_Col_5");
var origCol = GameObject.Find("North_Corr_West_Col_5");

return $"myCol parent: {(myCol.transform.parent != null ? myCol.transform.parent.name : "null")}\n" +
       $"myCol pos: {myCol.transform.position}, rot: {myCol.transform.eulerAngles}, scale: {myCol.transform.localScale}\n" +
       $"origCol parent: {origCol.transform.parent.name}\n" +
       $"origCol pos: {origCol.transform.position}, rot: {origCol.transform.eulerAngles}, scale: {origCol.transform.localScale}, lossyScale: {origCol.transform.lossyScale}";
