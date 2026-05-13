using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollowClamp2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Background Bounds")]
    public SpriteRenderer backgroundRenderer;

    [Header("Follow Settings")]
    public float smoothSpeed = 8f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null || backgroundRenderer == null)
            return;

        Bounds bounds = backgroundRenderer.bounds;

        float cameraHeight = cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;

        float minX = bounds.min.x + cameraWidth;
        float maxX = bounds.max.x - cameraWidth;
        float minY = bounds.min.y + cameraHeight;
        float maxY = bounds.max.y - cameraHeight;

        float targetX = target.position.x;
        float targetY = target.position.y;

        float clampedX = minX > maxX ? bounds.center.x : Mathf.Clamp(targetX, minX, maxX);
        float clampedY = minY > maxY ? bounds.center.y : Mathf.Clamp(targetY, minY, maxY);

        Vector3 desiredPosition = new Vector3(clampedX, clampedY, transform.position.z);

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}