using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoardCollisionHandler : MonoBehaviour, IReferenceRigidbody
{

    [Header("Settings")]
    [SerializeField] private float normalDamping = 0.2f;        // Bounce-Damping
    [SerializeField] private float tangentialPreserve = 1.0f;   // Speed-Conservation along the Surface
    [SerializeField] private float angularImpactDamping = 0.4f; // Rotation-kill on Impact
    [SerializeField] private float minImpactSpeed = 1.5f;


    [Header("Components")]
    public Rigidbody PhysicsRigidbody { get; set; }

    private AudioSource audioSource;


    void Awake()
    {

        PhysicsRigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter(Collision c)
    {
        audioSource.Play();
        HandleCollision(c, true);
    }

    void OnCollisionStay(Collision c)
    {
        HandleCollision(c, false);
    }

    void HandleCollision(Collision c, bool impact)
    {
        //if (!c.gameObject.CompareTag("Ground")) return;

        ContactPoint cp = c.GetContact(0);
        Vector3 n = cp.normal;

        Vector3 v = PhysicsRigidbody.linearVelocity;

        float intoSurface = Vector3.Dot(v, -n);
        if (intoSurface < minImpactSpeed) return;

        // Velocity zerlegen
        Vector3 normalVel = Vector3.Project(v, n);
        Vector3 tangentVel = v - normalVel;

        // Normalen-Anteil stark d�mpfen (kein Abprallen)
        normalVel *= normalDamping;

        // Tangentialen Anteil behalten (Gleiten)
        tangentVel *= tangentialPreserve;

        PhysicsRigidbody.linearVelocity = tangentVel + normalVel;

        // Rotationsd�mpfung nur bei Impact
        if (impact)
        {
            Vector3 av = PhysicsRigidbody.angularVelocity;
            av.x *= angularImpactDamping;
            av.z *= angularImpactDamping;
            PhysicsRigidbody.angularVelocity = av;
        }
    }
}
