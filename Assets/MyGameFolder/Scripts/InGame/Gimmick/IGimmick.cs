using UnityEngine;
using InGame.Player;

namespace InGame.Gimmick
{

    public interface IGimmick
    {
        void Activate();
        void Deactivate();
    }

    /// <summary>
    /// 掴む側（Player, Enemy等）のインターフェース
    /// </summary>
    public interface IGrabber
    {
        /// <summary>
        /// 掴む側の位置を取得
        /// </summary>
        Vector3 GrabberPosition { get; }

        /// <summary>
        /// 掴む側のGameObject
        /// </summary>
        GameObject GrabberObject { get; }
    }

    /// <summary>
    /// 掴まれる側（Box等）のインターフェース
    /// 状態管理と通知のみを担当し、物理制御はGrabber側で行う
    /// </summary>
    public interface IGrabbable
    {
        /// <summary>
        /// 掴むポイントの位置
        /// </summary>
        Vector3 GetGrapplePoint();

        /// <summary>
        /// 掴まれ開始時のコールバック
        /// </summary>
        void OnGrabbed(IGrabber grabber);

        /// <summary>
        /// 掴まれ終了時のコールバック
        /// </summary>
        void OnReleased(IGrabber grabber);

        /// <summary>
        /// 現在掴まれているかどうか
        /// </summary>
        bool IsGrabbed { get; }

        /// <summary>
        /// 現在掴んでいるGrabber（nullなら掴まれていない）
        /// </summary>
        IGrabber CurrentGrabber { get; }

        /// <summary>
        /// Rigidbody2Dへのアクセス（物理制御用）
        /// </summary>
        Rigidbody2D Rigidbody { get; }

        /// <summary>
        /// 掴まれているオブジェクトのGameObject
        /// </summary>
        GameObject GrabbableObject { get; }

        /// <summary>
        /// オブジェクトの重さ（持ち上げに必要なレベルの指標）
        /// </summary>
        float Weight { get; }

        /// <summary>
        /// 物理的な質量（Rigidbody2Dのmass）
        /// </summary>
        float PhysicsMass { get; }
    }

    public interface ICapturable
    {
        void Capture(out Virus virus);
    }
}
