using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleBehaviour : MonoBehaviour
{
    [Header("HoverSettings")]
    [SerializeField] private float hoverHeight = 0.5f; // maximum height of the hover
    [SerializeField] private float hoverSpeed = 2f; // speed of the hover

    [Header("Audio")]
    [SerializeField] private List<AudioClip> collectibleSounds;

    private AudioSource audioSource;
    private Vector3 startPosition;
    private Renderer[] renderers;
    private Rigidbody rb;
    private bool collected;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        renderers = GetComponentsInChildren<Renderer>(true);
        startPosition = transform.position;

        // moving trigger needs a kinematic rigidbody, otherwise it is treated as a static collider and trigger events arrive late
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void HoverAnimation()
    {
        float hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        rb.MovePosition(startPosition + Vector3.up * hoverOffset);
    }

    private void FixedUpdate()
    {
        HoverAnimation();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !collected)
        {
            BoxCollider coll = GetComponent<BoxCollider>();
            coll.enabled = false;
            foreach (Renderer r in renderers)
            {
                r.enabled = false;
            }
            Debug.Log("Player entered collectible trigger");
            collected = true;
            StartCoroutine(Collect());
        }
    }

    IEnumerator Collect()
    {
        GameManager.Instance.CollectibleCollected();

        if(collectibleSounds.Count > 0)
        {
            audioSource.clip = collectibleSounds[Random.Range(0, collectibleSounds.Count)];
            audioSource.Play();
        }
       

        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}
