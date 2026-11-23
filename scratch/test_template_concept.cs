var myCol = GameObject.Find("my_North_Corr_West_Col_5");
var origCol = GameObject.Find("North_Corr_West_Col_5");

var template = new GameObject("PillarTemplate");
template.transform.position = origCol.transform.position;
template.transform.rotation = Quaternion.identity;
template.transform.localScale = Vector3.one;

var srcPart1 = myCol.transform.Find("Cube.002_1");
var part1Go = UnityEngine.Object.Instantiate(srcPart1.gameObject);
part1Go.name = "Shaft_Base";
part1Go.transform.position = srcPart1.position;
part1Go.transform.rotation = srcPart1.rotation;
part1Go.transform.localScale = srcPart1.lossyScale;
part1Go.transform.SetParent(template.transform, true);

var srcPart2 = myCol.transform.Find("Cylinder_2");
var part2Go = UnityEngine.Object.Instantiate(srcPart2.gameObject);
part2Go.name = "Capital";
part2Go.transform.position = srcPart2.position;
part2Go.transform.rotation = srcPart2.rotation;
part2Go.transform.localScale = srcPart2.lossyScale;
part2Go.transform.SetParent(template.transform, true);

var rends = template.GetComponentsInChildren<Renderer>();
Bounds tb = rends[0].bounds;
for(int i=1; i<rends.Length; i++) tb.Encapsulate(rends[i].bounds);

string res = $"Template Bounds: Center={tb.center}, Size={tb.size}\n" +
             $"Orig Col Bounds: Center={origCol.GetComponent<Renderer>().bounds.center}, Size={origCol.GetComponent<Renderer>().bounds.size}\n" +
             $"Part1 localPos: {part1Go.transform.localPosition}, Part2 localPos: {part2Go.transform.localPosition}";

// Cleanup test template
UnityEngine.Object.DestroyImmediate(template);

return res;
