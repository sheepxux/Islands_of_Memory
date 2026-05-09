using UnityEngine;

public class AutumnPuzzleStart : MonoBehaviour
{
    public AutumnLeafGameManager leafGameManager;

    public void TriggerLeafInstruction()
    {
        if (leafGameManager != null)
            leafGameManager.ShowInstruction();
    }
}