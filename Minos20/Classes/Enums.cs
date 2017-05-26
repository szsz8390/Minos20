
namespace Minos20.Classes
{
    /// <summary>
    /// 横方向
    /// </summary>
    public enum HorizontalDirection
    {
        LEFT
        , RIGHT
    }

    /// <summary>
    /// めり込み判定結果
    /// </summary>
    public enum MerikomiResult
    {
        NONE, LEFT, UP, RIGHT, DOWN,
    }

    /// <summary>
    /// ブロックの状態
    /// </summary>
    public enum BlockState
    {
        EMPTY, FILL, ACTIVE,
    }

    /// <summary>
    /// ブロックの状態:拡張メソッド
    /// </summary>
    public static class FieldBlockStateExtension
    {
        /// <summary>
        /// 表示文字列を取得する。
        /// </summary>
        /// <param name="fbState">FieldBlockState</param>
        /// <returns>
        /// EMPTY: 　(全角スペース)
        /// FILL: ■
        /// ACTIVE: □
        /// </returns>
        public static string ToDispString(this BlockState fbState)
        {
            if (fbState == BlockState.EMPTY)
            {
                return "　";
            }
            else if (fbState == BlockState.FILL)
            {
                return "■";
            }
            else
            {
                return "□";
            }
        }
    }
}
