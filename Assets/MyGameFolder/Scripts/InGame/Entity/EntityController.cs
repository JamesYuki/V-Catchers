using UnityEngine;
using InGame.Gimmick;
using InGame.Entity.State;

namespace InGame.Entity
{
    /// <summary>
    /// Player, Enemy等の共通コントローラー基底クラス
    /// モジュールのライフサイクル管理とステート管理を担当
    /// </summary>
    public abstract class EntityController : MonoBehaviour, IGrabber
    {
        [Header("ステート管理")]
        [SerializeField]
        protected EntityStateMachine m_StateMachine = new EntityStateMachine();

        protected IEntityModule[] m_Modules;

        // IGrabber実装
        public Vector3 GrabberPosition => transform.position;
        public GameObject GrabberObject => gameObject;

        /// <summary>
        /// ステートマシンへのアクセス
        /// </summary>
        public EntityStateMachine StateMachine => m_StateMachine;

        /// <summary>
        /// 現在のステートクラス
        /// </summary>
        public EntityState CurrentState => m_StateMachine.CurrentState;

        /// <summary>
        /// 行動可能かどうか
        /// </summary>
        public bool CanAct => m_StateMachine.CanAct;

        /// <summary>
        /// 生存しているか
        /// </summary>
        public bool IsAlive => m_StateMachine.IsAlive;

        /// <summary>
        /// 指定した行動が可能かどうか
        /// </summary>
        public bool CanPerformAction(ActionCategory action) => m_StateMachine.CanPerformAction(action);

        protected virtual void Awake()
        {
            m_StateMachine.Initialize(this);
            m_Modules = GetComponents<IEntityModule>();

            foreach (var module in m_Modules)
            {
                module.Setup(this);
            }
        }

        protected virtual void Start()
        {
            foreach (var module in m_Modules)
            {
                module.StartModule();
            }
        }

        protected virtual void Update()
        {
            m_StateMachine.Update();

            foreach (var module in m_Modules)
            {
                module.UpdateModule();
            }
        }

        protected virtual void FixedUpdate()
        {
            m_StateMachine.FixedUpdate();

            foreach (var module in m_Modules)
            {
                module.FixedUpdateModule();
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (var module in m_Modules)
            {
                module.DestroyModule();
            }
        }

        /// <summary>
        /// 特定の型のモジュールを取得
        /// </summary>
        public bool TryGetModule<T>(out T module) where T : class, IEntityModule
        {
            module = null;
            foreach (var m in m_Modules)
            {
                if (m is T matchedModule)
                {
                    module = matchedModule;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 特定の型のモジュールを取得（後方互換性のため）
        /// </summary>
        public void GetModule<T>(out T module) where T : class, IEntityModule
        {
            TryGetModule(out module);
        }

        /// <summary>
        /// 死亡処理（派生クラスでオーバーライド可能）
        /// </summary>
        public virtual void OnDeath()
        {
            m_StateMachine.SetDead();
        }

        /// <summary>
        /// リスポーン処理（派生クラスでオーバーライド可能）
        /// </summary>
        public virtual void OnRespawn()
        {
            m_StateMachine.ResetState();
        }
    }
}
