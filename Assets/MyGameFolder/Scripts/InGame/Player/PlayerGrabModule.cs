using UnityEngine;
using InGame.Gimmick;
using InGame.Entity;
using InGame.Manager;

namespace InGame.Player
{
    /// <summary>
    /// プレイヤーの掴み機能を担当するモジュール
    /// 掴む側（Grabber）として物理制御を行う
    /// </summary>
    public class PlayerGrabModule : PlayerModuleBase
    {
        private PlayerInputModule m_PlayerInputModule;

        private IGrabbable m_CurrentGrabTarget = null;
        private TargetJoint2D m_CurrentTargetJoint = null;
        private LineRenderer m_ConnectLine = null;

        private bool m_IsDragging = false;
        private Vector2? m_DragPosition = null;
        private Vector2? m_LastDragPosition = null;

        [SerializeField, Header("掴み判定の最大距離（プレイヤーからの距離）")]
        private float m_GrabRange = 5f;

        [SerializeField, Header("スラム時の下向き初速")]
        private float m_SlamVelocity = 20f;

        [SerializeField, Header("叩きつけパワー（AddForceの強さ）")]
        private float m_SlamPower = 30f;

        [SerializeField, Header("掴み時の放物線の高さ")]
        private float m_ParabolaHeight = 2.5f;

        [SerializeField, Header("スワイプ勢い倍率")]
        private float m_SwipeIntensityMultiplier = 2f;

        [SerializeField, Header("スワイプ勢いの最小値")]
        private float m_SwipeIntensityMin = 1f;

        [SerializeField, Header("スワイプ勢いの最大値")]
        private float m_SwipeIntensityMax = 5f;

        [SerializeField, Header("TargetJoint2Dの最大力")]
        private float m_JointMaxForce = 1000f;

        [SerializeField, Header("TargetJoint2Dの周波数")]
        private float m_JointFrequency = 2.0f;

        [SerializeField, Header("TargetJoint2Dの減衰比")]
        private float m_JointDampingRatio = 0.7f;

        public override void Setup(EntityController controller)
        {
            base.Setup(controller);
            m_PlayerController.TryGetModule(out m_PlayerInputModule);
        }

        public override void StartModule() { }

        public override void UpdateModule()
        {
            // 行動不可状態なら掴み操作をスキップ
            if (!m_PlayerController.CanPerformAction(Entity.ActionCategory.Grab))
            {
                // 掴み中なら強制解放
                if (m_IsDragging)
                {
                    ForceRelease();
                }
                return;
            }

            if (m_PlayerInputModule.GetDrag(out Vector2 dragPos))
            {
                if (!m_IsDragging)
                {
                    OnDragStart(dragPos);
                }
                else
                {
                    m_DragPosition = dragPos;
                }
            }
            else
            {
                if (m_IsDragging)
                {
                    OnDragEnd();
                }
            }
        }

        /// <summary>
        /// 掴みを強制解放（死亡時など）
        /// </summary>
        private void ForceRelease()
        {
            if (m_CurrentTargetJoint != null)
            {
                m_CurrentTargetJoint.enabled = false;
                m_CurrentTargetJoint = null;
            }

            if (m_CurrentGrabTarget != null)
            {
                m_CurrentGrabTarget.OnReleased(m_PlayerController);
                m_CurrentGrabTarget = null;
            }

            m_IsDragging = false;
            m_DragPosition = null;
            m_LastDragPosition = null;
        }

        private void OnDragStart(Vector2 dragPos)
        {
            m_IsDragging = true;
            m_DragPosition = dragPos;
            m_LastDragPosition = dragPos;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(dragPos.x, dragPos.y, 0f));
            Vector2 point = new Vector2(worldPos.x, worldPos.y);

            Collider2D hit = Physics2D.OverlapPoint(point);
            if (hit == null) return;

            var grabbable = hit.GetComponent<IGrabbable>();
            if (grabbable == null) return;

            // 既に誰かに掴まれている場合はスキップ
            if (grabbable.IsGrabbed) return;

            // 重さチェック - プレイヤーのレベルに応じた持ち上げ力で判定
            var levelManager = ServiceLocator.Service<LevelManager>();
            if (levelManager != null && !levelManager.CanLift(grabbable.Weight))
            {
                AppLogger.Log($"Cannot lift object: weight {grabbable.Weight} > lift capacity {levelManager.CurrentLiftCapacity}");
                return;
            }

            float distance = Vector2.Distance(m_PlayerController.transform.position, hit.transform.position);;
            bool isAbovePlayer = hit.transform.position.y > m_PlayerController.transform.position.y;

            if (distance > m_GrabRange || !isAbovePlayer)
            {
                return;
            }

            // 掴み開始
            m_CurrentGrabTarget = grabbable;
            m_CurrentGrabTarget.OnGrabbed(m_PlayerController);

            // TargetJoint2Dをセットアップ
            SetupTargetJoint();
        }

        private void SetupTargetJoint()
        {
            if (m_CurrentGrabTarget?.Rigidbody == null) return;

            var targetObj = m_CurrentGrabTarget.GrabbableObject;
            m_CurrentTargetJoint = targetObj.GetComponent<TargetJoint2D>();
            if (m_CurrentTargetJoint == null)
            {
                m_CurrentTargetJoint = targetObj.AddComponent<TargetJoint2D>();
            }

            m_CurrentTargetJoint.enabled = true;
            m_CurrentTargetJoint.autoConfigureTarget = false;
            m_CurrentTargetJoint.target = targetObj.transform.position;
            m_CurrentTargetJoint.maxForce = m_JointMaxForce;
            m_CurrentTargetJoint.frequency = m_JointFrequency;
            m_CurrentTargetJoint.dampingRatio = m_JointDampingRatio;
        }

