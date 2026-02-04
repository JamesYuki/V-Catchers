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

        [SerializeField, Header("重さ設定")]
        [Tooltip("オブジェクトの重さ（持ち上げに必要なレベルの指標）")]
        private float m_Weight = 1f;

        [SerializeField, Tooltip("物理的な質量（Rigidbody2Dのmassに反映）")]
        private float m_PhysicsMass = 5f;

        [SerializeField]
        private Vector3 m_Velocity;
        public Vector3 Velocity
        {
            get => m_Velocity;
            set => m_Velocity = value;
        }

        public bool IsGrabed { get; set; } // グラッブ中かどうかのフラグ

        private Rigidbody2D m_Rigidbody2D;

        // IGrabbable実装
        public bool IsGrabbed => m_CurrentGrabber != null;
        public IGrabber CurrentGrabber => m_CurrentGrabber;
        public Rigidbody2D Rigidbody => m_Rigidbody2D;
        public GameObject GrabbableObject => gameObject;
        public float Weight => m_Weight;
        public float PhysicsMass => m_PhysicsMass * Weight;

        // IDamageable実装
        public int CurrentHealth => m_CurrentHP;
        public int MaxHealth => m_MaxHP;
        public bool IsAlive => m_CurrentHP > 0;
        public float HealthRatio => m_MaxHP > 0 ? (float)m_CurrentHP / m_MaxHP : 0f;

        private IGrabber m_CurrentGrabber;
        private IDisposable m_HasBeenGrappledHandler;
        private UseRefCounter m_HasBeenGrappled = new(); // グラッブ ~ 地面に設置までの参照カウンター

        private void Awake()
        {
            m_CurrentHP = m_MaxHP;
            m_Rigidbody2D = GetComponent<Rigidbody2D>();

            // 物理的な質量をRigidbody2Dに反映
            if (m_Rigidbody2D != null)
            {
                m_Rigidbody2D.mass = m_PhysicsMass;
            }
        }

        public void OnGrabbed(IGrabber grabber)
        {
            if (m_CurrentGrabber != null)
            {
                // 既に誰かに掴まれている場合は先に解放
                OnReleased(m_CurrentGrabber);
            }

            m_CurrentGrabber = grabber;
            IsGrabed = true;
            SetGrabLayer(Layers.GrabObject);

            // 掴まれている間は物理演算を一時的に調整
            if (m_Rigidbody2D != null)
            {
                m_Rigidbody2D.gravityScale = 0f;
            }

            if (!m_HasBeenGrappled.IsUsed)
            {
                m_HasBeenGrappledHandler = m_HasBeenGrappled.Use();
                m_DisposableGroup.Add(Disposable.Create(() => m_HasBeenGrappledHandler?.Dispose()));
            }

            AppLogger.Log($"Box grabbed by {grabber.GrabberObject.name}");
        }

        public void OnReleased(IGrabber grabber)
        {
            if (m_CurrentGrabber != grabber) return;

            m_CurrentGrabber = null;
            IsGrabed = false;
            RestoreLayer();

            // 物理演算を元に戻す
            if (m_Rigidbody2D != null)
            {
                m_Rigidbody2D.gravityScale = 1f;
            }

            AppLogger.Log($"Box released by {grabber.GrabberObject.name}");
        }

        private void FixedUpdate()
        {
            if (m_Rigidbody2D == null)
            {
                return;
            }

            AppLogger.Log($"Box FixedUpdate: {IsGrabed}, HasBeenGrappled Count: {m_HasBeenGrappled.Count}");

            if (IsGrabed) // 念のためグラッブ中は早期return
            {
                return;
            }

            // Velocityを更新
            Velocity = m_Rigidbody2D.linearVelocity;
            AppLogger.Log($"Box Velocity: {Velocity}");
        }

        /// <summary>
        /// IDamageable実装 - ダメージを受ける
        /// </summary>
        public void TakeDamage(int damage, GameObject damageSource = null)
        {
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
                AppLogger.Log($"Box damage blocked: impact velocity {impactVelocity:F2} < threshold {m_DamageVelocityThreshold:F2}");
                return;
            }

            AppLogger.Log($"Box taking damage: impact velocity {impactVelocity:F2} >= threshold {m_DamageVelocityThreshold:F2}");
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
            // relativeVelocityを使用して相対速度で判定（投げつけた時にも壊れるように）
            float impactVelocity = collision.relativeVelocity.magnitude;

            // レイヤーに関係なく、速度が十分な衝突で壊れる
            AppLogger.Log($"Box collision with {collision.gameObject.name}, impact velocity: {impactVelocity:F2}");
            TakeDamageWithVelocity(1, impactVelocity);

            if (!IsGrabed)
            {
                m_HasBeenGrappledHandler?.Dispose();
            }
        }
    }
}
