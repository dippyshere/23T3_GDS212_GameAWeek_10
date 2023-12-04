using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float walkMultiplier = 1f;
    [SerializeField] private float runMultiplier = 1.3f;
    [SerializeField] private float walkVisualMultiplier = 0.4f;
    [SerializeField] private float runVisualMultiplier = 0.08f;
    [SerializeField] private float jumpForce = 15f;
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform visualTransform;
    [SerializeField] private TextMeshProUGUI animalCountText;
    [SerializeField] private ParticleSystem sandTrailParticle;

    private float groundCheckRadius = 0.3f;
    private float speed = 8;
    private bool isGrounded;
    private Rigidbody rigidBody;
    private Vector3 direction;
    private Vector3 moveDirection = Vector3.forward;
    private float horizontalInput;
    private float verticalInput;
    private bool canMove = true;
    private int savedAnimals = 0;

    void Start()
    {
        rigidBody = transform.GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        UpdateAnimalText();
    }

    void Update()
    {
        direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        animator.SetBool("Grounded", isGrounded);
        if (isGrounded)
        {
            if (sandTrailParticle.isEmitting)
            {
                sandTrailParticle.Play();
            }
        }
        else
        {
            sandTrailParticle.Stop();
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && canMove)
        {
            rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetBool("Jump", true);
        }
        if (canMove)
        {
            visualTransform.rotation = Quaternion.Slerp(visualTransform.rotation, Quaternion.LookRotation(moveDirection), Time.deltaTime * 10f);
        }
    }

    void FixedUpdate()
    {
        bool isRunning = direction.magnitude > 0.1f;

        if (isRunning && canMove)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                speed = moveSpeed * runMultiplier;
                animator.SetBool("Run", true);
                animator.SetBool("Walk", false);
            }
            else
            {
                speed = moveSpeed * walkMultiplier;
                animator.SetBool("Run", false);
                animator.SetBool("Walk", true);
            }
            Vector3 viewDir = transform.position - Camera.main.transform.position;
            viewDir.y = 0;
            orientation.forward = viewDir.normalized;
            Quaternion targetRotation = Quaternion.LookRotation(viewDir);
            orientation.rotation = targetRotation;
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
            rigidBody.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);
            
            //rigidBody.MovePosition(rigidBody.position + moveDirection * (speed * Time.fixedDeltaTime));

            animator.SetFloat("RunSpeedMult", direction.magnitude * rigidBody.velocity.magnitude * runVisualMultiplier);
            animator.SetFloat("WalkSpeedMult", direction.magnitude * rigidBody.velocity.magnitude * walkVisualMultiplier);
        }
        else
        {
            animator.SetBool("Run", false);
            animator.SetBool("Walk", false);
        }

        if (rigidBody.velocity.y <= 0)
        {
            animator.SetBool("Jump", false);
        }
    }

    private void OnDrawGizmos()
    {
        bool isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawSphere(groundCheck.position, groundCheckRadius);
    }

    private void UpdateAnimalText()
    {
        //animalCountText.text = "Saved Animals: " + savedAnimals.ToString("N0", CultureInfo.InvariantCulture);
    }

    public void AddAnimal()
    {
        savedAnimals++;
        UpdateAnimalText();
    }

    private void OnTriggerEnter(Collider other)
    {

    }
}