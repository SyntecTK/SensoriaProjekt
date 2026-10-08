using UnityEngine;

public class ItemBehaviour : MonoBehaviour
{
    [SerializeField] private float forceAmount = 10f;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            if(!rb.isKinematic)
            {
                Debug.Log("Applying force to item away from player");
                rb.AddForce((transform.position - other.transform.position).normalized * forceAmount, ForceMode.Impulse);
            }
        }
    }
}
