using UnityEngine;

public class Rail : MonoBehaviour
{
    [Header("Runtime Variables")]
    public Vector3 railDirection, originPoint;
    [SerializeField] private bool railFlipped;
    private Transform playerTransform;
    private SimpleRailGrind railGrind;

    [Header("Settings")]
    [Tooltip("If true, the rail will rotate around its own axis. If false, the rail will stay in place.")]
    [SerializeField] private bool isPole;

    private void Awake()
    {
        railDirection = transform.forward;
        originPoint = transform.position;
    }

    public Vector3 GetRailDirection(Vector3 position = default)
    {        
        return railDirection;
    }

    private void FixedUpdate()
    {
        if (isPole)
        {
            CalculateRailDirection();
        }

        if (playerTransform != null)
        {
            if (railGrind != null) { railGrind.UpdateRail(true, true, railFlipped ? -railDirection : railDirection); }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = other.transform;
            var newRailGrind = other.gameObject.GetComponentInChildren<SimpleRailGrind>();
            railGrind = newRailGrind;

            if (isPole)
            {
                CalculateRailDirection();
            }

            railFlipped = playerTransform.InverseTransformDirection(railDirection).z < 0f;

            if (newRailGrind != null) { newRailGrind.UpdateRail(true, true, railFlipped ? -railDirection : railDirection); }
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = null;
            var newRailGrind = other.gameObject.GetComponentInChildren<SimpleRailGrind>();
            railGrind = null;

            if (newRailGrind != null) { newRailGrind.UpdateRail(false, false, railFlipped ? -railDirection : railDirection); }
        }
    }

    void CalculateRailDirection()
    {
        if (playerTransform != null)
        {
            railDirection = Vector3.Cross(originPoint - playerTransform.position, Vector3.up).normalized;

            Debug.DrawRay(originPoint, railDirection * 10f, Color.green);
        }
    }
}
