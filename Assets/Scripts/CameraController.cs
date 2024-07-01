using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minFOV = 10f;
    [SerializeField] private float maxFOV = 120f;

    private float initialRotationSpeed;
    private bool isMouseRightClicked = false;

    private void Start()
    {
        initialRotationSpeed = rotationSpeed;
        Cursor.visible = false; // Hide the cursor at the start
        // Confine the mouse within the screen
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Update()
    {
        // Right mouse button handling
        if (Input.GetMouseButtonDown(1))
        {
            isMouseRightClicked = !isMouseRightClicked; // Toggle the right mouse click state
            Cursor.visible = isMouseRightClicked; // Show or hide the cursor based on the right mouse click state

            if (isMouseRightClicked)
            {
                rotationSpeed = 0f; // Disable rotation when right mouse button is clicked
                return;
            }
            else
                rotationSpeed = initialRotationSpeed; // Enable rotation when right mouse button is released
        }

        // Rotate camera based on mouse movement
        if (!isMouseRightClicked)
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
            transform.RotateAround(Vector3.zero, Vector3.up, mouseX);
            transform.RotateAround(Vector3.zero, transform.right, -mouseY);
        }

        // Zoom in/out using scroll wheel
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        float currentFOV = Camera.main.fieldOfView;
        float newFOV = currentFOV - scrollWheel * zoomSpeed;
        newFOV = Mathf.Clamp(newFOV, minFOV, maxFOV);
        Camera.main.fieldOfView = newFOV;

        // Adjust rotation speed based on FOV
        float normalizedFOV = (currentFOV - minFOV) / (maxFOV - minFOV);
        rotationSpeed = initialRotationSpeed * (1f + normalizedFOV);
    }
}
