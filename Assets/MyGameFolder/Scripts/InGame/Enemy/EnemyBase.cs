using UnityEngine;
using System.Collections.Generic;

namespace MyGame.InGame.Enemy
{
    public abstract class EnemyBase : MonoBehaviour
    {
        protected List<IEnemyModule> m_Modules = new List<IEnemyModule>();

        protected virtual void Awake()
        {
        }

        public void AddModule(IEnemyModule module)
        {
            if (!m_Modules.Contains(module))
            {
                m_Modules.Add(module);
                module.OnModuleAdded(this);
            }
        }

        public void RemoveModule(IEnemyModule module)
        {
            if (m_Modules.Contains(module))
            {
                m_Modules.Remove(module);
                module.OnModuleRemoved(this);
            }
        }

        protected virtual void Update()
        {
            foreach (var module in m_Modules)
            {
                module.OnEnemyUpdate();
            }
        }
    }

    public interface IEnemyModule
    {
        void OnModuleAdded(EnemyBase enemy);
        void OnModuleRemoved(EnemyBase enemy);
        void OnEnemyUpdate();
    }
}
