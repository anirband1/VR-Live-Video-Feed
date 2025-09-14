using UnityEngine;
using UnityEngine.InputSystem;

public class MiniScreenManager : MonoBehaviour
{
    [Header("Mini Screen Configuration")]
    [SerializeField] private GameObject miniScreen;
    [SerializeField] private InputActionReference toggleButton;
    [SerializeField] private bool startVisible = false;

    private bool isVisible;
    private Renderer miniScreenRenderer;
    private Color originalColor;
    LiveKitConnectionManager LKCM;

    void Start()
    {
        InitializeMiniScreen();
        LKCM = FindObjectOfType<LiveKitConnectionManager>();
    }

    private void InitializeMiniScreen()
    {
        if (miniScreen != null)
        {
            // Get renderer for potential fade effects
            miniScreenRenderer = miniScreen.GetComponent<Renderer>();
            if (miniScreenRenderer != null)
            {
                originalColor = miniScreenRenderer.material.color;
            }

            // Set initial visibility
            isVisible = startVisible;
            miniScreen.SetActive(isVisible);

            Debug.Log($"Mini screen initialized - Visible: {isVisible}");
        }
        else
        {
            Debug.LogError("MiniScreen GameObject not assigned!");
        }
    }

    void Update()
    {
        CheckToggleInput();
        if (isVisible)
        {
            UpdateTexture();
        }
    }

    private void CheckToggleInput()
    {
        // Check for A button input
        if (toggleButton != null && toggleButton.action != null)
        {
            if (toggleButton.action.WasPressedThisFrame()) ToggleMiniScreen();
        }

    }

    private void ToggleMiniScreen()
    {
        if (miniScreen != null)
        {
            isVisible = !isVisible;

            miniScreen.SetActive(isVisible);
        }
    }

    private void UpdateTexture()
    {
        if (LKCM.streamScreenRenderer.material.mainTexture != null)
        {
            miniScreenRenderer.material.mainTexture = LKCM.streamScreenRenderer.material.mainTexture;
        }
    }
}
