
using UnityEngine;
using InGame.Gimmick;

namespace InGame.Player
{
    public class PlayerGrabModule : MonoBehaviour, IPlayerModule
    {
        private PlayerController m_PlayerController;
        private PlayerInputModule m_PlayerInputModule;
        private Vector2? m_DragPosition = null;
        private IGrabbable m_CurrentGrabTarget = null;
        private TargetJoint2D m_CurrentTargetJoint = null;
        private Vector2 m_TargetOffset = Vector2.zero;
        private LineRenderer m_ConnectLine = null;
        private int m_OriginalLayer = -1;
        private bool m_IsDragging = false;
        private Vector2? m_LastDragPosition = null; // 前フレームのドラッグ座標

        [SerializeField, Header("掴み判定の最大距離（プレイヤーからの距離）")]
        private float m_GrabRange = 5f;

        [SerializeField, Header("スラム時の下向き初速（叩きつけ時の初期速度）")]
        private float m_SlamVelocity = 20f;

        [SerializeField, Header("叩きつけパワー（AddForceの強さ）")]
        private float m_SlamPower = 30f;

        [SerializeField, Header("掴み時の放物線の高さ（なだらかさ）")]
        private float m_ParabolaHeight = 2.5f;
        [SerializeField, Header("スワイプ勢い倍率（delta.y × 倍率）")]
        private float m_SwipeIntensityMultiplier = 2f;
        [SerializeField, Header("スワイプ勢いの最小値")]
        private float m_SwipeIntensityMin = 1f;
        [SerializeField, Header("スワイプ勢いの最大値")]
        private float m_SwipeIntensityMax = 5f;
        public void Setup(PlayerController playerController)
        {
            m_PlayerController = playerController;
            playerController.GetModule(out m_PlayerInputModule);
        }

        public void StartModule()
        {

        }

        public void UpdateModule()
        {
            // ドラッグ座標取得（例: PlayerInputModuleにGetDragPosition()があると仮定）
            if (m_PlayerInputModule.GetDrag(out Vector2 dragPos))
            {
                if (!m_IsDragging)
                {
                    // ドラッグ開始時
                    m_IsDragging = true;
                    m_DragPosition = dragPos;
                    m_LastDragPosition = dragPos;
                    // カメラからワールド座標に変換
                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(dragPos.x, dragPos.y, 0f));
                    Vector2 point = new Vector2(worldPos.x, worldPos.y);
                    Collider2D hit = Physics2D.OverlapPoint(point);
                    if (hit != null)
                    {
                        var grabable = hit.GetComponent<IGrabbable>();
                        if (grabable != null)
                        {
                            grabable.GrapStart(m_PlayerController);
                            float distance = Vector2.Distance(m_PlayerController.transform.position, hit.transform.position);
                            if (distance <= m_GrabRange && hit.transform.position.y > m_PlayerController.transform.position.y)
                            {
                                m_CurrentGrabTarget = grabable;
                                var box = m_CurrentGrabTarget as InGame.Gimmick.Box;
                                if (box != null) box.IsGrabed = true;
                                // TargetJoint2Dを付与・設定
                                var targetMono = m_CurrentGrabTarget as MonoBehaviour;
                                if (targetMono != null)
                                {
                                    var rb = targetMono.GetComponent<Rigidbody2D>();
                                    if (rb != null)
                                    {
                                        m_CurrentTargetJoint = targetMono.GetComponent<TargetJoint2D>();
                                        if (m_CurrentTargetJoint == null)
                                            m_CurrentTargetJoint = targetMono.gameObject.AddComponent<TargetJoint2D>();
                                        m_CurrentTargetJoint.enabled = true;
                                        m_CurrentTargetJoint.autoConfigureTarget = false;
                                        // 掴み開始時のプレイヤーと対象の相対位置を記録
                                        m_TargetOffset = rb.position - (Vector2)m_PlayerController.transform.position;
                                        m_CurrentTargetJoint.target = m_PlayerController.transform.position + (Vector3)m_TargetOffset;
                                        m_CurrentTargetJoint.maxForce = 1000f;
                                        m_CurrentTargetJoint.frequency = 2.0f;
                                        m_CurrentTargetJoint.dampingRatio = 0.7f;
                                    }
                                }
                            }
                            else
                            {
                                m_CurrentGrabTarget?.GrapEnd(m_PlayerController);
                                m_CurrentGrabTarget = null;
                            }
                        }
                    }
                }
                else
                {
                    // ドラッグ中
                    m_DragPosition = dragPos;
                }
            }
            else
            {
                // ドラッグ終了
                m_DragPosition = null;
                m_LastDragPosition = null;
                m_IsDragging = false;
                // TargetJoint2Dを無効化
                if (m_CurrentTargetJoint != null)
                {
                    m_CurrentTargetJoint.enabled = false;
                    m_CurrentTargetJoint = null;
                }
                // 掴み解除時のAddForce/velocity強制は行わない
                // BoxならIsGrabedをfalseに
                var boxRelease = m_CurrentGrabTarget as InGame.Gimmick.Box;
                if (boxRelease != null) boxRelease.IsGrabed = false;
                // レイヤーを元に戻す
                // GimmickBaseのRestoreLayerで元に戻す
                var gimmickRelease = m_CurrentGrabTarget as InGame.Gimmick.GimmickBase;
                if (gimmickRelease != null)
                {
                    gimmickRelease.RestoreLayer();
                }
                m_CurrentGrabTarget = null;
            }
        }

