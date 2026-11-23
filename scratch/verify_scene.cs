var root = GameObject.Find("KALARAM_ENVIRONMENT");
if (root == null) return "KALARAM_ENVIRONMENT not found";

var allTransforms = root.GetComponentsInChildren<Transform>(true);
int cubeCount = 0;
int nonCubeCount = 0;
var bounds = new Bounds(Vector3.zero, Vector3.zero);
bool first = true;

foreach (var t in allTransforms) {
    if (t == root.transform) continue;
    var mf = t.GetComponent<MeshFilter>();
    if (mf != null && mf.sharedMesh != null) {
        if (mf.sharedMesh.name == "Cube") {
            cubeCount++;
            var rend = t.GetComponent<Renderer>();
            if (first) { bounds = rend.bounds; first = false; }
            else { bounds.Encapsulate(rend.bounds); }
        } else {
            nonCubeCount++;
        }
    }
}

var floor = GameObject.Find("floor");
var kalaram = GameObject.Find("kalaram_glb");

return $"Total Cubes: {cubeCount}, Non-Cubes with mesh: {nonCubeCount}\n" +
       $"Environment Total Bounds Min: {bounds.min}, Max: {bounds.max}, Size: {bounds.size}\n" +
       $"Floor Position: {floor.transform.position}, Scale: {floor.transform.localScale}\n" +
       $"Kalaram Position: {kalaram.transform.position}, Scale: {kalaram.transform.localScale}";
