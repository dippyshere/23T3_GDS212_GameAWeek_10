using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    static readonly int Property = Animator.StringToHash("Walk Forward");
    static readonly int Property1 = Animator.StringToHash("Stab Attack");
    static readonly int Die = Animator.StringToHash("Die");
    static readonly int Property2 = Animator.StringToHash("Take Damage");
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float health = 100f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 4f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRate = 0.5f;
    [SerializeField] private float movementRange = 30f;
    [SerializeField] private Image healthBar;
    [SerializeField] private GameObject healthBarCanvas;

    private Transform homePosition;
    private GameObject player;
    private float nextAttackTime = 0f;
    private float nextDamageTime = 0f;
    bool isDead = false;

    Camera _mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        homePosition = transform;
        agent.SetDestination(homePosition.position);
        healthBarCanvas.SetActive(false);
        _mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool(Property, !(agent.remainingDistance <= agent.stoppingDistance));
        if (!player || isDead)
        {
            return;
        }

        if (Vector3.Distance(homePosition.position, player.transform.position) <= movementRange)
        {
            agent.SetDestination(player.transform.position);
            healthBar.fillAmount = 1 - (health / 100);
            if (Vector3.Distance(attackPoint.position, player.transform.position) <= attackRange)
            {
                if (Time.time >= nextAttackTime)
                {
                    nextAttackTime = Time.time + 1f / attackRate;
                    animator.SetBool(Property1, true);
                    Attack();
                }
            }
            healthBarCanvas.transform.LookAt(_mainCamera.transform);
        }
        else
        {
            agent.SetDestination(homePosition.position);
        }
    }

    public void TakeDamage(float damageDealt)
    {
        if (Time.time < nextDamageTime || health <= 0)
        {
            nextDamageTime = Time.time + 0.5f;
            return;
        }
        health -= damageDealt;
        if (health <= 0)
        {
            animator.SetTrigger(Die);
            Destroy(gameObject, 1.3f);
            isDead = true;
            // move trail particle systems to separate game object to prevent it from being destroyed
            ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particleSystem in particleSystems)
            {
                particleSystem.transform.parent = null;
            }
        }
        else
        {
            animator.SetBool(Property2, true);
            Debug.Log("Enemy health: " + health);
        }
    }

    private void Attack()
    {
        if (Vector3.Distance(attackPoint.position, player.transform.position) <= attackRange &&
            Vector3.Dot(transform.forward, (player.transform.position - transform.position).normalized) > 0.5f)
        {
            PlayerController.Instance.TakeDamage(attackDamage);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        Vector3 forward = transform.forward * attackRange;
        Gizmos.DrawLine(attackPoint.position, attackPoint.position + forward);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (other.gameObject.GetComponent<PlayerController>() == null)
        {
            return;
        }

        player = other.gameObject;
        healthBarCanvas.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (other.gameObject.GetComponent<PlayerController>() == null)
        {
            return;
        }

        player = null;
        healthBarCanvas.SetActive(false);
    }
}
