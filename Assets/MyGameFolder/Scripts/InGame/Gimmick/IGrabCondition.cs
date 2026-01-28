using UnityEngine;

namespace InGame.Gimmick
{
    /// <summary>
    /// 掴める状態かどうかの条件を定義するインターフェース
    /// </summary>
    public interface IGrabCondition
    {
        /// <summary>
        /// 掴める状態かどうかを判定
        /// </summary>
        /// <param name="grabbable">対象のIGrabbable</param>
        /// <param name="grabber">掴もうとしているIGrabber</param>
        /// <returns>掴める場合はtrue</returns>
        bool CanGrab(IGrabbable grabbable, IGrabber grabber);
    }

    /// <summary>
    /// 掴み可能状態を提供するインターフェース（オプション）
    /// IGrabbableを拡張して、自身が掴まれる条件を提供できる
    /// </summary>
    public interface IGrabbableConditionProvider
    {
        /// <summary>
        /// 現在掴まれることが可能かどうか
        /// </summary>
        bool CanBeGrabbed { get; }
    }
}
