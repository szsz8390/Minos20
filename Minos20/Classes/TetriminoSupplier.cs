using System;
using System.Linq;

namespace Minos20.Classes
{
    /// <summary>
    /// ミノ生成クラス
    /// </summary>
    public class TetriminoSupplier
    {
        /// <summary>各種類のミノを生成したかどうか</summary>
        private bool[] initSupplied;
        /// <summary>ミノごとの生成数</summary>
        private int[] counts;
        /// <summary>ランダムのシード</summary>
        private Random seed;
        /// <summary>最初の7つを生成したかどうか</summary>
        private bool finishInitSupply;
        /// <summary>1つ前に生成したミノ番号</summary>
        private int beforeNum;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TetriminoSupplier()
        {
            counts = new int[Constants.MINO_KINDS];
            initSupplied = new bool[Constants.MINO_KINDS];
            int seedseed = (int) DateTime.UtcNow.Ticks / DateTime.UtcNow.Day;
            seed = new Random(seedseed);
            finishInitSupply = false;
        }

        /// <summary>
        /// ミノを生成して返す。
        /// </summary>
        /// <returns>ミノ</returns>
        public Tetrimino Supply()
        {
            int num = GetRandomNumber();
            // 最初の7つかどうか判定
            if (!finishInitSupply)
            {
                // 最初の7つの場合、7種類がかぶらずに出現するように、
                // かぶったら再抽選する
                while (initSupplied[num])
                {
                    num = GetRandomNumber();
                }
                initSupplied[num] = true;
                // 7種類全部出現させたら排出済みフラグをtrueにする
                if (!initSupplied.Any(b => !b))
                {
                    finishInitSupply = true;
                }
            }
            counts[num]++;
            beforeNum = num;
            switch (num)
            {
                case 0:
                    return Tetrimino.CreateI();
                case 1:
                    return Tetrimino.CreateO();
                case 2:
                    return Tetrimino.CreateT();
                case 3:
                    return Tetrimino.CreateJ();
                case 4:
                    return Tetrimino.CreateL();
                case 5:
                    return Tetrimino.CreateS();
                case 6:
                    return Tetrimino.CreateZ();
                default:
                    return Tetrimino.CreateO();
            }
        }

        /// <summary>
        /// 生成するミノ番号を取得する。
        /// </summary>
        /// <returns></returns>
        private int getSupplyMinoNum()
        {
            int num = GetRandomNumber();
            // 最初の7つかどうか判定
            if (!finishInitSupply)
            {
                // 最初の7つの場合、7種類がかぶらずに出現するように、
                // かぶったら再抽選する
                while (initSupplied[num])
                {
                    num = GetRandomNumber();
                }
                initSupplied[num] = true;
                // 7種類全部出現させたら排出済みフラグをtrueにする
                if (!initSupplied.Any(b => !b))
                {
                    finishInitSupply = true;
                }
            }
            // 排出回数が最大排出数よりしきい値以上少なかったら再抽選
            if (counts.Max() - counts[num] > Constants.SUPPLY_HOSEI_THRESHOLD)
            {
                num = GetRandomNumber();
            }
            // 前に排出したものと同じなら再抽選
            if (beforeNum == num)
            {
                int count = 0;
                while (beforeNum == num && count < Constants.DUPLI_REROLL_COUNT)
                {
                    num = GetRandomNumber();
                    count++;
                }
            }
            return num;
        }

        /// <summary>
        /// ミノ番号の中でランダムな番号を取得する。
        /// </summary>
        /// <returns>ランダムなミノ番号</returns>
        private int GetRandomNumber()
        {
            var r = new Random(seed.Next(int.MaxValue));
            return r.Next(Constants.MINO_KINDS);
        }
    }
}
