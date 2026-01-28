using Cysharp.Threading.Tasks;
using DG.Tweening;
using InGame.Gimmick;
using InGame.Entity;
using UnityEngine;
using System;

namespace InGame.Enemy
{
    /// <summary>
    /// 敵が物を投げる機能を担当するモジュール
    /// 掴む側（Grabber）として物理制御を行う
    /// </summary>
    public class EnemyThrowModule : EnemyModuleBase
    {
        [SerializeField]
        private float m_SearchRadius = 10f;

        [SerializeField]
        private float m_PlayerSearchRadius = 10f;

        [SerializeField, Tooltip("投げる山なりの高さ（放物線の頂点高さ）")]
        private float m_ThrowArcHeight = 3f;

        [SerializeField, Tooltip("持ち上げる高さ")]
        private float m_LiftHeight = 2f;

        [SerializeField]
        private float m_LiftTime = 1.0f;

        [SerializeField]
        private float m_ThrowCooldown = 2.0f;

        [SerializeField, Tooltip("速度倍率（軌道は同じまま到達時間を短縮）"), Range(0.5f, 3f)]
        private float m_SpeedMultiplier = 1f;

        private Transform m_PlayerTransform;
        private float m_NextCanThrowTime = 0f;
        private IGrabbable m_CurrentGrabbable = null;

        // 投げた後のgravityScale復元用
        private float m_OriginalGravityScale = 1f;

