var kalaramObj = GameObject.Find("kalaram_glb");
var mf = kalaramObj.GetComponentInChildren<MeshFilter>();
var mesh = mf.sharedMesh;
var vertices = mesh.vertices;

// Let's create an ASCII top-down map of vertex density!
// X from -20 to +20 (width 40), Z from -20 to +20 (height 40)
int W = 40, H = 40;
int[,] grid = new int[W, H];
foreach(var v in vertices) {
    Vector3 worldV = kalaramObj.transform.TransformPoint(v);
    int gx = Mathf.Clamp(Mathf.FloorToInt((worldV.x + 20f)), 0, W - 1);
    int gz = Mathf.Clamp(Mathf.FloorToInt((worldV.z + 20f)), 0, H - 1);
    grid[gx, gz]++;
}

var sb = new System.Text.StringBuilder();
sb.AppendLine("Top-down density map of kalaram_glb (Z vertical, X horizontal):");
// Print from top (Z=+20) down to bottom (Z=-20)
for(int z = H - 1; z >= 0; z--) {
    for(int x = 0; x < W; x++) {
        int count = grid[x, z];
        if (count == 0) sb.Append(" ");
        else if (count < 50) sb.Append(".");
        else if (count < 200) sb.Append("+");
        else sb.Append("#");
    }
    sb.AppendLine($" | Z={(z - 20):+00;-00}");
}
return sb.ToString();
