using UnityEngine;

namespace InGame.Player
{
    public class PlayerController : MonoBehaviour
    {
        private IPlayerModule[] m_PlayerModules;

        private void Awake()
        {
            m_PlayerModules = GetComponents<IPlayerModule>();

            foreach (var module in m_PlayerModules)
            {
                module.Setup(this);
            }
        }

        private void Start()
        {
            foreach (var module in m_PlayerModules)
            {
                module.StartModule();
            }
        }

        private void Update()
        {
            foreach (var module in m_PlayerModules)
            {
                module.UpdateModule();
            }
        }

        private void FixedUpdate()
        {
            foreach (var module in m_PlayerModules)
            {
                module.FixedUpdateModule();
            }
        }

        private void OnDestroy()
        {
            foreach (var module in m_PlayerModules)
            {
                module.DestroyModule();
            }
        }

        public void GetModule<T>(out T module) where T : class, IPlayerModule
        {
            module = null;
            foreach (var m in m_PlayerModules)
            {
                if (m is T matchedModule)
                {
                    module = matchedModule;
                    return;
                }
            }
        }
    }
}