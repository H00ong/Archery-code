using UnityEngine;

public class ObjectRotation : MonoBehaviour
{
    [SerializeField] int rotationSpeed = 10;
    [SerializeField] Vector3 rotationAxis = Vector3.up;

    private void Update()
    {
        RotateObject();
    }

    private void RotateObject()
    {
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.Self);
    }
}
