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
    public Animator animator;
    public float killY = -10f;

    Vector3 velocity;
    bool isGrounded;

    private bool spawnProtected = true;
    private bool isDead = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!IsOwner) return;

        if (isDead) return; // freeze completely while dead - no more gravity accumulation, no more movement

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

        float horizontalSpeed = new Vector3(move.x, 0, move.z).magnitude;
        animator.SetFloat("Speed", horizontalSpeed);
        animator.SetBool("IsGrounded", isGrounded);

        if (!spawnProtected && transform.position.y < killY)
        {
            Debug.Log("[Frame " + Time.frameCount + "] KILL CHECK FAILED. position.y=" + transform.position.y + " killY=" + killY + " velocity.y=" + velocity.y);
            isDead = true;
            RequestKillServerRpc();
        }
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
        spawnProtected = true;
        isDead = false;
        velocity = Vector3.zero;

        yield return null;

        while (SpawnPoints.Instance == null)
        {
            yield return null;
        }

        Transform spawnPoint = (OwnerClientId == 0) ? SpawnPoints.Instance.spawnPointA : SpawnPoints.Instance.spawnPointB;

        controller.enabled = false; // NEW - detach CharacterController before moving

        NetworkTransform netTransform = GetComponent<NetworkTransform>();
        if (netTransform != null)
        {
            netTransform.Teleport(spawnPoint.position, spawnPoint.rotation, transform.localScale);
        }

        controller.enabled = true; // NEW - re-attach now that position is correct

        Debug.Log("[Frame " + Time.frameCount + "] Teleport complete, position now: " + transform.position);

        yield return new WaitForSeconds(0.3f); // small buffer, not just 1 frame - covers any lingering settle time
        spawnProtected = false;
    }

    public void ResetToSpawn()
    {
        if (IsOwner)
        {
            StartCoroutine(TeleportToSpawnNextFrame());
        }
    }

    [ServerRpc]
    private void RequestKillServerRpc()
    {
        Health health = GetComponent<Health>();
        if (health != null) health.Kill();
    }
}