using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform cameraPosition;
    [SerializeField] private float speed;

    private void FixedUpdate()
    {
        cameraPosition.position = Vector3.Lerp(cameraPosition.position, transform.position, speed * Time.deltaTime);
        cameraPosition.position = new Vector3(cameraPosition.position.x, 0, cameraPosition.position.z);
    }
}
