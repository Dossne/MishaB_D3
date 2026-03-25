using UnityEngine;

namespace RainbowTower.WaveSystem
{
    [CreateAssetMenu(
        fileName = "WavePrototypeConfig",
        menuName = "RainbowTower/WaveSystem/Wave Prototype Config")]
    public sealed class WavePrototypeConfig : ScriptableObject
    {
        [SerializeField] private int startPlayerHp = 20;
        [SerializeField] private int totalWaves = 10;
        [SerializeField] private float initialDelay = 0.8f;
        [SerializeField] private float timeBetweenWaves = 1.75f;
        [SerializeField] private int baseEnemiesPerWave = 4;
        [SerializeField] private int enemiesAddedPerWave = 1;
        [SerializeField] private float spawnInterval = 0.9f;

        public int StartPlayerHp => startPlayerHp;
        public int TotalWaves => totalWaves;
        public float InitialDelay => initialDelay;
        public float TimeBetweenWaves => timeBetweenWaves;
        public int BaseEnemiesPerWave => baseEnemiesPerWave;
        public int EnemiesAddedPerWave => enemiesAddedPerWave;
        public float SpawnInterval => spawnInterval;
    }
}
