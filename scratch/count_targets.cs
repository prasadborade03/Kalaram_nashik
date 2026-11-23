var root = GameObject.Find("KALARAM_ENVIRONMENT");
var sb = new System.Text.StringBuilder();

int northCount = 0, southCount = 0, westCount = 0, mandapCount = 0;

var allT = root.GetComponentsInChildren<Transform>(true);
foreach(var t in allT) {
    if (t.name.StartsWith("North_Corr_") && t.name.Contains("_Col_")) northCount++;
    else if (t.name.StartsWith("South_Corr_") && t.name.Contains("_Col_")) southCount++;
    else if (t.name.StartsWith("West_Corr_") && t.name.Contains("_Col_")) westCount++;
    else if (t.name.StartsWith("Pillar_R") && !t.name.Contains("Capital")) mandapCount++;
}

return $"Target columns count:\n" +
       $"- North Corridor: {northCount}\n" +
       $"- South Corridor: {southCount}\n" +
       $"- West Corridor: {westCount}\n" +
       $"- Sabha Mandap: {mandapCount}\n" +
       $"Total columns to replace: {northCount + southCount + westCount + mandapCount}";
