var root = GameObject.Find("KALARAM_ENVIRONMENT");
if (root != null) {
    root.transform.position = Vector3.zero;
    root.transform.rotation = Quaternion.identity;
    var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene);
    return "Reset root to (0,0,0) and rotation identity, saved scene";
}
return "root null";
