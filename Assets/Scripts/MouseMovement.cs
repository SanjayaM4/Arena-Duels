using Unity.Netcode;
using UnityEngine;

public class MouseMovement : NetworkBehaviour
{
    public float mouseSensitivity = 500f;
    public Transform playerBody;
    public Camera playerCamera; // assign in inspector

    float xRotation = 0f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (playerCamera != null)
            {
                playerCamera.enabled = false; // disable rendering only

                AudioListener listener = playerCamera.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = false; // avoid duplicate audio listeners
            }
            enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // IsOwner is already guaranteed true here since we disabled the script otherwise
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}