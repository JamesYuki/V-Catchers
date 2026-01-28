using UnityEngine;

namespace InGame.Entity.Damage
{
    /// <summary>
    /// 速度ベースのダメージ条件
    /// 一定速度以上でダメージを与える
    /// </summary>
    [CreateAssetMenu(fileName = "VelocityDamageCondition", menuName = "MyGame/Damage/Velocity Condition")]
    public class VelocityDamageCondition : DamageConditionBase
    {
        [Header("速度閾値")]
        [SerializeField, Tooltip("この速度以上でダメージが発生する")]
        private float m_MinVelocityThreshold = 5f;

        [SerializeField, Tooltip("この速度以上で最大ダメージ（0で無効）")]
        private float m_MaxVelocityThreshold = 15f;

        [Header("ダメージスケーリング")]
        [SerializeField, Tooltip("速度に応じてダメージを増加させるか")]
        private bool m_ScaleDamageWithVelocity = false;

        [SerializeField, Tooltip("最大速度時のダメージ倍率")]
        private float m_MaxDamageMultiplier = 2f;

        /// <summary>
        /// 最小速度閾値
        /// </summary>
        public float MinVelocityThreshold => m_MinVelocityThreshold;

        /// <summary>
        /// 最大速度閾値
        /// </summary>
        public float MaxVelocityThreshold => m_MaxVelocityThreshold;

        public override bool IsSatisfied(DamageContext context)
        {
            return context.ImpactSpeed >= m_MinVelocityThreshold;
        }

        public override int CalculateDamage(DamageContext context, int baseDamage)
        {
            if (!m_ScaleDamageWithVelocity || m_MaxVelocityThreshold <= m_MinVelocityThreshold)
            {
                return baseDamage;
            }

            // 速度に応じたダメージスケーリング
            float velocityRange = m_MaxVelocityThreshold - m_MinVelocityThreshold;
            float velocityRatio = Mathf.Clamp01((context.ImpactSpeed - m_MinVelocityThreshold) / velocityRange);
            float multiplier = Mathf.Lerp(1f, m_MaxDamageMultiplier, velocityRatio);

            return Mathf.RoundToInt(baseDamage * multiplier);
        }
    }
}
