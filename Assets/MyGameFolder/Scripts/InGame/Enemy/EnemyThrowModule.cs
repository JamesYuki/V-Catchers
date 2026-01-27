using Cysharp.Threading.Tasks;
using DG.Tweening;
using InGame.Gimmick;
using UnityEngine;
using System;

namespace MyGame.InGame.Enemy
{
    public class EnemyThrowModule : MonoBehaviour, IEnemyModule
    {
        private EnemyBase m_Enemy;
        [SerializeField]
        private float m_SearchRadius = 10f; // 箱探索用
        [SerializeField]
        private float m_PlayerSearchRadius = 10f; // プレイヤー探索用
        [SerializeField, Tooltip("投げる速度の倍率")]
        private float m_ThrowSpeed = 1f;

        [SerializeField, Tooltip("持ち上げる高さ")]
        private float m_LiftHeight = 2f;

        [SerializeField, Tooltip("投げる山なりの高さ（放物線の頂点高さ）")]
        private float m_ThrowArcHeight = 3f;
        [SerializeField]
        private float m_LiftTime = 1.0f;
        private Transform m_PlayerTransform;
        // 直近の投げる方向を保存
        private Vector2? m_LastThrowDirection = null;

        private UseRefCounter m_ThrowHandler = new();
        private IDisposable m_ThrowHandlerDisposable;

        [SerializeField]
        private float m_ThrowCooldown = 2.0f; // 投げた後のクールダウン秒数
        private float m_NextCanThrowTime = 0f;

        private void OnDrawGizmosSelected()
        {
            if (m_Enemy != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(m_Enemy.transform.position, m_SearchRadius);

                // 軌道予測を可視化
                Vector3 liftPos = m_Enemy.transform.position + new Vector3(0, m_LiftHeight, 0);
                Vector3 targetPos = m_PlayerTransform != null ? m_PlayerTransform.position : liftPos + Vector3.right * 5f;
                Vector2 velocity = CalculateParabola2D(liftPos, targetPos, m_ThrowArcHeight, Physics2D.gravity.y) * m_ThrowSpeed;
                float timeStep = 0.05f;
                int steps = 80;
                Vector3 prev = liftPos;
                for (int i = 1; i <= steps; i++)
                {
                    float t = i * timeStep;
                    Vector3 next = liftPos + (Vector3)velocity * t + 0.5f * (Vector3)(Physics2D.gravity) * t * t;
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(prev, next);
                    prev = next;
                    // ターゲットより下に行ったら止める
                    if (next.y < targetPos.y - 1f) break;
                }
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(targetPos, 0.2f);
            }
            else if (transform != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, m_SearchRadius);
            }
        }

        public EnemyThrowModule(Transform player, float searchRadius = 10f, float throwSpeed = 1f, float playerSearchRadius = 10f)
        {
            this.m_PlayerTransform = player;
            this.m_SearchRadius = searchRadius;
            this.m_ThrowSpeed = throwSpeed;
            this.m_PlayerSearchRadius = playerSearchRadius;
        }

        public void OnModuleAdded(EnemyBase enemy)
        {
            this.m_Enemy = enemy;
            m_ThrowHandler = new();
        }

        public void OnModuleRemoved(EnemyBase enemy)
        {
            this.m_Enemy = null;
            m_ThrowHandlerDisposable?.Dispose();
        }

