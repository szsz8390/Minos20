
namespace Minos20.Classes
{
    /// <summary>
    /// 定数クラス
    /// </summary>
    public class Constants
    {
        /// <summary>フィールドの横ブロック数</summary>
        public const int H_BLOCKS = 10;
        /// <summary>フィールドの縦ブロック数</summary>
        public const int V_BLOCKS = 20;
        /// <summary>ミノの辺のブロック数</summary>
        public const int MINO_SIDES = 4;
        /// <summary>ミノの種類数</summary>
        public const int MINO_KINDS = 7;
        /// <summary>ミノ出現補正しきい値</summary>
        public const int SUPPLY_HOSEI_THRESHOLD = 3;
        /// <summary>前回と同一ミノを生成時の再抽選回数</summary>
        public const int DUPLI_REROLL_COUNT = 5;
        /// <summary>NEXT表示数</summary>
        public const int NEXT_COUNT = 3;
    }
}
