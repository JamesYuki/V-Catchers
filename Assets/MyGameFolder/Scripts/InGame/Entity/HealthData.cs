using UnityEngine;

namespace InGame.Entity
{
    /// <summary>
    /// HP設定用のScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "HealthData", menuName = "MyGame/Entity/HealthData")]
    public class HealthData : ScriptableObject
    {
        [Header("HP設定")]
        [SerializeField, Min(1)] private int m_MaxHealth = 100;
        
        [Header("無敵時間設定")]
        [SerializeField, Min(0f)] private float m_InvincibilityDuration = 0.5f;
        
        [Header("ダメージエフェクト")]
        [SerializeField] private GameObject m_DamageEffectPrefab;
        [SerializeField] private GameObject m_DeathEffectPrefab;

        /// <summary>
        /// 最大HP
        /// </summary>
        public int MaxHealth => m_MaxHealth;

        /// <summary>
        /// 無敵時間（秒）
        /// </summary>
        public float InvincibilityDuration => m_InvincibilityDuration;

        /// <summary>
        /// ダメージエフェクトのプレハブ
        /// </summary>
        public GameObject DamageEffectPrefab => m_DamageEffectPrefab;

        /// <summary>
        /// 死亡エフェクトのプレハブ
        /// </summary>
        public GameObject DeathEffectPrefab => m_DeathEffectPrefab;
    }
}
