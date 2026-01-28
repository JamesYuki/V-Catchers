using UnityEngine;
using InGame.Entity;

namespace InGame.Enemy
{
    public class EnemyController : EntityController
    {
        /// <summary>
        /// 特定の型のEnemyモジュールを取得
        /// </summary>
        public bool TryGetEnemyModule<T>(out T module) where T : class, IEnemyModule
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
    }
}
