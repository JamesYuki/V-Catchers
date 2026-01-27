using UnityEngine;

namespace MyGame.InGame.Enemy
{
    public class EnemyController : EnemyBase
    {
        private IEnemyModule[] m_EnemyModules;

        protected override void Awake()
        {
            base.Awake();
            m_EnemyModules = GetComponents<IEnemyModule>();
            foreach (var module in m_EnemyModules)
            {
                AddModule(module);
            }
        }

        protected override void Update()
        {
            base.Update();
        }

        public void GetModule<T>(out T module) where T : class, IEnemyModule
        {
            module = null;
            foreach (var m in m_EnemyModules)
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