        public void FixedUpdateModule()
        {
            // 掴み中に下方向へ勢いよくマウスを動かしたときのみAddForce/velocityを与える
            if (m_CurrentTargetJoint != null && m_IsDragging && m_DragPosition.HasValue && m_LastDragPosition.HasValue)
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(m_DragPosition.Value.x, m_DragPosition.Value.y, 0f));
                Vector2 handPos = new Vector2(mouseWorld.x, mouseWorld.y);
                Vector3 lastWorld = Camera.main.ScreenToWorldPoint(new Vector3(m_LastDragPosition.Value.x, m_LastDragPosition.Value.y, 0f));
                Vector2 lastHandPos = new Vector2(lastWorld.x, lastWorld.y);
                Vector2 delta = handPos - lastHandPos;
                if (delta.y < -0.75f)
                {
                    var targetMono = m_CurrentGrabTarget as MonoBehaviour;
                    if (targetMono != null)
                    {
                        var rb = targetMono.GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            // 勢いを出すために、より強い下方向への初速とマウスのスワイプ速度に応じた追加の力を与える
                            float swipeIntensity = Mathf.Clamp(Mathf.Abs(delta.y) * m_SwipeIntensityMultiplier, m_SwipeIntensityMin, m_SwipeIntensityMax);
                            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -m_SlamVelocity);
                            rb.AddForce(Vector2.down * m_SlamPower * swipeIntensity, ForceMode2D.Impulse);
                            AppLogger.Log("Applied slam force: " + rb.linearVelocity.y);
                            // IGrabableのVelocityプロパティにrbの速度をセット
                            if (m_CurrentGrabTarget != null)
                            {
                                m_CurrentGrabTarget.Velocity = rb.linearVelocity;
                            }
                        }
                    }
                }
            }

            // ドラッグ中はマウスの位置で掴んだ物体を動かす
            if (m_CurrentTargetJoint != null && m_IsDragging && m_DragPosition.HasValue)
            {
                // 掴み中は毎フレーム速度を更新
                if (m_CurrentGrabTarget != null)
                {
                    var targetMono = m_CurrentGrabTarget as MonoBehaviour;
                    if (targetMono != null)
                    {
                        var rb = targetMono.GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            m_CurrentGrabTarget.Velocity = rb.linearVelocity;
                        }
                    }
                }
                Vector2 playerPos = m_PlayerController.transform.position;
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(m_DragPosition.Value.x, m_DragPosition.Value.y, 0f));
                Vector2 handPos = new Vector2(mouseWorld.x, mouseWorld.y);
                m_CurrentTargetJoint.target = handPos;
                Vector2 offset = m_CurrentTargetJoint.target - playerPos;
                if (offset.magnitude > m_GrabRange)
                {
                    offset = offset.normalized * m_GrabRange;
                    m_CurrentTargetJoint.target = playerPos + offset;
                }
                if (m_ConnectLine == null)
                {
                    var go = new GameObject("ConnectLine");
                    m_ConnectLine = go.AddComponent<LineRenderer>();
                    m_ConnectLine.material = new Material(Shader.Find("Sprites/Default"));
                    m_ConnectLine.widthMultiplier = 0.07f;
                    m_ConnectLine.startColor = Color.cyan;
                    m_ConnectLine.endColor = Color.cyan;
                }
                var targetMonoForLine = m_CurrentGrabTarget as MonoBehaviour;
                if (targetMonoForLine != null)
                {
                    Vector2 endPos = targetMonoForLine.transform.position;
                    float desiredHeight = m_ParabolaHeight;
                    Vector2 midPoint = (playerPos + endPos) * 0.5f;
                    float peakY = Mathf.Max(playerPos.y, endPos.y) + desiredHeight;
                    float dx = endPos.x - playerPos.x;
                    float dy = endPos.y - playerPos.y;
                    float gravity = Mathf.Abs(Physics2D.gravity.y);
                    float t_peak = Mathf.Sqrt(2f * (peakY - playerPos.y) / gravity);
                    float t_end = t_peak + Mathf.Sqrt(2f * (peakY - endPos.y) / gravity);
                    float vx = dx / t_end;
                    float vy = gravity * t_peak;
                    Vector2 velocity = new Vector2(vx, vy);
                    var points = CalculateParabolaPoints(playerPos, velocity, endPos, 30, t_end / 30f);
                    m_ConnectLine.positionCount = points.Length;
                    m_ConnectLine.SetPositions(points);
                }
                m_LastDragPosition = m_DragPosition;
            }
            else
            {
                if (m_CurrentGrabTarget != null)
                {
                    var targetMono = m_CurrentGrabTarget as MonoBehaviour;
                    if (targetMono != null)
                    {
                        var line = targetMono.GetComponent<LineRenderer>();
                        if (line != null)
                        {
                            line.positionCount = 0;
                        }
                    }
                }
                if (m_ConnectLine != null)
                {
                    m_ConnectLine.positionCount = 0;
                }
            }
        }

        public void DestroyModule()
        {
            m_PlayerController = null;
        }

        // プレイヤーから物体までの放物線座標リストを計算
        private Vector3[] CalculateParabolaPoints(Vector2 startPos, Vector2 velocity, Vector2 endPos, int pointCount = 30, float timeStep = 0.05f)
        {
            Vector3[] points = new Vector3[pointCount];
            float gravity = Physics2D.gravity.y;
            for (int i = 0; i < pointCount; i++)
            {
                float t = i * timeStep;
                float x = startPos.x + velocity.x * t;
                float y = startPos.y + velocity.y * t + 0.5f * gravity * t * t;
                points[i] = new Vector3(x, y, 0f);
            }
            // 最後の点は物体位置に
            points[pointCount - 1] = new Vector3(endPos.x, endPos.y, 0f);
            return points;
        }
    }
}
