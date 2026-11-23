var t = System.Type.GetType("KalaramEnvironmentBuilder, Assembly-CSharp-Editor");
if (t == null) {
    // Search in all loaded assemblies
    foreach(var a in System.AppDomain.CurrentDomain.GetAssemblies()) {
        t = a.GetType("KalaramEnvironmentBuilder");
        if (t != null) break;
    }
}
if (t == null) return "Type KalaramEnvironmentBuilder not found in any assembly";

var method = t.GetMethod("BuildEnvironment");
if (method == null) return "BuildEnvironment method not found";

return (string)method.Invoke(null, null);
