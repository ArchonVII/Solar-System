using System.Diagnostics;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public enum CameraMode
    {
        FollowObject,
        FreeLook,
        TopDownSelect
    }

    public CameraMode currentMode = CameraMode.FollowObject;
    public Transform target; // The object to follow in FollowObject and TopDownSelect modes
    public Vector3 followOffset = new Vector3(0f, 2f, -5f); // Offset from the target in FollowObject mode
    public float topDownDistance = 10f; // Vertical distance above the target in TopDownSelect mode

    public float lookSensitivity = 2f;
    private float rotationX = 0f;
    private float rotationY = 0f;

    private bool topDownLocked = true; // Whether the TopDown camera is locked to the object's position

    void Start()
    {
        // Ensure the cursor is locked and invisible for free look initially
        if (currentMode == CameraMode.FreeLook)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (currentMode == CameraMode.TopDownSelect && target != null)
        {
            PositionTopDown();
        }
    }

    void LateUpdate()
    {
        if (currentMode == CameraMode.FollowObject)
        {
            if (target != null)
            {
                transform.position = target.position + followOffset;
                transform.LookAt(target);
            }
            else
            {
                UnityEngine.Debug.LogWarning("Target not assigned for FollowObject camera mode.");
            }
        }
        else if (currentMode == CameraMode.FreeLook)
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

            rotationY += mouseX;
            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -90f, 90f);

            transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
        }
        else if (currentMode == CameraMode.TopDownSelect)
        {
            if (target != null)
            {
                if (topDownLocked)
                {
                    PositionTopDown();
                }
                else
                {
                    // Free look around the top-down position
                    float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
                    float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

                    rotationY += mouseX;
                    rotationX -= mouseY;
                    rotationX = Mathf.Clamp(rotationX, -90f, 90f);

                    // Keep the camera's world position at the desired height above the target
                    Vector3 targetTopPoint = target.position + Vector3.up * topDownDistance;
                    transform.position = targetTopPoint;
                    transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
                }

                // Toggle lock/free look in TopDown mode with the 'L' key
                if (Input.GetKeyDown(KeyCode.L))
                {
                    topDownLocked = !topDownLocked;
                    if (!topDownLocked)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                    }
                    else
                    {
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        // Reset rotation when locking back
                        rotationX = 90f;
                        rotationY = 0f;
                        PositionTopDown();
                    }
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("Target not assigned for TopDownSelect camera mode.");
            }
        }

        // Toggle camera mode with the 'C' key
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleCameraMode();
        }
    }

    void ToggleCameraMode()
    {
        currentMode = (currentMode == CameraMode.FollowObject) ?
                      (target != null ? CameraMode.TopDownSelect : CameraMode.FreeLook) :
                      (currentMode == CameraMode.FreeLook ? (target != null ? CameraMode.TopDownSelect : CameraMode.FollowObject) : CameraMode.FollowObject);

        UpdateCursorState();

        if (currentMode == CameraMode.TopDownSelect && target != null)
        {
            PositionTopDown();
            rotationX = 90f;
            rotationY = 0f;
        }
    }

    void PositionTopDown()
    {
        if (target != null)
        {
            transform.position = target.position + Vector3.up * topDownDistance;
            transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Look straight down
        }
    }

    void UpdateCursorState()
    {
        if (currentMode == CameraMode.FreeLook || (currentMode == CameraMode.TopDownSelect && !topDownLocked))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Public method to switch to Follow Object mode and set the target
    public void SetFollowTarget(Transform newTarget)
    {
        currentMode = CameraMode.FollowObject;
        target = newTarget;
        if (target != null)
        {
            transform.position = target.position + followOffset;
            transform.LookAt(target);
        }
        UpdateCursorState();
    }

    // Public method to switch to Free Look mode
    public void EnableFreeLook()
    {
        currentMode = CameraMode.FreeLook;
        UpdateCursorState();
    }

    // Public method to switch to Top Down Select mode and set the target
    public void SetTopDownTarget(Transform newTarget)
    {
        currentMode = CameraMode.TopDownSelect;
        target = newTarget;
        if (target != null)
        {
            PositionTopDown();
            rotationX = 90f;
            rotationY = 0f;
        }
        UpdateCursorState();
    }

    // Public method to set the vertical distance for Top Down mode
    public void SetTopDownDistance(float distance)
    {
        topDownDistance = distance;
        if (currentMode == CameraMode.TopDownSelect && target != null && topDownLocked)
        {
            PositionTopDown();
        }
    }
}