var kalaramObj = GameObject.Find("kalaram_glb");
var mf = kalaramObj.GetComponentInChildren<MeshFilter>();
var mesh = mf.sharedMesh;
var vertices = mesh.vertices;

// Let's analyze the 4 cardinal directions of the temple:
// 1. Z > 8 (North): what is the height?
// 2. Z < -10 (South): what is the height?
// 3. X > 8 (East): what is the height?
// 4. X < -8 (West): what is the height?

float maxYNorth = -999, maxYSouth = -999, maxYEast = -999, maxYWest = -999;
float minYNorth = 999, minYSouth = 999, minYEast = 999, minYWest = 999;

foreach(var v in vertices) {
    Vector3 w = kalaramObj.transform.TransformPoint(v);
    if (w.z > 8f) { if (w.y > maxYNorth) maxYNorth = w.y; if (w.y < minYNorth) minYNorth = w.y; }
    if (w.z < -10f) { if (w.y > maxYSouth) maxYSouth = w.y; if (w.y < minYSouth) minYSouth = w.y; }
    if (w.x > 8f) { if (w.y > maxYEast) maxYEast = w.y; if (w.y < minYEast) minYEast = w.y; }
    if (w.x < -8f) { if (w.y > maxYWest) maxYWest = w.y; if (w.y < minYWest) minYWest = w.y; }
}

return $"North (Z > 8): Y [{minYNorth:F2} to {maxYNorth:F2}]\n" +
       $"South (Z < -10): Y [{minYSouth:F2} to {maxYSouth:F2}]\n" +
       $"East (X > 8): Y [{minYEast:F2} to {maxYEast:F2}]\n" +
       $"West (X < -8): Y [{minYWest:F2} to {maxYWest:F2}]";
