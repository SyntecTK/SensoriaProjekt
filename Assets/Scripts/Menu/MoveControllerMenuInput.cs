using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MoveControllerMenuInput : MonoBehaviour
{
    public enum Face { Triangle, Circle, Cross, Square }

    [System.Serializable]
    public class Slot
    {
        public string name;
        public Face face;
        public Button button;
    }

    [Header("Move-Tasten (im Inspector per 'Listen' belegen)")]
    [SerializeField] InputAction triangle = new InputAction("Triangle", InputActionType.Button);
    [SerializeField] InputAction circle = new InputAction("Circle", InputActionType.Button);
    [SerializeField] InputAction cross = new InputAction("Cross", InputActionType.Button);
    [SerializeField] InputAction square = new InputAction("Square", InputActionType.Button);
    [SerializeField] InputAction move = new InputAction("Move", InputActionType.Button);

    [Header("Zuordnung: Taste -> Button (nur aktive Buttons werden ausgewählt)")]
    [SerializeField] Slot[] slots;

    void OnEnable()
    {
        triangle.Enable(); circle.Enable(); cross.Enable(); square.Enable(); move.Enable();
    }

    void OnDisable()
    {
        triangle.Disable(); circle.Disable(); cross.Disable(); square.Disable(); move.Disable();
    }

    void Update()
    {
        if (triangle.WasPressedThisFrame()) Select(Face.Triangle);
        if (circle.WasPressedThisFrame()) Select(Face.Circle);
        if (cross.WasPressedThisFrame()) Select(Face.Cross);
        if (square.WasPressedThisFrame()) Select(Face.Square);
        if (move.WasPressedThisFrame()) Confirm();
    }

    void Select(Face face)
    {
        var es = EventSystem.current;
        if (es == null) return;

        foreach (var s in slots)
        {
            if (s.face != face || s.button == null) continue;
            if (!s.button.gameObject.activeInHierarchy || !s.button.IsInteractable()) continue;

            es.SetSelectedGameObject(s.button.gameObject);   // löst auch den Hover-Rand aus (ISelectHandler)
            return;
        }
    }

    void Confirm()
    {
        var es = EventSystem.current;
        if (es == null) return;

        var go = es.currentSelectedGameObject;
        if (go == null || !go.activeInHierarchy) return;

        ExecuteEvents.Execute(go, new BaseEventData(es), ExecuteEvents.submitHandler);
    }
}