        private void OnDrawGizmosSelected()
        {
            float baseGravity = Mathf.Abs(Physics2D.gravity.y);

            if (m_EnemyController != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(m_EnemyController.transform.position, m_SearchRadius);

                // 軌道予測を可視化
                Vector3 liftPos = m_EnemyController.transform.position + new Vector3(0, m_LiftHeight, 0);
                Vector3 targetPos = m_PlayerTransform != null ? m_PlayerTransform.position : liftPos + Vector3.right * 5f;

                // 速度倍率適用後の計算
                Vector2 baseVelocity = CalculateParabola2D(liftPos, targetPos, m_ThrowArcHeight, baseGravity);
                Vector2 velocity = baseVelocity * m_SpeedMultiplier;
                float effectiveGravity = baseGravity * m_SpeedMultiplier * m_SpeedMultiplier;

                float timeStep = 0.05f / m_SpeedMultiplier; // 速度が上がると到達時間が短くなる
                int steps = 80;
                Vector3 prev = liftPos;
                for (int i = 1; i <= steps; i++)
                {
                    float t = i * timeStep;
                    float x = liftPos.x + velocity.x * t;
                    float y = liftPos.y + velocity.y * t - 0.5f * effectiveGravity * t * t;
                    Vector3 next = new Vector3(x, y, 0f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(prev, next);
                    prev = next;
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

        public override void DestroyModule()
        {
            // 掴み中なら解放
            if (m_CurrentGrabbable != null)
            {
                m_CurrentGrabbable.OnReleased(m_EnemyController);
                m_CurrentGrabbable = null;
            }
        }

        public override void UpdateModule()
        {
            if (m_EnemyController == null) return;

            // プレイヤー探索
            if (m_PlayerTransform == null)
            {
                FindPlayer();
                if (m_PlayerTransform == null) return;
            }

            // クールダウン中または掴み中はスキップ
            if (Time.time < m_NextCanThrowTime) return;
            if (m_CurrentGrabbable != null) return;

            // 近くのIGrabbableを探索
            var grabbable = FindNearestGrabbable();
            if (grabbable != null)
            {
                ThrowAtPlayer(grabbable);
            }
        }

        private void FindPlayer()
        {
            Vector2 enemyPos2D = m_EnemyController.transform.position;
            Collider2D[] playerColliders = Physics2D.OverlapCircleAll(enemyPos2D, m_PlayerSearchRadius);

            foreach (var col in playerColliders)
            {
                if (col.CompareTag(Tags.Player))
                {
                    m_PlayerTransform = col.transform;
                    break;
                }
            }
        }

        private IGrabbable FindNearestGrabbable()
        {
            IGrabbable nearest = null;
            float minDist = float.MaxValue;
            Vector2 enemyPos2D = m_EnemyController.transform.position;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(enemyPos2D, m_SearchRadius);

            foreach (var col in colliders)
            {
                var grabbable = col.GetComponent<IGrabbable>();
                if (grabbable == null) continue;

                // 既に誰かに掴まれている場合はスキップ
                if (grabbable.IsGrabbed) continue;

                float dist = Vector2.Distance(enemyPos2D, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = grabbable;
                }
            }

            return nearest;
        }

        private async void ThrowAtPlayer(IGrabbable grabbable)
        {
            if (grabbable == null) return;
            if (grabbable.Rigidbody == null) return;

            m_CurrentGrabbable = grabbable;

            // 掴み開始
            grabbable.OnGrabbed(m_EnemyController);

            var rb = grabbable.Rigidbody;
            var obj = grabbable.GrabbableObject;

            // 元のgravityScaleを保存
            m_OriginalGravityScale = rb.gravityScale;

            // 1. 持ち上げる（敵の頭上に移動）
            Vector3 liftPos = m_EnemyController.transform.position + new Vector3(0, m_LiftHeight, 0);

            obj.transform.DOMove(liftPos, m_LiftTime).SetEase(Ease.OutQuad);
            await UniTask.WaitForSeconds(m_LiftTime);

            // 投げる直前に掴みを解除（物理演算を有効に）
            grabbable.OnReleased(m_EnemyController);
            m_CurrentGrabbable = null;

            // 2. プレイヤー位置まで放物線で届く初速を計算して投げる
            Vector2 start = liftPos;
            Vector2 end = m_PlayerTransform.position;
            float baseGravity = Mathf.Abs(Physics2D.gravity.y);

            // 基本の放物線速度を計算
            Vector2 baseVelocity = CalculateParabola2D(start, end, m_ThrowArcHeight, baseGravity);

            // 速度倍率を適用（軌道は同じまま速度を上げる）
            // 速度をk倍にするには、重力もk²倍にする必要がある
            Vector2 velocity = baseVelocity * m_SpeedMultiplier;
            float gravityScale = m_SpeedMultiplier * m_SpeedMultiplier;

            Debug.Log($"[EnemyThrowModule] Throwing {obj.name} from {start} to {end} with velocity {velocity}, gravityScale {gravityScale}");

            rb.gravityScale = gravityScale;
            rb.linearVelocity = velocity;

            // 着地後にgravityScaleを戻すためのコルーチン開始
            RestoreGravityOnLanding(rb, m_OriginalGravityScale).Forget();

            // クールダウンタイマーをセット
            m_NextCanThrowTime = Time.time + m_ThrowCooldown;
        }

        /// <summary>
        /// 着地（速度がほぼ0）または一定時間後にgravityScaleを元に戻す
        /// </summary>
        private async UniTaskVoid RestoreGravityOnLanding(Rigidbody2D rb, float originalGravityScale)
        {
            if (rb == null) return;

            float maxWaitTime = 5f; // 最大待機時間
            float elapsed = 0f;

            // 少し待ってから監視開始（投げた直後は速度がある）
            await UniTask.WaitForSeconds(0.5f);

            while (elapsed < maxWaitTime)
            {
                if (rb == null) return;

                // 速度がほぼ0になったら着地と判定
                if (rb.linearVelocity.magnitude < 0.1f)
                {
                    rb.gravityScale = originalGravityScale;
                    Debug.Log($"[EnemyThrowModule] Restored gravityScale to {originalGravityScale}");
                    return;
                }

                await UniTask.WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            // タイムアウト時も元に戻す
            if (rb != null)
            {
                rb.gravityScale = originalGravityScale;
            }
        }

        /// <summary>
        /// 2D放物線の初速を計算する
        /// </summary>
        /// <param name="start">開始位置</param>
        /// <param name="end">目標位置</param>
        /// <param name="arcHeight">放物線の頂点高さ（startからの相対高さ）</param>
        /// <param name="gravity">重力の絶対値</param>
        /// <returns>初速ベクトル</returns>
        private Vector2 CalculateParabola2D(Vector2 start, Vector2 end, float arcHeight, float gravity)
        {
            Vector2 displacement = end - start;
            float dx = displacement.x;
            float dy = displacement.y;

            // 頂点の高さ（開始位置からの相対高さ）
            // 終点が開始点より高い場合は、それに合わせて調整
            float h = Mathf.Max(arcHeight, Mathf.Max(dy, 0) + 0.5f);
            float g = Mathf.Abs(gravity);

            // 上昇時間: 頂点まで
            float tUp = Mathf.Sqrt(2f * h / g);

            // 下降時間: 頂点から終点まで
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
