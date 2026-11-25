using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] private GameObject shards;

    private void Destruct()
    {
        if (shards != null)
        {
            Instantiate(shards, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("BOTTLE COLLIDED");
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("DESTRUCTING");
            Destruct();
        }
    }
}
