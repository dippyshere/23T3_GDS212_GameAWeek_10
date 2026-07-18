using UnityEngine;
using UnityEngine.InputSystem;

public static class InputSystemEditorFix
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void EnableGlobalInputActions()
    {
        if (InputSystem.actions != null)
        {
            InputSystem.actions.Enable();
        }
    }
}