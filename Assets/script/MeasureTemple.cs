using UnityEngine;

public class MeasureTemple : MonoBehaviour
{
    [ContextMenu("Measure Temple Height")]
    public void MeasureHeight()
    {
        Renderer rend = GetComponent<Renderer>();

        if (rend == null)
        {
            Debug.LogError("No Renderer found on this object!");
            return;
        }

        float height = rend.bounds.size.y;

        Debug.Log("========== TEMPLE MEASUREMENT ==========");
        Debug.Log("Current Height: " + height.ToString("F3") + " meters");
        Debug.Log("Target Height: 22.86 meters (75 feet)");

        float scaleFactor = 22.86f / height;

        Debug.Log("Required Scale Multiplier: " + scaleFactor.ToString("F3"));
        Debug.Log("========================================");
    }
}