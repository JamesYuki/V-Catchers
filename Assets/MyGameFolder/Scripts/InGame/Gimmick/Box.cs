using UnityEngine;
using R3;
using System;

namespace InGame.Gimmick
{

    public class Box : GimmickBase, IGrabbable, IDamageable
    {
        [SerializeField]
        private int m_MaxHP = 3;
        private int m_CurrentHP;

        [SerializeField]
        private float m_DamageVelocityThreshold = 10f;

        [SerializeField]
        private Vector3 m_Velocity;
        public Vector3 Velocity
        {
            get => m_Velocity;
            set => m_Velocity = value;
        }

        public bool IsGrabed { get; set; } // グラッブ中かどうかのフラグ

        private Rigidbody2D m_Rigidbody2D;
        private IDisposable m_HasBeenGrappledHandler;
        private UseRefCounter m_HasBeenGrappled = new(); // グラッブ ~ 地面に設置までの参照カウンター

        private void Awake()
        {
            m_CurrentHP = m_MaxHP;
            m_Rigidbody2D = GetComponent<Rigidbody2D>();
        }

        public void GrapStart(object grabber)
        {
            SetGrabLayer(Layers.GrabObject);
        }
        public void Grap(object grabber)
        {
            // IGrabberインターフェースを持つ場合のみドラッグ座標取得
            if (grabber is InGame.IGrabber iGrabber)
            {
                Vector2 dragPos;
                if (iGrabber.GetDrag(out dragPos))
                {
                    // カメラからワールド座標に変換
                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(new UnityEngine.Vector3(dragPos.x, dragPos.y, 0f));
                    worldPos.z = transform.position.z;
                    transform.position = worldPos;

                }
            }

            if (!m_HasBeenGrappled.IsUsed)
            {
                m_HasBeenGrappledHandler = m_HasBeenGrappled.Use();
                m_DisposableGroup.Add(Disposable.Create(() => m_HasBeenGrappledHandler.Dispose()));
            }
        }

        public void GrapEnd(object grabber)
        {
            SetGrabLayer(Layers.Default);
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

            m_Rigidbody2D.linearVelocity += new Vector2(0f, -9.81f) * Time.fixedDeltaTime;
            Velocity = m_Rigidbody2D.linearVelocity;
            AppLogger.Log($"Box Velocity: {Velocity}");
        }

        public void TakeDamage(int amount)
        {
            if (Velocity.magnitude <= m_DamageVelocityThreshold)
            {
                return;
            }

            m_CurrentHP -= amount;
            if (m_CurrentHP <= 0)
            {
                DestroyBox();
            }
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
            TakeDamage(1);

            if (!IsGrabed)
            {
                m_HasBeenGrappledHandler?.Dispose();
            }
        }
    }
}
