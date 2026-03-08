using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    static readonly int Grounded = Animator.StringToHash("Grounded");
    static readonly int Jump = Animator.StringToHash("Jump");
    static readonly int Attack1 = Animator.StringToHash("Attack");
    static readonly int Run = Animator.StringToHash("Run");
    static readonly int Walk = Animator.StringToHash("Walk");
    static readonly int RunSpeedMult = Animator.StringToHash("RunSpeedMult");
    static readonly int WalkSpeedMult = Animator.StringToHash("WalkSpeedMult");
    static readonly int Die = Animator.StringToHash("Die");
    static readonly int Death = Animator.StringToHash("Death");
    static readonly int Damage = Animator.StringToHash("TakeDamage");

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
    [SerializeField] private CinemachineInputAxisController freeLookCamera;
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
    [SerializeField] private Button tradeGemButton;
    [SerializeField] private Button replenishButton;
    [SerializeField] private Button convenienceWaterButton;
    [SerializeField] private Button convenienceGemButton;
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private GameObject deathUI;
    [SerializeField] private TextMeshProUGUI deathGold;

    private float groundCheckRadius = 0.3f;
    private float speed = 8;
    private bool isGrounded;
    private Rigidbody rigidBody;
    private Vector3 direction;
    private Vector3 moveDirection = Vector3.left;
    private bool canMove = true;
    private float nextAttackTime = 0f;
    private GameObject interactingChest;
    private GameObject interactingTrade;
    private GameObject interactingWater;
    private GameObject interactingConvenience;
    private GameObject interactingWin;

    private float health = 100f;
    // inventory
    public float water = 100f;
    public float gold = 0f;
    public float gems = 0f;
    private List<GemTradeType> gemInventory = new List<GemTradeType>();

    Camera _mainCamera;

    InputAction moveAction;
    InputAction jumpAction;
    InputAction sprintAction;
    InputAction attackAction;
    InputAction menuAction;
    InputAction interactAction;

    void Start()
    {
        rigidBody = transform.GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (PlayerPrefs.HasKey("gems"))
        {
            gems = PlayerPrefs.GetFloat("gems");
        }
        if (PlayerPrefs.HasKey("gold"))
        {
            gold = PlayerPrefs.GetFloat("gold");
        }
        _mainCamera = Camera.main;
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        attackAction = InputSystem.actions.FindAction("Attack");
        menuAction = InputSystem.actions.FindAction("Menu");
        interactAction = InputSystem.actions.FindAction("Interact");
    }

    void Update()
    {
        direction = moveAction.ReadValue<Vector2>();
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        animator.SetBool(Grounded, isGrounded);
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

        if (jumpAction.WasPressedThisFrame() && isGrounded && canMove)
        {
            rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetBool(Jump, true);
        }
        if (attackAction.WasPressedThisFrame() && canMove)
        {
            if (Time.time >= nextAttackTime)
            {
                animator.SetBool(Attack1, true);
                nextAttackTime = Time.time + 1f / attackRate;
                Attack();
            }
        }
        if (canMove)
        {
            visualTransform.rotation = Quaternion.Slerp(visualTransform.rotation, Quaternion.LookRotation(moveDirection), Time.deltaTime * 10f);
        }
        if (menuAction.IsPressed() && canMove)
        {
            inventoryUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            freeLookCamera.Controllers[0].Enabled = false;
            freeLookCamera.Controllers[1].Enabled = false;
        }
        else if (canMove)
        {
            inventoryUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            freeLookCamera.Controllers[0].Enabled = true;
            freeLookCamera.Controllers[1].Enabled = true;
        }
        if (interactingChest || interactingTrade || interactingWater || interactingConvenience)
        {
            interactionUI.SetActive(true);
        }
        else
        {
            interactionUI.SetActive(false);
        }
        if (interactAction.WasPressedThisFrame())
        {
            if (interactingChest)
            {
                interactingChest.GetComponent<Chest>().OpenChest();
                interactingChest = null;
            }
            if (interactingTrade)
            {
                OpenTradeGemUI();
                interactingTrade = null;
            }
            if (interactingWater)
            {
                OpenReplenishUI();
                interactingWater = null;
            }
            if (interactingConvenience)
            {
                OpenConvenienceUI();
                interactingConvenience = null;
            }
            //if (interactingWin != null)
            //{
            //    CloseGameAndClearSaved();
            //    interactingWin = null;
            //}
        }
        UpdateUI();
        interactionUI.transform.LookAt(_mainCamera.transform);

        bool isRunning = direction.magnitude > 0.1f;

        if (isRunning && canMove)
        {
            if (sprintAction.IsPressed() && water >= 0)
            {
                animator.SetBool(Run, true);
                animator.SetBool(Walk, false);
            }
            else
            {
                animator.SetBool(Run, false);
                animator.SetBool(Walk, true);
            }

            animator.SetFloat(RunSpeedMult, direction.magnitude * rigidBody.linearVelocity.magnitude * runVisualMultiplier);
            animator.SetFloat(WalkSpeedMult, direction.magnitude * rigidBody.linearVelocity.magnitude * walkVisualMultiplier);
        }
        else
        {
            animator.SetBool(Run, false);
            animator.SetBool(Walk, false);
        }

        if (rigidBody.linearVelocity.y <= 0)
        {
            animator.SetBool(Jump, false);
        }
    }

    void FixedUpdate()
    {
        if (water > 0)
        {
            water -= Time.deltaTime * waterDrainRate;
        }
        bool isRunning = direction.magnitude > 0.1f;

        if (!isRunning || !canMove)
        {
            return;
        }

        if (sprintAction.IsPressed() && water >= 0)
        {
            speed = moveSpeed * runMultiplier;
            if (water > 0)
            {
                water -= Time.deltaTime * waterDrainRateRun;
            }
        }
        else
        {
            speed = moveSpeed * walkMultiplier;
        }
        Vector3 viewDir = transform.position - _mainCamera.transform.position;
        viewDir.y = 0;
        orientation.forward = viewDir.normalized;
        Quaternion targetRotation = Quaternion.LookRotation(viewDir);
        orientation.rotation = targetRotation;
        moveDirection = orientation.forward * moveAction.ReadValue<Vector2>().y + orientation.right * moveAction.ReadValue<Vector2>().x;
        rigidBody.AddForce(moveDirection.normalized * (speed * 10f), ForceMode.Force);
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
            animator.SetTrigger(Die);
            canMove = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            freeLookCamera.Controllers[0].Enabled = false;
            freeLookCamera.Controllers[1].Enabled = false;
            PlayerPrefs.SetFloat("gems", gems);
            PlayerPrefs.SetFloat("gold", gold);
            deathUI.SetActive(true);
            deathGold.text = gold.ToString(CultureInfo.InvariantCulture);
            deathUI.GetComponent<Animator>().SetBool(Death, true);
            Invoke(nameof(ReloadScene), 2.1f);
        }
        else
        {
            animator.SetBool(Damage, true);
            Debug.Log("Player health: " + health);
        }
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Chest"))
        {
            interactingChest = other.gameObject;
        }
        if (other.CompareTag("Gems"))
        {
            interactingTrade = other.gameObject;
        }
        if (other.CompareTag("Water"))
        {
            interactingWater = other.gameObject;
        }
        if (other.CompareTag("Convenience"))
        {
            interactingConvenience = other.gameObject;
        }
        if (other.CompareTag("Win"))
        {
            interactingWin = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Chest"))
        {
            interactingChest = null;
        }
        if (other.CompareTag("Gems"))
        {
            interactingTrade = null;
        }
        if (other.CompareTag("Water"))
        {
            interactingWater = null;
        }
        if (other.CompareTag("Convenience"))
        {
            interactingConvenience = null;
        }
        if (other.CompareTag("Win"))
        {
            interactingWin = null;
        }
    }

    int WaterReplenishCost(bool convenience)
    {
        if (convenience)
        {
            int cost = Mathf.RoundToInt(40 - (water / 2.5f));
            return Mathf.Clamp(cost, 1, 40);
        }
        else
        {
            int cost = Mathf.RoundToInt(20 - (water / 5));
            return Mathf.Clamp(cost, 1, 20);
        }
    }

    void ReplenishWater()
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
        Debug.Log("Added " + gemType);
    }

    int TradeGemsValue(bool convenience)
    {
        int goldToAdd = 0;
        foreach (GemTradeType gemType in gemInventory)
        {
            goldToAdd += (int)gemType;
        }
        if (!convenience)
        {
            goldToAdd = Mathf.RoundToInt(goldToAdd * 1.5f);
        }
        return goldToAdd;
    }

    public void TradeGems(bool convenience)
    {
        int goldToAdd = TradeGemsValue(convenience);
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
        gems = gemInventory.Count;
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
        conveienceGoldWaterText.text = WaterReplenishCost(true) + " Gold";
        conveienceGoldGemText.text = TradeGemsValue(true) + " Gold";
        convenienceGemText.text = gems + " Gems";
        replenishGoldText.text = WaterReplenishCost(false) + " Gold";
        tradeGemsText.text = gems + " Gems";
        tradeGoldText.text = TradeGemsValue(false) + " Gold";
        tradeGemButton.interactable = gemInventory.Count > 0;
        replenishButton.interactable = water < 100;
        convenienceWaterButton.interactable = water < 100;
        convenienceGemButton.interactable = gemInventory.Count > 0;
    }

    public void BuyWater(bool convenience)
    {
        int cost = WaterReplenishCost(convenience);
        if (!(gold >= cost))
        {
            return;
        }

        gold -= cost;
        ReplenishWater();
    }

    public void SellGems(bool convenience)
    {
        int value = TradeGemsValue(convenience);
        if (value <= 0)
        {
            return;
        }

        gold += value;
        gemInventory.Clear();
    }

    public void CloseUI()
    {
        tradeGemUI.SetActive(false);
        replenishUI.SetActive(false);
        convenienceUI.SetActive(false);
        canMove = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        freeLookCamera.Controllers[0].Enabled = true;
        freeLookCamera.Controllers[1].Enabled = true;
    }

    public void OpenTradeGemUI()
    {
        tradeGemUI.SetActive(true);
        canMove = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        freeLookCamera.Controllers[0].Enabled = false;
        freeLookCamera.Controllers[1].Enabled = false;
    }

    public void OpenReplenishUI()
    {
        replenishUI.SetActive(true);
        canMove = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        freeLookCamera.Controllers[0].Enabled = false;
        freeLookCamera.Controllers[1].Enabled = false;
    }

    public void OpenConvenienceUI()
    {
        convenienceUI.SetActive(true);
        canMove = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        freeLookCamera.Controllers[0].Enabled = false;
        freeLookCamera.Controllers[1].Enabled = false;
    }

    public void CloseGameAndClearSaved()
    {
        PlayerPrefs.DeleteAll();
        Application.Quit();
    }
}