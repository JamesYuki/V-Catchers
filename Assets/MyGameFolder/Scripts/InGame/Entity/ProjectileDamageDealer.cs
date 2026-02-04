using UnityEngine;
using InGame.Entity.Damage;
using InGame.Gimmick;

namespace InGame.Entity
{
    /// <summary>
    /// 投擲物としてダメージを与えるコンポーネント
    /// 箱など投げられるオブジェクトにアタッチ
    /// IGrabbableと連携して、投げた人へのダメージ免疫を自動管理
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class ProjectileDamageDealer : MonoBehaviour, IDamageSource
    {
        [Header("ダメージ設定")]
        [SerializeField, Min(1)]
        private int m_BaseDamage = 1;

        [Header("ダメージ条件")]
        [SerializeField, Tooltip("ダメージ発生条件（ScriptableObject）")]
        private DamageConditionBase m_DamageCondition;

        [Header("対象設定")]
        [SerializeField, Tooltip("ダメージを与える対象レイヤー")]
        private LayerMask m_TargetLayers;

        [SerializeField, Tooltip("自分自身にダメージを与えない")]
        private bool m_IgnoreSelf = true;

        [Header("状態")]
        [SerializeField, Tooltip("現在ダメージを与えることができる状態か")]
        private bool m_IsActive = true;

        [Header("クールダウン")]
        [SerializeField, Tooltip("同じ対象への連続ダメージを防ぐクールダウン（秒）")]
        private float m_DamageCooldown = 0.5f;

        [Header("投げた人への免疫設定")]
        [SerializeField, Tooltip("投げた人への免疫時間（秒）。0で永続免疫（何かに当たるまで）")]
        private float m_ThrowerImmunityDuration = 0f;

        [SerializeField, Tooltip("何かに衝突したら投げた人への免疫を解除する")]
        private bool m_ClearImmunityOnImpact = true;

        private Rigidbody2D m_Rigidbody;
        private IGrabbable m_Grabbable;

        private System.Collections.Generic.Dictionary<GameObject, float> m_DamageTimestamps
            = new System.Collections.Generic.Dictionary<GameObject, float>();

        // 投げた人（免疫対象）の追跡
        private GameObject m_LastThrower;
        private float m_ThrowerImmunityEndTime;
        private bool m_HasHitSomething;

        public int DamageAmount => m_BaseDamage;

        /// <summary>
        /// ダメージを与えることができる状態か
        /// </summary>
        public bool IsActive
        {
            get => m_IsActive;
            set => m_IsActive = value;
        }

        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody2D>();
            m_Grabbable = GetComponent<IGrabbable>();
        }

        private void OnEnable()
        {
            // IGrabbableがある場合、Grab/Release時にコールバックを受け取るための監視を開始
            if (m_Grabbable != null)
            {
                // 掴まれている状態で開始した場合の初期化
                if (m_Grabbable.IsGrabbed && m_Grabbable.CurrentGrabber != null)
                {
                    m_LastThrower = m_Grabbable.CurrentGrabber.GrabberObject;
                }
            }
        }

        private void Update()
        {
            // IGrabbableの状態を監視
            UpdateGrabbableState();
        }

        /// <summary>
        /// IGrabbableの状態を監視して、投げた人を追跡
        /// </summary>
        private void UpdateGrabbableState()
        {
            if (m_Grabbable == null) return;

            if (m_Grabbable.IsGrabbed)
            {
                // 掴まれている間は投げた人を更新し続ける
                if (m_Grabbable.CurrentGrabber != null)
                {
                    m_LastThrower = m_Grabbable.CurrentGrabber.GrabberObject;
                    m_HasHitSomething = false;
                }
            }
        }

        /// <summary>
        /// 外部から投げた人を設定（IGrabbableがない場合や手動制御用）
        /// </summary>
        public void SetThrower(GameObject thrower, float immunityDuration = -1f)
        {
            m_LastThrower = thrower;
            m_HasHitSomething = false;

            if (immunityDuration >= 0f)
            {
                m_ThrowerImmunityEndTime = Time.time + immunityDuration;
            }
            else if (m_ThrowerImmunityDuration > 0f)
            {
                m_ThrowerImmunityEndTime = Time.time + m_ThrowerImmunityDuration;
            }
            else
            {
                // 0以下の場合は永続免疫（何かに当たるまで）
                m_ThrowerImmunityEndTime = float.MaxValue;
            }
        }

        /// <summary>
        /// 投げた人への免疫をクリア
        /// </summary>
        public void ClearThrowerImmunity()
        {
            m_LastThrower = null;
            m_HasHitSomething = true;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!m_IsActive) return;

            bool dealtDamage = TryDealDamage(collision.gameObject, collision);

            // 何かに衝突したら免疫解除フラグを立てる
            if (m_ClearImmunityOnImpact && !IsThrowerImmune(collision.gameObject))
            {
                m_HasHitSomething = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!m_IsActive) return;

