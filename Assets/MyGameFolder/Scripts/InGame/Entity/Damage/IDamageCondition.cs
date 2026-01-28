using UnityEngine;

namespace InGame.Entity.Damage
{
    /// <summary>
    /// ダメージ発生条件のインターフェース
    /// Strategy パターンでダメージ条件を柔軟に設定可能
    /// </summary>
    public interface IDamageCondition
    {
        /// <summary>
        /// ダメージ条件を満たしているか判定
        /// </summary>
        /// <param name="context">ダメージコンテキスト</param>
        /// <returns>条件を満たしていればtrue</returns>
        bool IsSatisfied(DamageContext context);

        /// <summary>
        /// ダメージ量を計算（条件によってダメージ量が変動する場合）
        /// </summary>
        /// <param name="context">ダメージコンテキスト</param>
        /// <param name="baseDamage">基本ダメージ</param>
        /// <returns>最終ダメージ量</returns>
        int CalculateDamage(DamageContext context, int baseDamage);
    }

    /// <summary>
    /// ダメージ判定に必要なコンテキスト情報
    /// </summary>
    public struct DamageContext
    {
        /// <summary>
        /// ダメージを与える側のGameObject
        /// </summary>
        public GameObject Source;

        /// <summary>
        /// ダメージを受ける側のGameObject
        /// </summary>
        public GameObject Target;

        /// <summary>
        /// ダメージを与える側のRigidbody2D（あれば）
        /// </summary>
        public Rigidbody2D SourceRigidbody;

        /// <summary>
        /// ダメージを受ける側のRigidbody2D（あれば）
        /// </summary>
        public Rigidbody2D TargetRigidbody;

        /// <summary>
        /// 衝突情報（あれば）
        /// </summary>
        public Collision2D Collision;

        /// <summary>
        /// 衝突点（Triggerの場合など）
        /// </summary>
        public Vector2 ContactPoint;

        /// <summary>
        /// 相対速度
        /// </summary>
        public Vector2 RelativeVelocity;

        /// <summary>
        /// 相対速度の大きさ
        /// </summary>
        public float ImpactSpeed => RelativeVelocity.magnitude;
    }
}
