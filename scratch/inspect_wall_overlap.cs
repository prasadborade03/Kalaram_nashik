var kalaramObj = GameObject.Find("kalaram_glb");
var mf = kalaramObj.GetComponentInChildren<MeshFilter>();
var mesh = mf.sharedMesh;
var vertices = mesh.vertices;

// Find min and max X for vertices where Z is near +16 and -16
float minXNearPosZ = 999, maxXNearPosZ = -999;
float minXNearNegZ = 999, maxXNearNegZ = -999;

foreach(var v in vertices) {
    Vector3 w = kalaramObj.transform.TransformPoint(v);
    if (w.z >= 15.8f) {
        if (w.x < minXNearPosZ) minXNearPosZ = w.x;
        if (w.x > maxXNearPosZ) maxXNearPosZ = w.x;
    }
    if (w.z <= -15.8f) {
        if (w.x < minXNearNegZ) minXNearNegZ = w.x;
        if (w.x > maxXNearNegZ) maxXNearNegZ = w.x;
    }
}

return $"At Z >= +15.8: X spans from {minXNearPosZ:F2} to {maxXNearPosZ:F2}\n" +
       $"At Z <= -15.8: X spans from {minXNearNegZ:F2} to {maxXNearNegZ:F2}";
