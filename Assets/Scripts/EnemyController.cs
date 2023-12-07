using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
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

    // Start is called before the first frame update
    void Start()
    {
        homePosition = transform;
        agent.SetDestination(homePosition.position);
        healthBarCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            animator.SetBool("Walk Forward", false);
        }
        else
        {
            animator.SetBool("Walk Forward", true);
        }
        if (player != null)
        {
            if (Vector3.Distance(transform.position, player.transform.position) <= movementRange)
            {
                agent.SetDestination(player.transform.position);
                healthBar.fillAmount = 1 - (health / 100);
                if (Vector3.Distance(attackPoint.position, player.transform.position) <= attackRange)
                {
                    if (Time.time >= nextAttackTime)
                    {
                        nextAttackTime = Time.time + 1f / attackRate;
                        animator.SetBool("Stab Attack", true);
                        Attack();
                    }
                }
                healthBarCanvas.transform.LookAt(Camera.main.transform);
            }
            else
            {
                agent.SetDestination(homePosition.position);
            }
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
            animator.SetTrigger("Die");
            Destroy(gameObject, 1.3f);
            // move trail particle systems to separate game object to prevent it from being destroyed
            ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem particleSystem in particleSystems)
            {
                particleSystem.transform.parent = null;
            }
        }
        else
        {
            animator.SetBool("Take Damage", true);
            Debug.Log("Enemy health: " + health);
        }
    }

    private void Attack()
    {
        Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRange, LayerMask.GetMask("Player"));
        foreach (Collider player in hitPlayers)
        {
            player.GetComponent<PlayerController>().TakeDamage(attackDamage);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.gameObject.GetComponent<PlayerController>() != null)
            {
                player = other.gameObject;
                healthBarCanvas.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.gameObject.GetComponent<PlayerController>() != null)
            {
                player = null;
                healthBarCanvas.SetActive(false);
            }
        }
    }
}
