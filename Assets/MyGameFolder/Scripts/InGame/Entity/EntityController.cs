using UnityEngine;
using InGame.Gimmick;

namespace InGame.Entity
{
    /// <summary>
    /// Player, Enemy等の共通コントローラー基底クラス
    /// モジュールのライフサイクル管理を担当
    /// </summary>
    public abstract class EntityController : MonoBehaviour, IGrabber
    {
        protected IEntityModule[] m_Modules;

        // IGrabber実装
        public Vector3 GrabberPosition => transform.position;
        public GameObject GrabberObject => gameObject;

        protected virtual void Awake()
        {
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
            foreach (var module in m_Modules)
            {
                module.UpdateModule();
            }
        }

        protected virtual void FixedUpdate()
        {
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
    }
}
