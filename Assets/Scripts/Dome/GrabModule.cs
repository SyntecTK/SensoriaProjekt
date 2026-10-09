using UnityEngine;

public class GrabModule : MonoBehaviour
{
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private float rayDistance = 2f;
    [SerializeField] private LayerMask rayMask = ~0;

    private const string WallTag = "Wall";
    private const string PoleTag = "Pole";

    private bool isInWallrideRange;

    public void Grab(int index)
    {
        Transform origin = rayOrigin != null ? rayOrigin : transform;
        Vector3 direction = index == 0 ? -origin.right : origin.right;

        if (!Physics.Raycast(origin.position, direction, out RaycastHit hit, rayDistance, rayMask, QueryTriggerInteraction.Ignore))
            return;

        if (hit.collider.CompareTag(WallTag))
        {
            Debug.Log($"Controller {index + 1}: Wall hit.");
            EventManager.WallrideStarted();
        }
        else if (hit.collider.CompareTag(PoleTag))
        {
            Debug.Log($"Controller {index + 1}: Pole hit.");
        }
    }

    public void IsInWallrideRange(bool isInRange)
    {
        isInWallrideRange = isInRange;
    }


}
