using System.Collections;
using UnityEngine;

public class CollectibleBehaviour : MonoBehaviour
{
    [Header("HoverSettings")]
    [SerializeField] private float hoverHeight = 0.5f; // maximum height of the hover
    [SerializeField] private float hoverSpeed = 2f; // speed of the hover

    private AudioSource audioSource;
    private Vector3 startPosition;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        meshRenderer = GetComponent<MeshRenderer>();
        startPosition = transform.position;
    }

    private void HoverAnimation()
    {
        float hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.position = startPosition + Vector3.up * hoverOffset;
    }

    private void Update()
    {
        HoverAnimation();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("Player entered collectible trigger");
            StartCoroutine(Collect());
        }
    }

    IEnumerator Collect()
    {
        GameManager.Instance.CollectibleCollected();
        audioSource.Play();
        meshRenderer.enabled = false;

        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}
