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
            BoxCollider coll = GetComponent<BoxCollider>();
            coll.enabled = false;
            Debug.Log("Player entered collectible trigger");
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
        meshRenderer.enabled = false;

        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}