        private void OnDragEnd()
        {
            m_IsDragging = false;
            m_DragPosition = null;
            m_LastDragPosition = null;

            // TargetJoint2Dを無効化
            if (m_CurrentTargetJoint != null)
            {
                m_CurrentTargetJoint.enabled = false;
                m_CurrentTargetJoint = null;
            }

            // 掴み解除
            if (m_CurrentGrabTarget != null)
            {
                m_CurrentGrabTarget.OnReleased(m_PlayerController);
                m_CurrentGrabTarget = null;
            }

            // ラインを非表示
            if (m_ConnectLine != null)
            {
                m_ConnectLine.positionCount = 0;
            }
        }

        public override void FixedUpdateModule()
        {
            if (m_CurrentTargetJoint == null || !m_IsDragging || !m_DragPosition.HasValue)
            {
                return;
            }

            // スラム処理（下方向への勢いよいスワイプ）
            ProcessSlamInput();

            // 掴み中の位置更新
            UpdateGrabPosition();

            // ライン描画
            UpdateConnectLine();

            m_LastDragPosition = m_DragPosition;
        }

        private void ProcessSlamInput()
        {
            if (!m_LastDragPosition.HasValue) return;
            if (m_CurrentGrabTarget?.Rigidbody == null) return;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(m_DragPosition.Value.x, m_DragPosition.Value.y, 0f));
            Vector3 lastWorld = Camera.main.ScreenToWorldPoint(new Vector3(m_LastDragPosition.Value.x, m_LastDragPosition.Value.y, 0f));

            Vector2 delta = (Vector2)mouseWorld - (Vector2)lastWorld;

            if (delta.y < -0.75f)
            {
                var rb = m_CurrentGrabTarget.Rigidbody;
                float swipeIntensity = Mathf.Clamp(Mathf.Abs(delta.y) * m_SwipeIntensityMultiplier, m_SwipeIntensityMin, m_SwipeIntensityMax);

                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -m_SlamVelocity);
                rb.AddForce(Vector2.down * m_SlamPower * swipeIntensity, ForceMode2D.Impulse);

                AppLogger.Log($"Slam applied: velocity={rb.linearVelocity.y}");
            }
        }

        private void UpdateGrabPosition()
        {
            Vector2 playerPos = m_PlayerController.transform.position;
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(m_DragPosition.Value.x, m_DragPosition.Value.y, 0f));
            Vector2 handPos = new Vector2(mouseWorld.x, mouseWorld.y);

            // 範囲制限
            Vector2 offset = handPos - playerPos;
            if (offset.magnitude > m_GrabRange)
            {
                offset = offset.normalized * m_GrabRange;
                handPos = playerPos + offset;
            }

            m_CurrentTargetJoint.target = handPos;
        }

        private void UpdateConnectLine()
        {
            if (m_CurrentGrabTarget == null) return;

            if (m_ConnectLine == null)
            {
                CreateConnectLine();
            }

            Vector2 playerPos = m_PlayerController.transform.position;
            Vector2 endPos = m_CurrentGrabTarget.GrabbableObject.transform.position;

            var points = CalculateParabolaPoints(playerPos, endPos, m_ParabolaHeight, 30);
            m_ConnectLine.positionCount = points.Length;
            m_ConnectLine.SetPositions(points);
        }

        private void CreateConnectLine()
        {
            var go = new GameObject("ConnectLine");
            m_ConnectLine = go.AddComponent<LineRenderer>();
            m_ConnectLine.material = new Material(Shader.Find("Sprites/Default"));
            m_ConnectLine.widthMultiplier = 0.07f;
            m_ConnectLine.startColor = Color.cyan;
            m_ConnectLine.endColor = Color.cyan;
        }

        private Vector3[] CalculateParabolaPoints(Vector2 startPos, Vector2 endPos, float desiredHeight, int pointCount)
        {
            Vector3[] points = new Vector3[pointCount];
            float gravity = Mathf.Abs(Physics2D.gravity.y);

            float dx = endPos.x - startPos.x;
            float peakY = Mathf.Max(startPos.y, endPos.y) + desiredHeight;

            float t_peak = Mathf.Sqrt(2f * (peakY - startPos.y) / gravity);
            float t_end = t_peak + Mathf.Sqrt(2f * (peakY - endPos.y) / gravity);

            float vx = dx / t_end;
            float vy = gravity * t_peak;
            Vector2 velocity = new Vector2(vx, vy);

            float timeStep = t_end / pointCount;

            for (int i = 0; i < pointCount; i++)
            {
                float t = i * timeStep;
                float x = startPos.x + velocity.x * t;
                float y = startPos.y + velocity.y * t + 0.5f * Physics2D.gravity.y * t * t;
                points[i] = new Vector3(x, y, 0f);
            }

            points[pointCount - 1] = new Vector3(endPos.x, endPos.y, 0f);
            return points;
        }

        public override void DestroyModule()
        {
            if (m_ConnectLine != null)
            {
                Destroy(m_ConnectLine.gameObject);
            }
        }
    }
}
