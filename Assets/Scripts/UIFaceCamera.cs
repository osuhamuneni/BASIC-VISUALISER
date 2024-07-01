using UnityEngine;

public class UIFaceCamera : MonoBehaviour
{
    private Camera _camera;
    private RectTransform rectTransform;

    private void Start()
    {
        _camera = Camera.main;
        rectTransform = GetComponent<RectTransform>();

        GetComponent<Canvas>().worldCamera = _camera;
        rectTransform.localPosition = Vector3.zero;
        rectTransform.localScale = new Vector3(0.003f, 0.003f, 1);
    }

    private void LateUpdate()
    {
        // Face the camera
        transform.rotation = _camera.transform.rotation;
    }
}
