var kalaramObj = GameObject.Find("kalaram_glb");
var mf = kalaramObj.GetComponentInChildren<MeshFilter>();
var mesh = mf.sharedMesh;
var vertices = mesh.vertices;

// Check low points (steps) in all 4 directions
// Find vertices with Y between 0 and 1.5m
int negZSteps = 0, posZSteps = 0, negXSteps = 0, posXSteps = 0;
Vector3 minZPos = Vector3.zero, maxZPos = Vector3.zero, minXPos = Vector3.zero, maxXPos = Vector3.zero;
float minZ = 999, maxZ = -999, minX = 999, maxX = -999;

foreach(var v in vertices) {
    Vector3 w = kalaramObj.transform.TransformPoint(v);
    if (w.y >= -0.1f && w.y <= 1.5f) {
        if (w.z < minZ) { minZ = w.z; minZPos = w; }
        if (w.z > maxZ) { maxZ = w.z; maxZPos = w; }
        if (w.x < minX) { minX = w.x; minXPos = w; }
        if (w.x > maxX) { maxX = w.x; maxXPos = w; }
        
        if (w.z < -15f) negZSteps++;
        if (w.z > 15f) posZSteps++;
        if (w.x < -11f) negXSteps++;
        if (w.x > 11f) posXSteps++;
    }
}

return $"minZ step: {minZPos} (count: {negZSteps})\n" +
       $"maxZ step: {maxZPos} (count: {posZSteps})\n" +
       $"minX step: {minXPos} (count: {negXSteps})\n" +
       $"maxX step: {maxXPos} (count: {posXSteps})";
