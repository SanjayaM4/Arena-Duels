using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Components;
using System.Collections;

public class PlayerMovement : NetworkBehaviour
{
    private CharacterController controller;

    public float speed = 12f;
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;
    bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!IsOwner) return; // only move the player YOU control

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            StartCoroutine(TeleportToSpawnNextFrame());
        }
    }

    private IEnumerator TeleportToSpawnNextFrame()
    {
        yield return null; // always wait at least one frame, regardless of anything else

        while (SpawnPoints.Instance == null)
        {
            yield return null; // extra safety wait for the client case, if needed
        }

        Transform spawnPoint = (OwnerClientId == 0) ? SpawnPoints.Instance.spawnPointA : SpawnPoints.Instance.spawnPointB;

        NetworkTransform netTransform = GetComponent<NetworkTransform>();
        if (netTransform != null)
        {
            netTransform.Teleport(spawnPoint.position, spawnPoint.rotation, transform.localScale);
        }

        Debug.Log("Teleported OwnerClientId " + OwnerClientId + " to " + (OwnerClientId == 0 ? "A" : "B"));
    }
}