        public void OnEnemyUpdate()
        {
            if (m_Enemy == null) return;

            // 自動でプレイヤーを範囲内から探す
            if (m_PlayerTransform == null)
            {
                Vector2 enemyPos2D = m_Enemy.transform.position;
                Collider2D[] playerColliders = Physics2D.OverlapCircleAll(enemyPos2D, m_PlayerSearchRadius);
                Debug.Log($"[EnemyThrowModule] PlayerSearchRadius内で取得したCollider2D数: {playerColliders.Length}");
                foreach (var col in playerColliders)
                {
                    Debug.Log($"[EnemyThrowModule] Player探索: {col.name} (tag: {col.tag})");
                    // ここでタグ判定しているが、今回は全てログ出力
                    if (col.CompareTag(Tags.Player))
                    {
                        m_PlayerTransform = col.transform;
                        break;
                    }
                }
                if (m_PlayerTransform == null) return;
            }


            // クールダウン中は持てない
            if (Time.time < m_NextCanThrowTime) return;
            if (m_ThrowHandler?.IsUsed ?? false) return;

            // Find nearest IGrabbable object
            IGrabbable nearest = null;
            float minDist = float.MaxValue;
            Vector2 enemyPos2DBox = m_Enemy.transform.position;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(enemyPos2DBox, m_SearchRadius);
            Debug.Log($"[EnemyThrowModule] BoxSearchRadius内で取得したCollider2D数: {colliders.Length}");
            foreach (var col in colliders)
            {
                Debug.Log($"[EnemyThrowModule] Box探索: {col.name} (tag: {col.tag})");
                var grabbable = col.GetComponent<IGrabbable>();
                if (grabbable != null)
                {
                    float dist = Vector2.Distance(enemyPos2DBox, col.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = grabbable;
                    }
                }
            }

            if (nearest != null)
            {
                ThrowAtPlayer(nearest);
            }
        }

        private async void ThrowAtPlayer(IGrabbable grabbable)
        {
            if (grabbable == null) return;
            if (grabbable.IsGrabed) return;
            var obj = (grabbable as MonoBehaviour)?.gameObject;
            if (obj == null) return;
            Rigidbody2D rb2d = obj.GetComponent<Rigidbody2D>();
            if (rb2d == null) return;

            m_ThrowHandler = new();
            m_ThrowHandlerDisposable = m_ThrowHandler.Use();

            grabbable.GrapStart(m_Enemy);

            // 1. 持ち上げる（敵の頭上に移動）
            Vector3 liftPos = m_Enemy.transform.position + new Vector3(0, m_LiftHeight, 0); // m_LiftHeightは持ち上げ高さ
            grabbable.Grap(m_Enemy); // グラッブ状態にする

            obj.transform.DOMove(liftPos, m_LiftTime).SetEase(Ease.OutQuad);
            await UniTask.WaitForSeconds(m_LiftTime);

            // 2. プレイヤー位置まで放物線で届く初速を計算
            Vector2 start = liftPos;
            Vector2 end = m_PlayerTransform.position;
            Vector2 velocity = CalculateParabola2D(start, end, m_ThrowArcHeight, -9.81f) * m_ThrowSpeed;
            Debug.Log($"[EnemyThrowModule] Throwing {obj.name} from {start} to {end} with velocity {velocity}");
            rb2d.linearVelocity = velocity; // 直接velocityをセット（より正確な放物線）

            await UniTask.Delay(250); // 少し待つ

            grabbable?.GrapEnd(m_Enemy); // グラッブ状態を解除
            m_ThrowHandlerDisposable.Dispose();

            // クールダウンタイマーをセット
            m_NextCanThrowTime = Time.time + m_ThrowCooldown;
        }

        /// <summary>
        /// 2D放物線の初速を計算する
        /// </summary>
        /// <param name="start">開始位置</param>
        /// <param name="end">目標位置</param>
        /// <param name="arcHeight">放物線の頂点高さ（startからの相対高さ）</param>
        /// <param name="gravity">重力（負の値）</param>
        /// <returns>初速ベクトル</returns>
        private Vector2 CalculateParabola2D(Vector2 start, Vector2 end, float arcHeight, float gravity)
        {
            Vector2 displacement = end - start;
            float dx = displacement.x;
            float dy = displacement.y;
            float h = Mathf.Max(arcHeight, Mathf.Max(dy, 0) + 0.5f);
            float g = Mathf.Abs(gravity);
            float tUp = Mathf.Sqrt(2f * h / g);
            float fallHeight = h - dy;
            float tDown = Mathf.Sqrt(2f * Mathf.Max(fallHeight, 0.01f) / g);
            float totalTime = tUp + tDown;

            // 初速計算
            float vy = Mathf.Sqrt(2f * g * h); // 上向き初速
            float vx = dx / totalTime;

            return new Vector2(vx, vy);
        }
    }
}
