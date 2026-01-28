using UnityEngine;
using InGame.Entity;
using InGame.Gimmick;

namespace InGame.Enemy
{
    /// <summary>
    /// 敵がダメージを受ける当たり判定を処理
    /// ProjectileDamageDealerと連携して、投擲物からのダメージを処理
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EnemyDamageReceiver : MonoBehaviour
    {
        [Header("設定")]
        [SerializeField] private int m_DefaultDamage = 1;
        [SerializeField] private string m_DamageSourceTag = "Player";
        [SerializeField] private LayerMask m_DamageSourceLayer;

        [Header("速度ベースダメージ")]
        [SerializeField, Tooltip("投擲物の速度によるダメージを有効化")]
        private bool m_UseVelocityDamage = true;
        
        [SerializeField, Tooltip("ダメージを受ける最低速度")]
        private float m_MinDamageVelocity = 5f;

        private EnemyHealthModule m_HealthModule;
        private EnemyController m_EnemyController;

        private void Awake()
        {
            m_HealthModule = GetComponentInParent<EnemyHealthModule>();
            if (m_HealthModule == null)
            {
                m_HealthModule = GetComponent<EnemyHealthModule>();
            }

            m_EnemyController = GetComponentInParent<EnemyController>();
            if (m_EnemyController == null)
            {
                m_EnemyController = GetComponent<EnemyController>();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            TryApplyDamage(collision.gameObject, null);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryApplyDamage(collision.gameObject, collision);
        }

        private void TryApplyDamage(GameObject source, Collision2D collision)
        {
            if (m_HealthModule == null || !m_HealthModule.IsAlive)
            {
                return;
            }

            // 自分自身が投げたものかチェック（IGrabbableの場合）
            var grabbable = source.GetComponent<IGrabbable>();
            if (grabbable != null && m_EnemyController != null)
            {
                // 現在掴んでいる、または直前まで掴んでいた場合は無視
                if (grabbable.IsGrabbed && grabbable.CurrentGrabber?.GrabberObject == m_EnemyController.gameObject)
                {
                    return;
                }
            }

            // タグまたはレイヤーでフィルタリング
            bool isValidSource = false;
            
            if (!string.IsNullOrEmpty(m_DamageSourceTag) && source.CompareTag(m_DamageSourceTag))
            {
                isValidSource = true;
            }
            
            if (m_DamageSourceLayer != 0 && ((1 << source.layer) & m_DamageSourceLayer) != 0)
            {
                isValidSource = true;
            }

            // ProjectileDamageDealerがあればそちらで処理される（二重処理を避ける）
            var projectileDealer = source.GetComponent<ProjectileDamageDealer>();
            if (projectileDealer != null && projectileDealer.IsActive)
            {
                // ProjectileDamageDealerが処理するので、ここでは何もしない
                return;
            }

            if (!isValidSource)
            {
                return;
            }

            // ダメージ量を取得
            int damage = m_DefaultDamage;
            var damageSource = source.GetComponent<IDamageSource>();
            if (damageSource != null)
            {
                damage = damageSource.DamageAmount;
            }

            // 速度ベースダメージのチェック
            if (m_UseVelocityDamage && collision != null)
            {
                float impactSpeed = collision.relativeVelocity.magnitude;
                if (impactSpeed < m_MinDamageVelocity)
                {
                    return; // 速度が足りない場合はダメージなし
                }
            }

            m_HealthModule.TakeDamage(damage, source);
        }
    }
}
