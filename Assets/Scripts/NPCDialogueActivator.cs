using UnityEngine;
using System.Collections;
using TMPro;

public class NPCDialogueActivator : MonoBehaviour
{
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TextMeshProUGUI dialogueTextComponent;
    [SerializeField] private string dialogueText;
    [SerializeField] Animator animator;

    static readonly int Property = Animator.StringToHash("Talk");
    private Camera _mainCamera;

     private void Start()
    {
        dialogueUI.SetActive(false);
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!dialogueUI.activeInHierarchy)
        {
            return;
        }

        Vector3 direction = _mainCamera.transform.position - dialogueUI.transform.position;
        direction.y = 0;
        dialogueUI.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        animator.SetBool(Property, true);
        StartCoroutine(WriteDialogue());
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        animator.SetBool(Property, false);
        StopCoroutine(WriteDialogue());
    }

    IEnumerator WriteDialogue()
    {
        dialogueUI.SetActive(true);
        dialogueTextComponent.text = "";

        foreach (char letter in dialogueText)
        {
            dialogueTextComponent.text += letter;
            yield return new WaitForSeconds(0.025f);
        }
    }
}
