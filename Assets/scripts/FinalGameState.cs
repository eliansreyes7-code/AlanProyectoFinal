using UnityEngine;

public static class FinalGameState
{
    public static bool showReflectionPanel = false;

    // =====================================================
    // REINICIAR AL COMENZAR UNA NUEVA EJECUCIÓN
    // =====================================================

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        showReflectionPanel = false;
    }
}