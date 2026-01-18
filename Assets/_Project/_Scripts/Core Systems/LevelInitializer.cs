using GridTool;
using UnityEngine;
using VContainer;

public class LevelInitializer : MonoBehaviour
{
    [Inject] private ISpawnService spawnService;

    private void Start()
    {
        spawnService.SpawnAllFixed();
    }
}