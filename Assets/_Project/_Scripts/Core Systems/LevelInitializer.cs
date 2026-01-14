using GridTool;
using UnityEngine;
using VContainer;

public class LevelInitializer : MonoBehaviour
{
    [Inject] private ISpawnService spawnService;

    private void Start()
    {
        // Spawns all fixed spawns (King, Resources, etc.) that have "SpawnOnLevelStart = true"
        spawnService.SpawnAllFixed();
        
        Debug.Log("Level initialized with all fixed spawns!");
    }
}