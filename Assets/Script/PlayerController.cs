using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    public float jumpForce = 5.0f;
    public float gravity = -9.81f;
    public float doubleJumpForce = 5.0f;

    public float dashSpeed = 10.0f;
    public float dashDistance = 3.0f;
    private float dashTimer;
    private Camera playerCamera;
    private bool isDashing = false;
    public float dashCooldown = 2.0f;
    private float lastDashTime;

    public float wallClimbSpeed = 3.0f;
    private float initialGravity;
    public float maxWallClimbDistance = 5.0f;
    private float wallClimbDistance;

    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float shootDelay = 0.3f;
    public float coneShootAngle = 20f;
    private float lastShootTime;

    private CharacterController controller;
    private Vector3 moveDirection;
    private int jumpCount = 0;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        lastDashTime = -dashCooldown;
        initialGravity = gravity;
    }

    private void Shoot()
    {
        if (Time.time >= lastShootTime + shootDelay)
        {
            StartCoroutine(ShootRapidly());
            lastShootTime = Time.time;
        }
    }

    private IEnumerator ShootRapidly()
    {
        for (int i = 0; i < 3; i++)
        {
            Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
            yield return new WaitForSeconds(shootDelay);
        }
    }

    private void ShootCone()
    {
        if (Time.time >= lastShootTime + shootDelay)
        {
            float offsetAngle = 15f;

            Vector3 forward = projectileSpawnPoint.forward;
            Vector3 leftDirection = projectileSpawnPoint.TransformDirection(Quaternion.Euler(0, -offsetAngle, 0) * Vector3.forward);
            Vector3 rightDirection = projectileSpawnPoint.TransformDirection(Quaternion.Euler(0, offsetAngle, 0) * Vector3.forward);

            Quaternion leftRotation = Quaternion.LookRotation(leftDirection);
            Quaternion rightRotation = Quaternion.LookRotation(rightDirection);

            Instantiate(projectilePrefab, projectileSpawnPoint.position, leftRotation);
            Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
            Instantiate(projectilePrefab, projectileSpawnPoint.position, rightRotation);

            lastShootTime = Time.time;
        }
    }

    private void Dash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
        {
            Vector3 dashDirection = playerCamera.transform.forward;

            float verticalLookAngle = playerCamera.transform.localEulerAngles.x;
            if (verticalLookAngle > 180f)
            {
                verticalLookAngle -= 360f;
            }

            if (isGrounded && verticalLookAngle > -90f && verticalLookAngle < 90f)
            {
                verticalLookAngle = 90f;
            }

            float verticalModifier = Mathf.Clamp(verticalLookAngle / 90f, -1f, 1f);
            dashDirection += Vector3.up * verticalModifier;
            dashDirection = dashDirection.normalized;

            moveDirection = dashDirection * dashSpeed;

            float dashDuration = dashDistance / dashSpeed;
            dashTimer = dashDuration;
            isDashing = true;

            lastDashTime = Time.time;
        }
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded || (isDashing && dashTimer <= 0))
        {
            moveDirection = new Vector3(0, moveDirection.y, 0);
            isDashing = false;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move * speed * Time.deltaTime);

        if (isGrounded)
        {
            moveDirection.y = gravity * Time.deltaTime;
            jumpCount = 0;

            if (Input.GetButtonDown("Jump"))
            {
                moveDirection.y = jumpForce;
                jumpCount++;
            }
        }
        else
        {
            if (dashTimer > 0)
            {
                dashTimer -= Time.deltaTime;
            }
            else
            {
                moveDirection.y += gravity * Time.deltaTime;
            }

            if (jumpCount < 2 && Input.GetButtonDown("Jump"))
            {
                moveDirection.y = doubleJumpForce;
                jumpCount++;
            }
        }

        if (Time.time >= lastDashTime + dashCooldown)
        {
            Dash();
        }

        controller.Move(moveDirection * Time.deltaTime);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 1.5f) && Input.GetKey(KeyCode.Space) && wallClimbDistance < maxWallClimbDistance)
        {
            gravity = 0;
            moveDirection.y = wallClimbSpeed;
            wallClimbDistance += wallClimbSpeed * Time.deltaTime;
        }
        else
        {
            gravity = initialGravity;
            if (isGrounded)
            {
                wallClimbDistance = 0;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }

        if (Input.GetMouseButtonDown(1))
        {
            ShootCone();
        }
    }
}
