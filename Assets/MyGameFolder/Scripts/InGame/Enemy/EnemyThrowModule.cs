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
        [SerializeField]
        private float m_ThrowForce = 15f;

        [SerializeField]
        private float m_LiftHeight = 2f;
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
                Vector2 throwDir = ((Vector2)targetPos - (Vector2)liftPos).normalized;
                float throwPower = m_ThrowForce;
                Vector2 velocity = throwDir * throwPower;
                float timeStep = 0.05f;
                int steps = 60;
                Vector3 prev = liftPos;
                for (int i = 1; i <= steps; i++)
                {
                    float t = i * timeStep;
                    Vector3 next = liftPos + (Vector3)velocity * t + 0.5f * (Vector3)(Physics2D.gravity) * t * t;
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(prev, next);
                    prev = next;
                }
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(prev, 0.15f);
            }
            else if (transform != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, m_SearchRadius);
            }
        }

        public EnemyThrowModule(Transform player, float searchRadius = 10f, float throwForce = 15f, float playerSearchRadius = 10f)
        {
            this.m_PlayerTransform = player;
            this.m_SearchRadius = searchRadius;
            this.m_ThrowForce = throwForce;
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
            float height = m_LiftHeight; // 山なり度
            Vector2 start = liftPos;
            Vector2 end = m_PlayerTransform.position;
            Vector2 velocity = CalculateParabola2D(start, end, height, Physics2D.gravity.y) * m_ThrowForce;
            Debug.Log($"[EnemyThrowModule] Throwing {obj.name} from {liftPos} to {end} with velocity {velocity} (AddForce: {velocity * rb2d.mass})");
            rb2d.AddForce(velocity * rb2d.mass, ForceMode2D.Impulse);

            await UniTask.Delay(250); // 少し待つ

            grabbable?.GrapEnd(m_Enemy); // グラッブ状態を解除
            m_ThrowHandlerDisposable.Dispose();

            // クールダウンタイマーをセット
            m_NextCanThrowTime = Time.time + m_ThrowCooldown;
        }

        // 2D放物線の初速計算
        private Vector2 CalculateParabola2D(Vector2 start, Vector2 end, float height, float gravity)
        {
            Vector2 displacement = end - start;
            float displacementY = displacement.y;
            float displacementX = displacement.x;
            float peakHeight = Mathf.Max(height, 0.1f);
            float g = gravity;
            float timeUp = Mathf.Sqrt(2 * peakHeight / -g);
            float timeDown = Mathf.Sqrt(2 * Mathf.Max(displacementY - peakHeight, 0.1f) / -g);
            float totalTime = timeUp + timeDown;
            float velocityY = timeUp * -g;
            float velocityX = displacementX / totalTime;
            return new Vector2(velocityX, velocityY);
        }
    }
}
