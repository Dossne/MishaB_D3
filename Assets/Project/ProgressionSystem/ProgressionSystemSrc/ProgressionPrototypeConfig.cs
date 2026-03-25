using UnityEngine;

namespace RainbowTower.ProgressionSystem
{
    [CreateAssetMenu(
        fileName = "ProgressionPrototypeConfig",
        menuName = "RainbowTower/ProgressionSystem/Progression Prototype Config")]
    public sealed class ProgressionPrototypeConfig : ScriptableObject
    {
        [SerializeField, Min(0)] private int startXp;
        [SerializeField, Min(0)] private int baseWaveClearXp = 2;
        [SerializeField, Min(0)] private int waveClearXpAddedPerWave = 1;
        [SerializeField, Min(0)] private int waveClearXpBonusEveryWaves = 3;
        [SerializeField, Min(0)] private int waveClearXpBonusAmount = 1;

        public int StartXp => Mathf.Max(0, startXp);

        public int GetWaveClearXp(int waveNumber)
        {
            var normalizedWave = Mathf.Max(1, waveNumber);
            var linear = Mathf.Max(0, waveClearXpAddedPerWave) * (normalizedWave - 1);
            var milestone = Mathf.Max(1, waveClearXpBonusEveryWaves);
            var milestoneBonus = ((normalizedWave - 1) / milestone) * Mathf.Max(0, waveClearXpBonusAmount);
            return Mathf.Max(0, baseWaveClearXp + linear + milestoneBonus);
        }
    }
}
