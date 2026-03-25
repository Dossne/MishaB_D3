using UnityEngine;

namespace RainbowTower.WaveSystem
{
    [CreateAssetMenu(
        fileName = "WavePrototypeConfig",
        menuName = "RainbowTower/WaveSystem/Wave Prototype Config")]
    public sealed class WavePrototypeConfig : ScriptableObject
    {
        [Header("Session")]
        [SerializeField, Min(1)] private int startPlayerHp = 20;
        [SerializeField, Min(1)] private int totalWaves = 10;
        [SerializeField, Min(0f)] private float initialDelay = 0.8f;

        [Header("Wave Count")]
        [SerializeField, Min(1)] private int baseEnemiesPerWave = 4;
        [SerializeField, Min(0)] private int enemiesAddedPerWave = 1;
        [SerializeField, Min(1)] private int enemyCountBonusEveryWaves = 3;
        [SerializeField, Min(0)] private int enemyCountBonusAmount = 1;

        [Header("Spawn Pacing")]
        [SerializeField, Min(0.1f)] private float baseSpawnInterval = 0.95f;
        [SerializeField, Min(0f)] private float spawnIntervalReductionPerWave = 0.05f;
        [SerializeField, Min(0.1f)] private float minSpawnInterval = 0.45f;

        [Header("Between Waves")]
        [SerializeField, Min(0f)] private float baseTimeBetweenWaves = 2.4f;
        [SerializeField, Min(0f)] private float timeBetweenWavesReductionPerWave = 0.12f;
        [SerializeField, Min(0.1f)] private float minTimeBetweenWaves = 0.9f;

        [Header("Enemy Scaling")]
        [SerializeField, Min(0)] private int enemyHpAddedPerWave = 1;
        [SerializeField, Min(1)] private int enemyHpBonusEveryWaves = 2;
        [SerializeField, Min(0)] private int enemyHpBonusAmount = 1;

        [Header("XP Scaling")]
        [SerializeField, Min(0)] private int rewardXpAddedPerWave = 0;
        [SerializeField, Min(1)] private int rewardXpBonusEveryWaves = 2;
        [SerializeField, Min(0)] private int rewardXpBonusAmount = 1;

        public int StartPlayerHp => Mathf.Max(1, startPlayerHp);
        public int TotalWaves => Mathf.Max(1, totalWaves);
        public float InitialDelay => Mathf.Max(0f, initialDelay);

        public int GetEnemiesToSpawnForWave(int waveNumber)
        {
            var normalizedWave = Mathf.Clamp(waveNumber, 1, TotalWaves);
            var linear = Mathf.Max(0, enemiesAddedPerWave) * (normalizedWave - 1);
            var milestone = Mathf.Max(1, enemyCountBonusEveryWaves);
            var milestoneBonus = ((normalizedWave - 1) / milestone) * Mathf.Max(0, enemyCountBonusAmount);
            return Mathf.Max(1, baseEnemiesPerWave + linear + milestoneBonus);
        }

        public float GetSpawnIntervalForWave(int waveNumber)
        {
            var normalizedWave = Mathf.Clamp(waveNumber, 1, TotalWaves);
            var reduced = baseSpawnInterval - (normalizedWave - 1) * Mathf.Max(0f, spawnIntervalReductionPerWave);
            return Mathf.Max(minSpawnInterval, reduced);
        }

        public float GetTimeBetweenWavesAfter(int completedWaveNumber)
        {
            var normalizedWave = Mathf.Clamp(completedWaveNumber, 1, TotalWaves);
            var reduced = baseTimeBetweenWaves - (normalizedWave - 1) * Mathf.Max(0f, timeBetweenWavesReductionPerWave);
            return Mathf.Max(minTimeBetweenWaves, reduced);
        }

        public int GetEnemyHpBonusForWave(int waveNumber)
        {
            var normalizedWave = Mathf.Clamp(waveNumber, 1, TotalWaves);
            var linear = Mathf.Max(0, enemyHpAddedPerWave) * (normalizedWave - 1);
            var milestone = Mathf.Max(1, enemyHpBonusEveryWaves);
            var milestoneBonus = ((normalizedWave - 1) / milestone) * Mathf.Max(0, enemyHpBonusAmount);
            return Mathf.Max(0, linear + milestoneBonus);
        }

        public int GetEnemyRewardXpBonusForWave(int waveNumber)
        {
            var normalizedWave = Mathf.Clamp(waveNumber, 1, TotalWaves);
            var linear = Mathf.Max(0, rewardXpAddedPerWave) * (normalizedWave - 1);
            var milestone = Mathf.Max(1, rewardXpBonusEveryWaves);
            var milestoneBonus = ((normalizedWave - 1) / milestone) * Mathf.Max(0, rewardXpBonusAmount);
            return Mathf.Max(0, linear + milestoneBonus);
        }
    }
}
