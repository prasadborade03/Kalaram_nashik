string path = "Assets/hdri/citrus_orchard_road_puresky_2k.exr";
UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceUpdate);
UnityEditor.AssetDatabase.ImportAsset(path, UnityEditor.ImportAssetOptions.ForceUpdate);

UnityEditor.TextureImporter importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
if (importer == null) return "Importer is null for " + path;

importer.textureType = UnityEditor.TextureImporterType.Default;
importer.textureShape = UnityEditor.TextureImporterShape.Texture2D;
importer.sRGBTexture = false;
importer.isReadable = true;
importer.mipmapEnabled = false;
importer.wrapMode = UnityEngine.TextureWrapMode.Repeat;
importer.filterMode = UnityEngine.FilterMode.Bilinear;
importer.maxTextureSize = 2048;
importer.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
importer.SaveAndReimport();

UnityEngine.Texture2D tex = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(path);
if (tex == null) return "Loaded texture is null";

var panoShader = UnityEngine.Shader.Find("Skybox/Panoramic");
var cubeShader = UnityEngine.Shader.Find("Skybox/Cubemap");

return "Tex loaded: " + tex.width + "x" + tex.height + ", format=" + tex.format + ", PanoShader=" + (panoShader != null) + ", CubeShader=" + (cubeShader != null);
