using UnityEngine;

public class GalaxiesFaceCamera : MonoBehaviour
{
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Start()
    {
        FaceCamera();
    }

    public void FaceCamera()
    {
        Vector3 cameraPosition = mainCamera.transform.position;
        Vector3 objectPosition = transform.position;
        Vector3 direction = (cameraPosition - objectPosition).normalized;//direction

        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = targetRotation;
    }
}