            TryDealDamage(other.gameObject, null);
        }

        /// <summary>
        /// 対象が投げた人で、免疫期間中かどうか
        /// </summary>
        private bool IsThrowerImmune(GameObject target)
        {
            if (m_LastThrower == null) return false;

            // 対象が投げた人かどうか（親階層も確認）
            bool isThrower = target == m_LastThrower ||
                             target.transform.IsChildOf(m_LastThrower.transform) ||
                             (m_LastThrower.transform.IsChildOf(target.transform));

            if (!isThrower) return false;

            // 既に何かに当たっていたら免疫解除
            if (m_HasHitSomething) return false;

            // 時間ベースの免疫チェック
            if (m_ThrowerImmunityDuration > 0f && Time.time > m_ThrowerImmunityEndTime)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// ダメージを与えることを試みる
        /// </summary>
        private bool TryDealDamage(GameObject target, Collision2D collision)
        {
            // 自分自身チェック
            if (m_IgnoreSelf && target == gameObject) return false;

            // 投げた人への免疫チェック
            if (IsThrowerImmune(target))
            {
                return false;
            }

            // レイヤーチェック
            if (m_TargetLayers != 0 && ((1 << target.layer) & m_TargetLayers) == 0) return false;

            // クールダウンチェック
            if (!CheckCooldown(target)) return false;

            // ダメージコンテキストを作成
            var context = CreateDamageContext(target, collision);

            // 条件チェック
            if (m_DamageCondition != null && !m_DamageCondition.IsSatisfied(context))
            {
                return false;
            }

            // ダメージ量を計算
            int damage = m_BaseDamage;
            if (m_DamageCondition != null)
            {
                damage = m_DamageCondition.CalculateDamage(context, damage);
            }

            // ダメージを与える
            ApplyDamage(target, damage, context);
            return true;
        }

        /// <summary>
        /// ダメージコンテキストを作成
        /// </summary>
        private DamageContext CreateDamageContext(GameObject target, Collision2D collision)
        {
            var targetRb = target.GetComponent<Rigidbody2D>();
            Vector2 relativeVelocity;

            if (collision != null)
            {
                relativeVelocity = collision.relativeVelocity;
            }
            else
            {
                // Triggerの場合は自身の速度を使用
                relativeVelocity = m_Rigidbody != null ? m_Rigidbody.linearVelocity : Vector2.zero;
                if (targetRb != null)
                {
                    relativeVelocity -= targetRb.linearVelocity;
                }
            }

            Vector2 contactPoint = collision != null && collision.contactCount > 0
                ? collision.GetContact(0).point
                : (Vector2)target.transform.position;

            return new DamageContext
            {
                Source = gameObject,
                Target = target,
                SourceRigidbody = m_Rigidbody,
                TargetRigidbody = targetRb,
                Collision = collision,
                ContactPoint = contactPoint,
                RelativeVelocity = relativeVelocity
            };
        }

        /// <summary>
        /// クールダウンをチェック
        /// </summary>
        private bool CheckCooldown(GameObject target)
        {
            if (m_DamageCooldown <= 0) return true;

            float currentTime = Time.time;

            if (m_DamageTimestamps.TryGetValue(target, out float lastDamageTime))
            {
                if (currentTime - lastDamageTime < m_DamageCooldown)
                {
                    return false;
                }
            }

            m_DamageTimestamps[target] = currentTime;
            return true;
        }

        /// <summary>
        /// ダメージを適用
        /// </summary>
        private void ApplyDamage(GameObject target, int damage, DamageContext context)
        {
            // IDamageableインターフェースを持つコンポーネントを探す
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, gameObject);
                OnDamageDealt(target, damage, context);
                return;
            }

            // 親オブジェクトも探す
            damageable = target.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, gameObject);
                OnDamageDealt(target, damage, context);
            }
        }

        /// <summary>
        /// ダメージを与えた時のコールバック（オーバーライド用）
        /// </summary>
        protected virtual void OnDamageDealt(GameObject target, int damage, DamageContext context)
        {
            AppLogger.Log($"[ProjectileDamageDealer] {gameObject.name} dealt {damage} damage to {target.name} (Speed: {context.ImpactSpeed:F1})");
        }

        /// <summary>
        /// 基本ダメージを設定
        /// </summary>
        public void SetBaseDamage(int damage)
        {
            m_BaseDamage = Mathf.Max(1, damage);
        }

        /// <summary>
        /// ダメージ条件を設定
        /// </summary>
        public void SetDamageCondition(DamageConditionBase condition)
        {
            m_DamageCondition = condition;
        }

        /// <summary>
        /// クールダウン記録をクリア
        /// </summary>
        public void ClearCooldowns()
        {
            m_DamageTimestamps.Clear();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // 現在の速度を可視化
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null && Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, rb.linearVelocity * 0.2f);

                // 速度閾値を表示
                if (m_DamageCondition is VelocityDamageCondition velocityCondition)
                {
                    float currentSpeed = rb.linearVelocity.magnitude;
                    bool canDamage = currentSpeed >= velocityCondition.MinVelocityThreshold;
                    Gizmos.color = canDamage ? Color.green : Color.red;
                    Gizmos.DrawWireSphere(transform.position, 0.3f);
                }
            }
        }
#endif
    }
}
