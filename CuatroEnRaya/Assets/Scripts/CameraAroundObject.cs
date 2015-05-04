using UnityEngine;
using System.Collections;

public class CameraAroundObject : MonoBehaviour {

    public Transform target;
    public float targetVerticalOffset = 0f;
    public float targetHorizontalOffset = 0f;
    public float horizontalSpeed = 10;
    public float verticalSpeed = 10;
    public float distanceFromObject = 0.5f;

    private Vector3 computedTargetPosition;
    private Vector3 distanceVector;

	// Use this for initialization
	void Start () {
        computedTargetPosition = target.position + new Vector3(targetHorizontalOffset, targetVerticalOffset, 0f);
        distanceVector = new Vector3(0f, 0f, -distanceFromObject);
        moveCamera(0, 0);
	}
	
	void FixedUpdate () 
    {
        if (Input.GetMouseButton(1))
        {
            float deltaX = Input.GetAxis("Mouse X");
            float deltaY = Input.GetAxis("Mouse Y");

            moveCamera(deltaX, deltaY);
        }
	
	}

    void moveCamera (float deltaX, float deltaY)
    {
        //Transform angle in degree in quaternion form used by Unity for rotation.
        Quaternion rotation = Quaternion.Euler(transform.rotation.eulerAngles.x + verticalSpeed * deltaY * -1, transform.rotation.eulerAngles.y + horizontalSpeed * deltaX, 0.0f);

        // Limit vertical angle to avoid going through the floor
        if (rotation.eulerAngles.x > 90 && rotation.eulerAngles.x < 355)
        {
            Vector3 tmp = rotation.eulerAngles;
            tmp.x = 355;
            rotation.eulerAngles = tmp;
        }

        //The new position is the target position + the distance vector of the camera
        //rotated at the specified angle.
        Vector3 position = rotation * distanceVector + computedTargetPosition;

        //Update the rotation and position of the camera.
        transform.rotation = rotation;
        transform.position = position;
    }
}
