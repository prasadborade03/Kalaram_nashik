var origCol = GameObject.Find("North_Corr_West_Col_5");
var rend = origCol.GetComponent<Renderer>();

return $"Parent: {origCol.transform.parent.name}, Pos: {origCol.transform.position}, LocalPos: {origCol.transform.localPosition}, LocalScale: {origCol.transform.localScale}, LossyScale: {origCol.transform.lossyScale}, BoundsMin: {rend.bounds.min}, BoundsMax: {rend.bounds.max}, BoundsCenter: {rend.bounds.center}";
