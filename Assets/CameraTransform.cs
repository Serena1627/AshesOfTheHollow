using UnityEngine;

public class CameraTransform : MonoBehaviour
{
    public GameObject targetObject;
    private Vector3 initialPositionRelativeToTarget;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (targetObject == null)
        {
            targetObject = this.gameObject;
            Debug.Log ("defaultTarget target not specified. Defaulting to parent GameObject");
        }
        initialPositionRelativeToTarget = transform.position - targetObject.transform.position;
    }
    // Update is called once per frame
    void Update()
    {
        transform.position = initialPositionRelativeToTarget + targetObject.transform.position;
        transform.LookAt(targetObject.transform);
    }
}
