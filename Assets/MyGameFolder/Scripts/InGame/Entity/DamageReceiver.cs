using UnityEngine;
using InGame.Entity.Damage;

namespace InGame.Entity
{
    /// <summary>
    /// ダメージを受けるコンポーネント（汎用）
    /// IDamageableを実装したHealthModuleなどに橋渡しする
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DamageReceiver : MonoBehaviour
    {
        [Header("ダメージ処理対象")]
        [SerializeField, Tooltip("ダメージを処理するコンポーネント（自動検索も可）")]
        private MonoBehaviour m_DamageableComponent;

        [Header("ダメージ条件")]
        [SerializeField, Tooltip("ダメージを受ける条件（オプション）")]
        private DamageConditionBase m_ReceiveCondition;

        [Header("ダメージソース設定")]
        [SerializeField, Tooltip("ダメージソースレイヤー")]
        private LayerMask m_DamageSourceLayers;

        [SerializeField, Tooltip("デフォルトダメージ量（IDamageSourceがない場合）")]
        private int m_DefaultDamage = 1;

        [Header("速度ベースダメージ")]
        [SerializeField, Tooltip("速度からダメージを計算するか")]
        private bool m_UseVelocityBasedDamage = false;

        [SerializeField, Tooltip("ダメージ発生の最低速度")]
        private float m_MinDamageVelocity = 5f;

        [SerializeField, Tooltip("速度からダメージへの変換係数")]
        private float m_VelocityToDamageRatio = 0.5f;

        private IDamageable m_Damageable;
        private Rigidbody2D m_Rigidbody;

        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody2D>();

            // IDamageableを探す
            if (m_DamageableComponent != null)
            {
                m_Damageable = m_DamageableComponent as IDamageable;
            }

            if (m_Damageable == null)
            {
                m_Damageable = GetComponent<IDamageable>();
            }

            if (m_Damageable == null)
            {
                m_Damageable = GetComponentInParent<IDamageable>();
            }

            if (m_Damageable == null)
            {
                AppLogger.LogWarning($"[DamageReceiver] {gameObject.name}: IDamageableが見つかりません");
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryReceiveDamage(collision.gameObject, collision);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryReceiveDamage(other.gameObject, null);
        }

        /// <summary>
        /// ダメージを受けることを試みる
        /// </summary>
        private void TryReceiveDamage(GameObject source, Collision2D collision)
        {
            if (m_Damageable == null || !m_Damageable.IsAlive) return;

            // レイヤーチェック
            if (m_DamageSourceLayers != 0 && ((1 << source.layer) & m_DamageSourceLayers) == 0) return;

            // ダメージコンテキストを作成
            var context = CreateDamageContext(source, collision);

            // 条件チェック
            if (m_ReceiveCondition != null && !m_ReceiveCondition.IsSatisfied(context))
            {
                return;
            }

            // ダメージ量を計算
            int damage = CalculateDamage(source, context);

            if (damage > 0)
            {
                m_Damageable.TakeDamage(damage, source);
            }
        }

        /// <summary>
        /// ダメージコンテキストを作成
        /// </summary>
        private DamageContext CreateDamageContext(GameObject source, Collision2D collision)
        {
            var sourceRb = source.GetComponent<Rigidbody2D>();
            Vector2 relativeVelocity;

            if (collision != null)
            {
                relativeVelocity = collision.relativeVelocity;
            }
            else
            {
                relativeVelocity = sourceRb != null ? sourceRb.linearVelocity : Vector2.zero;
                if (m_Rigidbody != null)
                {
                    relativeVelocity -= m_Rigidbody.linearVelocity;
                }
            }

            Vector2 contactPoint = collision != null && collision.contactCount > 0
                ? collision.GetContact(0).point
                : (Vector2)transform.position;

            return new DamageContext
            {
                Source = source,
                Target = gameObject,
                SourceRigidbody = sourceRb,
                TargetRigidbody = m_Rigidbody,
                Collision = collision,
                ContactPoint = contactPoint,
                RelativeVelocity = relativeVelocity
            };
        }

        /// <summary>
        /// ダメージ量を計算
        /// </summary>
        private int CalculateDamage(GameObject source, DamageContext context)
        {
            int damage = m_DefaultDamage;

            // IDamageSourceからダメージを取得
            var damageSource = source.GetComponent<IDamageSource>();
            if (damageSource != null)
            {
                damage = damageSource.DamageAmount;
            }

            // 速度ベースダメージ
            if (m_UseVelocityBasedDamage)
            {
                float speed = context.ImpactSpeed;
                if (speed < m_MinDamageVelocity)
                {
                    return 0; // 最低速度未満はダメージなし
                }

                int velocityDamage = Mathf.RoundToInt((speed - m_MinDamageVelocity) * m_VelocityToDamageRatio);
                damage = Mathf.Max(damage, velocityDamage);
            }

            // 条件によるダメージ補正
            if (m_ReceiveCondition != null)
            {
                damage = m_ReceiveCondition.CalculateDamage(context, damage);
            }

            return damage;
        }

        /// <summary>
        /// IDamageableを手動で設定
        /// </summary>
        public void SetDamageable(IDamageable damageable)
        {
            m_Damageable = damageable;
        }
    }
}
