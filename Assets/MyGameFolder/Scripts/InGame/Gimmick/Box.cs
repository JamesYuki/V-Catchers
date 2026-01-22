using UnityEngine;
using InGame.Player;

namespace InGame.Gimmick
{

    public class Box : MonoBehaviour, IGimmick, IGrabable, IDamageable
    {
        [SerializeField]
        private int m_MaxHP = 3;
        private int m_CurrentHP;

        private void Awake()
        {
            m_CurrentHP = m_MaxHP;
        }
        public void Activate()
        {
            // ギミックがアクティブになったときの処理
        }

        public void Deactivate()
        {
            // ギミックが非アクティブになったときの処理
        }

        public Vector3 GetGrapplePoint()
        {
            // グラップルポイントの位置を返す
            return transform.position;
        }
        public void Grap(PlayerController player)
        {
            // プレイヤーの現在のドラッグ位置（マウス位置）にBoxを移動させる
            // PlayerControllerにドラッグ座標(Vector2? DragPosition)プロパティがある前提
            PlayerInputModule inputModule;
            player.GetModule(out inputModule);
            if (inputModule != null)
            {
                Vector2 dragPos;
                if (inputModule.GetDrag(out dragPos))
                {
                    // カメラからワールド座標に変換
                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(dragPos.x, dragPos.y, 0f));
                    worldPos.z = transform.position.z; // 2Dなのでzは維持
                    transform.position = worldPos;
                }
            }
        }

        public void TakeDamage(int amount)
        {
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
            if (m_SpawnerGimmickPrefab != null)
            {
                for (int i = 0; i < m_SpawnCount; i++)
                {
                    // 箱の中心からスポーン
                    var obj = Instantiate(m_SpawnerGimmickPrefab, transform.position, Quaternion.identity);
                    var rb = obj.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        // ランダムな方向に力を加える
                        float angle = Random.Range(0, Mathf.PI * 2f);
                        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.7f + 0.7f).normalized;
                        rb.AddForce(dir * m_SpawnForce, ForceMode2D.Impulse);
                    }
                }
            }
            Destroy(gameObject);
        }

        // グラップ中かどうかのフラグ
        public bool IsGrabed { get; set; } = false;

        // 衝突時のダメージ判定
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // "Ground"タグの床に衝突した場合のみ
            if (collision.collider.CompareTag(Tags.Ground))
            {
                // グラップ中かつ、下向き速度が一定以上
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                AppLogger.Log("Box collided with Ground" + rb?.linearVelocity.magnitude);

                if (IsGrabed && rb != null)
                {
                    TakeDamage(1);
                }
            }
        }
    }
}
