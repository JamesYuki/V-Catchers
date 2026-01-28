using UnityEngine;
using R3;
using System;
using InGame.Entity;

namespace InGame.Gimmick
{

    public class Box : GimmickBase, IGrabbable, IDamageable
    {
        [SerializeField]
        private int m_MaxHP = 3;
        private int m_CurrentHP;

        [SerializeField]
        private float m_DamageVelocityThreshold = 10f;

        private Rigidbody2D m_Rigidbody2D;

        // IGrabbable実装
        public bool IsGrabbed => m_CurrentGrabber != null;
        public IGrabber CurrentGrabber => m_CurrentGrabber;
        public Rigidbody2D Rigidbody => m_Rigidbody2D;
        public GameObject GrabbableObject => gameObject;

        // IDamageable実装
        public int CurrentHealth => m_CurrentHP;
        public int MaxHealth => m_MaxHP;
        public bool IsAlive => m_CurrentHP > 0;
        public float HealthRatio => m_MaxHP > 0 ? (float)m_CurrentHP / m_MaxHP : 0f;

        private IGrabber m_CurrentGrabber;

        private void Awake()
        {
            m_CurrentHP = m_MaxHP;
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
        }

        public void OnGrabbed(IGrabber grabber)
        {
            if (m_CurrentGrabber != null)
            {
                // 既に誰かに掴まれている場合は先に解放
                OnReleased(m_CurrentGrabber);
            }

            m_CurrentGrabber = grabber;
            SetGrabLayer(Layers.GrabObject);

            // 掴まれている間は物理演算を一時的に調整
            if (m_Rigidbody2D != null)
            {
                m_Rigidbody2D.gravityScale = 0f;
            }

            AppLogger.Log($"Box grabbed by {grabber.GrabberObject.name}");
        }

        public void OnReleased(IGrabber grabber)
        {
            if (m_CurrentGrabber != grabber) return;

            m_CurrentGrabber = null;
            RestoreLayer();

            // 物理演算を元に戻す
            if (m_Rigidbody2D != null)
            {
                m_Rigidbody2D.gravityScale = 1f;
            }

            AppLogger.Log($"Box released by {grabber.GrabberObject.name}");
        }

        /// <summary>
        /// IDamageable実装 - ダメージを受ける
        /// </summary>
        public void TakeDamage(int damage, GameObject damageSource = null)
        {
            // 衝突速度による判定が必要な場合はDamageReceiverを使用
            // ここでは直接ダメージを受け付ける
            m_CurrentHP -= damage;
            AppLogger.Log($"Box took {damage} damage. HP: {m_CurrentHP}/{m_MaxHP}");

            if (m_CurrentHP <= 0)
            {
                DestroyBox();
            }
        }

        /// <summary>
        /// 速度付きダメージ処理（衝突時に使用）
        /// </summary>
        public void TakeDamageWithVelocity(int amount, float impactVelocity)
        {
            if (impactVelocity < m_DamageVelocityThreshold)
            {
                return;
            }

            TakeDamage(amount);
        }

        [SerializeField]
        private GameObject m_SpawnerGimmickPrefab;
        [SerializeField]
        private int m_SpawnCount = 5;
        [SerializeField]
        private float m_SpawnForce = 7f;

        private void DestroyBox()
        {
            // 破壊時にSpawnerGimmickを飛び散らせる
            if (m_SpawnerGimmickPrefab == null)
            {
                Deactivate();
                return;
            }
            for (int i = 0; i < m_SpawnCount; i++)
            {
                // 箱の中心からスポーン
                var obj = Instantiate(m_SpawnerGimmickPrefab, transform.position, Quaternion.identity);
                var rb = obj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // ランダムな方向に力を加える
                    float angle = UnityEngine.Random.Range(0, Mathf.PI * 2f);
                    Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.7f + 0.7f).normalized;
                    rb.AddForce(dir * m_SpawnForce, ForceMode2D.Impulse);
                }
            }
            Deactivate();
        }

        // 衝突時のダメージ判定
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!IsGrabbed && m_Rigidbody2D != null)
            {
                float impactVelocity = m_Rigidbody2D.linearVelocity.magnitude;
                TakeDamageWithVelocity(1, impactVelocity);
            }
        }
    }
}
