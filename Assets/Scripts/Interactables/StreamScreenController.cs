using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class StreamScreenController : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 3.0f;
    [SerializeField] private float scaleSpeed = 2.0f;
    [SerializeField] private bool enableScaling = true;

    [Header("Input Action References")]
    [SerializeField] private InputActionReference leftJoystickAction;
    [SerializeField] private InputActionReference rightJoystickAction;

    [Header("Visual Feedback")]
    [SerializeField] private Color grabbedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    private XRGrabInteractable grabInteractable;
    private Vector3 initialScale;
    private Material screenMaterial;
    private Color originalColor;
    private bool isGrabbed = false;

    void Start()
    {
        SetupComponents();
        SetupInteractionEvents();
        SetupVisualFeedback();

        Debug.Log("StreamScreen is now grabbable and resizable!");
    }

    private void SetupComponents()
    {
        // Get or add XR Grab Interactable
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        }

        // Store initial scale
        initialScale = transform.localScale;

        // Configure grab settings
        grabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = true;
        grabInteractable.throwOnDetach = false;
    }

    private void SetupInteractionEvents()
    {
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
        grabInteractable.hoverEntered.AddListener(OnHoverEntered);
        grabInteractable.hoverExited.AddListener(OnHoverExited);
    }

    private void SetupVisualFeedback()
    {
        // Get material for visual feedback
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            screenMaterial = renderer.material;
            originalColor = screenMaterial.color;
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;

        // Visual feedback
        if (screenMaterial != null)
            screenMaterial.color = grabbedColor;

        // Debug.Log("StreamScreen grabbed! Use joystick/WASD to resize.");
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;

        // Reset visual feedback
        if (screenMaterial != null)
            screenMaterial.color = originalColor;

        // Debug.Log("StreamScreen released.");
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        // Debug.Log("StreamScreen hover entered - ready to grab!");
    }

    private void OnHoverExited(HoverExitEventArgs args)
    {
        // Debug.Log("StreamScreen hover exited.");
    }

    void Update()
    {
        if (isGrabbed && enableScaling && grabInteractable.isSelected)
        {
            HandleScaling();
        }
    }

    private void HandleScaling()
    {
        // Method 1: Use InputActionReference for joystick input
        Vector2 joystickInput = GetJoystickInput();

        if (Mathf.Abs(joystickInput.y) > 0.1f)
        {
            ScaleScreen(joystickInput.y);
            return; // Exit early if joystick input detected
        }

    }

    private Vector2 GetJoystickInput()
    {
        Vector2 totalInput = Vector2.zero;

        // Try left joystick first
        if (leftJoystickAction != null && leftJoystickAction.action != null)
        {
            Vector2 leftInput = leftJoystickAction.action.ReadValue<Vector2>();
            totalInput += leftInput;
        }

        // Try right joystick as backup
        if (rightJoystickAction != null && rightJoystickAction.action != null)
        {
            Vector2 rightInput = rightJoystickAction.action.ReadValue<Vector2>();
            totalInput += rightInput;
        }

        return totalInput;
    }

    private void ScaleScreen(float scaleDirection)
    {
        float scaleChange = scaleDirection * scaleSpeed * Time.deltaTime;
        Vector3 newScale = transform.localScale + Vector3.one * scaleChange;

        // FIXED: Scale the parent (this GameObject) instead of child
        transform.localScale = newScale;
    }


    // Public methods for testing
    public void ResetScale()
    {
        transform.localScale = initialScale;
        Debug.Log("Screen scale reset to original size");
    }

    public void SetScale(float multiplier)
    {
        multiplier = Mathf.Clamp(multiplier, minScale, maxScale);
        transform.localScale = initialScale * multiplier;
        Debug.Log($"Screen scale set to: {multiplier:F2}x original size");
    }
}
