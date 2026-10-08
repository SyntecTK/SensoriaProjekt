using System.Collections.Generic;
using UnityEngine;

public class ShelfBehaviour : MonoBehaviour
{
    [Header("Shelf Boost Settings")]
    [SerializeField] private float boostForce = 10f;
    private List<Rigidbody> shelfItems = new List<Rigidbody>();
    private bool boosted = false;

    private void Awake()
    {
        // GetComponentsInChildren also returns components on this GameObject - skip the shelf itself
        foreach(Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            if(rb.gameObject == gameObject) continue;
            shelfItems.Add(rb);
        }
    }

    private void BoostItems()
    {
        foreach(Rigidbody rb in shelfItems)
        {
            rb.isKinematic = false;
            rb.AddForce(
                rb.transform.right * boostForce * 10f + rb.transform.up * boostForce,
                ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player collided with shelf");
            if(!boosted)
            {
                BoostItems();
                boosted = true;
            }
        }
    }
}
