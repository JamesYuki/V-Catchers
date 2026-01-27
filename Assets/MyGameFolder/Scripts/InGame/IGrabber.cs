namespace InGame
{
    public interface IGrabber
    {
        // 例: ドラッグ座標を取得するAPI（プレイヤー用）
        bool GetDrag(out UnityEngine.Vector2 dragPos);
    }
}
