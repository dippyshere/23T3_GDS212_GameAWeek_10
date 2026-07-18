using System;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;
using Random = UnityEngine.Random;

public class NPCAnimatorSet : MonoBehaviour
{
    [NoAutoStaticsCleanup]
    static readonly int Drink = Animator.StringToHash("Drink");
    [NoAutoStaticsCleanup]
    static readonly int Unarmed = Animator.StringToHash("Unarmed");
    [NoAutoStaticsCleanup]
    static readonly int Idle2 = Animator.StringToHash("Idle2");
    [NoAutoStaticsCleanup]
    static readonly int IdleSpeed = Animator.StringToHash("IdleSpeed");
    public Animator animator;
    public int idleType;
    public bool isDrinking;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        animator.SetBool(Drink, isDrinking);
        switch (idleType)
        {
            case 1:
                animator.SetBool(Unarmed, true);
                break;
            case 2:
                animator.SetBool(Idle2, true);
                break;
            default:
                break;
        }
        animator.SetFloat(IdleSpeed, Random.Range(0.8f, 1.2f));
    }
}
