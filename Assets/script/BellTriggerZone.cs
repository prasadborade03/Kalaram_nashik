using UnityEngine;

/// <summary>
/// Trigger zone placed on the porch floor in front of the temple bell.
/// When the player steps into this area, it signals the bell to ring and swing.
/// </summary>
public class BellTriggerZone : MonoBehaviour
{
    [Tooltip("Reference to the TempleBell component")]
    public TempleBell targetBell;

    void OnTriggerEnter(Collider other)
    {
        if (targetBell != null && (other.CompareTag("Player") || other.name.Contains("Player")))
        {
            targetBell.Ring();
        }
    }
}
