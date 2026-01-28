using UnityEngine;
using InGame.Gimmick;
using InGame.Entity;

namespace InGame.Player
{
    public class PlayerController : EntityController
    {
        /// <summary>
        /// 特定の型のPlayerモジュールを取得
        /// </summary>
        public bool TryGetPlayerModule<T>(out T module) where T : class, IPlayerModule
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