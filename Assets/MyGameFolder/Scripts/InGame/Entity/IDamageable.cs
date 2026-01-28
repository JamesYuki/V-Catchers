using UnityEngine;

namespace InGame.Entity
{
    /// <summary>
    /// ダメージを受けることができるオブジェクトのインターフェース
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="damage">ダメージ量</param>
        /// <param name="damageSource">ダメージ元（オプション）</param>
        void TakeDamage(int damage, GameObject damageSource = null);

        /// <summary>
        /// 現在のHP
        /// </summary>
        int CurrentHealth { get; }

        /// <summary>
        /// 最大HP
        /// </summary>
        int MaxHealth { get; }

        /// <summary>
        /// 生存しているか
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// HP割合（0.0〜1.0）
        /// </summary>
        float HealthRatio { get; }
    }
}
