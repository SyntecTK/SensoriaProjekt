using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MenuButtonHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] Material outlineMaterial;
    [SerializeField] Color outlineColor = Color.cyan;
    [SerializeField] float thickness = 6f;
    [SerializeField] float speed = 0.5f;
    [SerializeField, Range(0.05f, 1f)] float tailLength = 0.4f;
    [SerializeField, Range(0.05f, 0.95f)] float alphaCutoff = 0.5f;   
    [SerializeField] float fadeSpeed = 10f;
    [SerializeField] Vector2 centerOffset = Vector2.zero;

    Material mat;
    Image src;
    RectTransform rect;
    float intensity, target, phase;

    void Awake()
    {
        src = GetComponent<Image>();
        rect = (RectTransform)transform;
        src.alphaHitTestMinimumThreshold = 0.1f;

        var go = new GameObject("OutlineTrail", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var img = go.AddComponent<Image>();
        img.sprite = src.sprite;
        img.preserveAspect = src.preserveAspect;
        img.raycastTarget = false;

        mat = new Material(outlineMaterial);
        img.material = mat;
    }

    void Update()
    {
        intensity = Mathf.MoveTowards(intensity, target, fadeSpeed * Time.unscaledDeltaTime);
        phase = Mathf.Repeat(phase + speed * Time.unscaledDeltaTime, 1f); 

        float texPerUnit = src.sprite != null && rect.rect.width > 0.01f
            ? src.sprite.rect.width / rect.rect.width : 1f;

        mat.SetColor("_OutlineColor", outlineColor);
        mat.SetFloat("_Thickness", thickness * texPerUnit);
        mat.SetFloat("_Tail", tailLength);
        mat.SetFloat("_AlphaCutoff", alphaCutoff);
        mat.SetFloat("_Phase", phase);

        if (src.sprite != null)
        {
            Rect tr = src.sprite.textureRect;
            Texture2D tex = src.sprite.texture;
            Vector2 center = new Vector2(
                (tr.x + tr.width * (0.5f + centerOffset.x)) / tex.width,
                (tr.y + tr.height * (0.5f + centerOffset.y)) / tex.height);
            mat.SetVector("_CenterUV", new Vector4(center.x, center.y, 0, 0));
        }

        mat.SetFloat("_Intensity", intensity);
    }

    void OnDisable() { target = 0f; intensity = 0f; }
    void OnDestroy() { if (mat) Destroy(mat); }

    public void OnPointerEnter(PointerEventData e) => target = 1f;
    public void OnPointerExit(PointerEventData e) => target = 0f;
    public void OnSelect(BaseEventData e) => target = 1f;
    public void OnDeselect(BaseEventData e) => target = 0f;
}