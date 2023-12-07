using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackRate = 0.5f;
    [SerializeField] private float waterDrainRate = 5f;
    [SerializeField] private float waterDrainRateRun = 7f;
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform visualTransform;
    [SerializeField] private ParticleSystem sandTrailParticle;
    [Header("UI References")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image waterBar;
    [SerializeField] private Image goldBar;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI gemsText;
    [SerializeField] private List<TextMeshProUGUI> waterTexts = new List<TextMeshProUGUI>();
    [SerializeField] private TextMeshProUGUI goldGoalText;
    [SerializeField] private TextMeshProUGUI conveienceGoldWaterText;
    [SerializeField] private TextMeshProUGUI conveienceGoldGemText;
    [SerializeField] private TextMeshProUGUI convenienceGemText;
    [SerializeField] private TextMeshProUGUI replenishGoldText;
    [SerializeField] private TextMeshProUGUI tradeGemsText;
    [SerializeField] private TextMeshProUGUI tradeGoldText;
    [SerializeField] private GameObject tradeGemUI;
    [SerializeField] private GameObject replenishUI;
    [SerializeField] private GameObject convenienceUI;
    [SerializeField] private GameObject inventoryUI;

    private float groundCheckRadius = 0.3f;
    private float speed = 8;
    private bool isGrounded;
    private Rigidbody rigidBody;
    private Vector3 direction;
    private Vector3 moveDirection = Vector3.forward;
    private float horizontalInput;
    private float verticalInput;
    private bool canMove = true;
    private float nextAttackTime = 0f;

    private float health = 100f;
    // inventory
    public float water = 100f;
    public float gold = 0f;
    public float gems = 0f;
    private List<GemTradeType> gemInventory = new List<GemTradeType>();

    void Start()
    {
        rigidBody = transform.GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
            if (!sandTrailParticle.isEmitting)
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
        if (Input.GetMouseButtonDown(0) && canMove)
        {
            if (Time.time >= nextAttackTime)
            {
                animator.SetBool("Attack", true);
                nextAttackTime = Time.time + 1f / attackRate;
                Attack();
            }
        }
        if (canMove)
        {
            visualTransform.rotation = Quaternion.Slerp(visualTransform.rotation, Quaternion.LookRotation(moveDirection), Time.deltaTime * 10f);
        }
        if (Input.GetKey(KeyCode.Tab))
        {
            inventoryUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            inventoryUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        UpdateUI();
    }

    void FixedUpdate()
    {
        if (water > 0)
        {
            water -= Time.deltaTime * waterDrainRate;
        }
        bool isRunning = direction.magnitude > 0.1f;

        if (isRunning && canMove)
        {
            if (Input.GetKey(KeyCode.LeftShift) && water >= 0)
            {
                speed = moveSpeed * runMultiplier;
                animator.SetBool("Run", true);
                animator.SetBool("Walk", false);
                if (water > 0)
                {
                    water -= Time.deltaTime * waterDrainRateRun;
                }
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

    private void Attack()
    {
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attackRange, LayerMask.GetMask("Enemy"));
        foreach (Collider enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyController>().TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(float damageDealt)
    {
        if (health <= 0)
        {
            return;
        }
        health -= damageDealt;
        if (health <= 0)
        {
            animator.SetTrigger("Die");
            canMove = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            animator.SetBool("TakeDamage", true);
            Debug.Log("Player health: " + health);
        }
    }

    private void OnTriggerEnter(Collider other)
    {

    }

    public int WaterReplenishCost()
    {
        int cost = Mathf.RoundToInt(20 - (water / 5));
        return Mathf.Clamp(cost, 1, 20);
    }

    public void ReplenishWater()
    {
        water = 100;
    }

    public void AddGold(int goldQuantity)
    {
        gold += goldQuantity;
    }

    public void AddGems(GemTradeType gemType)
    {
        gemInventory.Add(gemType);
    }

    public int TradeGemsValue()
    {
        int goldToAdd = 0;
        foreach (GemTradeType gemType in gemInventory)
        {
            goldToAdd += (int)gemType;
        }
        return goldToAdd;
    }

    public void TradeGems()
    {
        int goldToAdd = TradeGemsValue();
        gold += goldToAdd;
        gemInventory.Clear();
    }

    public enum GemTradeType
    {
        Stone1 = 10,
        Stone2 = 15,
        Stone5 = 20,
        Stone6 = 25,
    }

    private void UpdateUI()
    {
        healthBar.fillAmount = health / 100f;
        waterBar.fillAmount = water / 100f;
        goldBar.fillAmount = gold / 500f;
        goldText.text = gold + " Gold";
        gemsText.text = gems + " Gems";
        foreach (TextMeshProUGUI waterText in waterTexts)
        {
            waterText.text = (int)water + "% Water";
        }
        goldGoalText.text = gold + " / 500 Gold Goal";
        conveienceGoldWaterText.text = WaterReplenishCost() + " Gold";
        conveienceGoldGemText.text = TradeGemsValue() + " Gold";
        convenienceGemText.text = gems + " Gems";
        replenishGoldText.text = WaterReplenishCost() + " Gold";
        tradeGemsText.text = gems + " Gems";
        tradeGoldText.text = TradeGemsValue() + " Gold";
    }
}