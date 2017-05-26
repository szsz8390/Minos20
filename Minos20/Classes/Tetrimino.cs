using System;
using System.Text;

namespace Minos20.Classes
{
    /// <summary>
    /// ミノ
    /// </summary>
    public class Tetrimino
    {
        /// <summary>ミノ定義フィールド</summary>
        public BlockState[,] Fields { get; private set; }
        /// <summary>形状</summary>
        public TetriminoFigure Figure { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Tetrimino()
        {
            Fields = new BlockState[Constants.MINO_SIDES, Constants.MINO_SIDES];
        }

        /// <summary>
        /// 回転処理
        /// </summary>
        /// <param name="rotate">回転方向</param>
        public void Rotate(HorizontalDirection rotate)
        {
            BlockState[,] tmpFields = new BlockState[Constants.MINO_SIDES, Constants.MINO_SIDES];
            int kijun = Constants.MINO_SIDES - 1;
            // Tの回転に対する応急処置
            if (Figure == TetriminoFigure.T)
            {
                kijun = Constants.MINO_SIDES - 2;
            }

            int kijunY = Constants.MINO_SIDES - 1;
            int kijunX = 0;
            // Tの回転に対する応急処置
            if (Figure == TetriminoFigure.T)
            {
                kijunY = Constants.MINO_SIDES - 2;
            }
            if (rotate == HorizontalDirection.LEFT)
            {
                kijunY = 0;
                kijunX = Constants.MINO_SIDES - 1;
                // Tの回転に対する応急処置
                if (Figure == TetriminoFigure.T)
                {
                    kijunX = Constants.MINO_SIDES -2;
                }
            }
            int srcY = kijunY;
            for (int dstY = 0; dstY <= kijun; dstY++)
            {
                int srcX = kijunX;
                for (int dstX = 0; dstX <= kijun; dstX++)
                {
                    tmpFields[dstX, dstY] = Fields[srcY, srcX];

                    if (rotate == HorizontalDirection.LEFT)
                    {
                        srcX--;
                    }
                    else
                    {
                        srcX++;
                    }
                }
                if (rotate == HorizontalDirection.LEFT)
                {
                    srcY++;
                }
                else
                {
                    srcY--;
                }
            }
            for (int i = 0; i < Fields.GetLength(0); i++)
            {
                for (int j = 0; j < Fields.GetLength(1); j++)
                {
                    Fields[i, j] = tmpFields[i, j];
                }
            }
        }

        /// <summary>
        /// 指定方向の端の位置を取得する。
        /// 左方向ならFILLの列位置の最小値、
        /// 右方向ならFILLの列位置の最大値を返却する。
        /// </summary>
        /// <param name="r">方向</param>
        /// <returns>端の位置</returns>
        public int GetMaxSide(HorizontalDirection r)
        {
            // 未定義値(左方向ならint最大値、右方向ならint最小値)で初期化
            int side = int.MinValue;
            if (r == HorizontalDirection.LEFT)
            {
                side = int.MaxValue;
            }
            for (int i = 0; i < Fields.GetLength(0); i++)
            {
                for (int j = 0; j < Fields.GetLength(1); j++)
                {
                    BlockState f = Fields[i, j];
                    // FILLの場合のみ列位置を確認
                    if (f == BlockState.FILL)
                    {
                        switch (r)
                        {
                            case HorizontalDirection.LEFT:
                                // 左方向の場合、最小値
                                side = Math.Min(side, j);
                                break;
                            default:
                                // 右方向の場合、最大値
                                side = Math.Max(side, j);
                                break;
                        }
                    }
                }
            }
            return side;
        }

        /// <summary>
        /// 指定方向の端の位置を取得する。
        /// </summary>
        /// <param name="r">方向</param>
        /// <returns>端の位置(行数ぶん)</returns>
        public int[] GetSides(HorizontalDirection r)
        {
            int[] sides = new int[Constants.MINO_SIDES];
            // 初期化値
            int fill = -1;
            // 未定義値(左方向ならint最大値、右方向ならint最小値)で初期化
            switch (r)
            {
                case HorizontalDirection.LEFT:
                    fill = int.MaxValue;
                    break;
                default:
                    fill = int.MinValue;
                    break;
            }
            for (int i = 0; i < sides.Length; i++)
            {
                sides[i] = fill;
            }

            for (int i = 0; i < Fields.GetLength(0); i++)
            {
                for (int j = 0; j < Fields.GetLength(1); j++)
                {
                    var f = Fields[i, j];
                    // FILLの場合のみ列位置を取得
                    if (f == BlockState.FILL)
                    {
                        switch (r)
                        {
                            case HorizontalDirection.LEFT:
                                // 左方向の場合、最小値
                                sides[i] = Math.Min(sides[i], j);
                                break;
                            default:
                                // 右方向の場合、最大値
                                sides[i] = Math.Max(sides[i], j);
                                break;
                        }
                    }
                }
            }
            return sides;
        }

        /// <summary>
        /// 底辺の位置を取得する。
        /// </summary>
        /// <returns>底辺の位置(列数ぶん)</returns>
        public int[] GetFloors()
        {
            // 未定義値(int.MinValue)で初期化
            int[] floors = new int[Constants.MINO_SIDES]
            {
                int.MinValue, int.MinValue, int.MinValue, int.MinValue
            };
            for (int x = 0; x < Fields.GetLength(0); x++)
            {
                for (int y = 0; y < Fields.GetLength(1); y++)
                {
                    BlockState f = Fields[x, y];
                    if (f == BlockState.FILL)
                    {
                        floors[y] = x;
                    }
                }
            }
            return floors;
        }

        /// <summary>
        /// 各行の幅を取得する。
        /// </summary>
        /// <returns>幅(行数ぶん)</returns>
        public int[] GetWidths()
        {
            int[] widths = new int[Constants.MINO_SIDES];

            for (int i = 0; i < Fields.GetLength(0); i++)
            {
                for (int j = 0; j < Fields.GetLength(1); j++)
                {
                    var f = Fields[i, j];
                    // FILLである行をカウント(隙間は無いものとする)
                    if (f == BlockState.FILL)
                    {
                        widths[i]++;
                    }
                }
            }
            return widths;
        }

        /// <summary>
        /// ディープコピー
        /// </summary>
        /// <returns>コピーされたミノ</returns>
        public Tetrimino DeepCopy()
        {
            Tetrimino minoCopy = new Tetrimino();
            for (int i = 0; i < this.Fields.GetLength(0); i++)
            {
                for (int j = 0; j < this.Fields.GetLength(1); j++)
                {
                    minoCopy.Fields[i, j] = this.Fields[i, j];
                }
            }
            minoCopy.Figure = this.Figure;
            return minoCopy;
        }

        /// <summary>
        /// 文字列表現を取得する。
        /// </summary>
        /// <returns>文字列表現</returns>
        public override string ToString()
        {
            if (Fields == null) return "";
            var sb = new StringBuilder();
            for (int i = 0; i < Fields.GetLength(0); i++)
            {
                for (int j = 0; j < Fields.GetLength(1); j++)
                {
                    BlockState f = Fields[i, j];
                    sb.Append(f.ToDispString());
                }
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        /// <summary>
        /// I字ミノを生成する。
        /// </summary>
        /// <returns>I字ミノ</returns>
        public static Tetrimino CreateI()
        {
            return new Tetrimino
            {
                Fields = new BlockState[,]
                {
                    {BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, },
                    {BlockState.FILL, BlockState.FILL, BlockState.FILL, BlockState.FILL, },
                    {BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, },
                    {BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, },
                },
                Figure = TetriminoFigure.I,
            };
        }
        /// <summary>
        /// O字ミノを生成する。
        /// </summary>
        /// <returns>O字ミノ</returns>
        public static Tetrimino CreateO()
        {
            return new Tetrimino
            {
                Fields = new BlockState[,]
                {
                    {BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, },
                    {BlockState.EMPTY, BlockState.FILL, BlockState.FILL, BlockState.EMPTY, },
                    {BlockState.EMPTY, BlockState.FILL, BlockState.FILL, BlockState.EMPTY, },
                    {BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, },
                },
                Figure = TetriminoFigure.O,
            };
        }
        /// <summary>
        /// T字ミノを生成する。
        /// </summary>
        /// <returns>T字ミノ</returns>
        public static Tetrimino CreateT()
        {
            return new Tetrimino
            {
                Fields = new BlockState[,]
                {
                    {BlockState.EMPTY, BlockState.FILL, BlockState.EMPTY, BlockState.EMPTY, },
                    {BlockState.FILL, BlockState.FILL, BlockState.FILL, BlockState.EMPTY, },
                    {BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, },
                    {BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, },
                },
                Figure = TetriminoFigure.T,
            };
        }
        /// <summary>
        /// L字ミノを生成する。
        /// </summary>
        /// <returns>L字ミノ</returns>
        public static Tetrimino CreateL()
        {
            return new Tetrimino
            {
                Fields = new BlockState[,]
                {
                    {BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, },
                    {BlockState.EMPTY, BlockState.EMPTY, BlockState.FILL, BlockState.EMPTY, },
                    {BlockState.FILL, BlockState.FILL, BlockState.FILL, BlockState.EMPTY, },
                    {BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, },
                },
                Figure = TetriminoFigure.L,
            };
        }
        /// <summary>
        /// J字ミノを生成する。
        /// </summary>
        /// <returns>J字ミノ</returns>
        public static Tetrimino CreateJ()
        {
            return new Tetrimino
            {
                Fields = new BlockState[,]
                {
                    {BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, },
                    {BlockState.EMPTY, BlockState.FILL, BlockState.EMPTY, BlockState.EMPTY, },
                    {BlockState.EMPTY, BlockState.FILL, BlockState.FILL, BlockState.FILL, },
                    {BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, },
                },
                Figure = TetriminoFigure.J,
            };
        }
        /// <summary>
        /// S字ミノを生成する。
        /// </summary>
        /// <returns>S字ミノ</returns>
        public static Tetrimino CreateS()
        {
            return new Tetrimino
            {
                Fields = new BlockState[,]
                {
                    {BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, },
                    {BlockState.EMPTY, BlockState.EMPTY, BlockState.FILL, BlockState.FILL, },
                    {BlockState.EMPTY, BlockState.FILL, BlockState.FILL, BlockState.EMPTY, },
                    {BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, },
                },
                Figure = TetriminoFigure.S,
            };
        }
        /// <summary>
        /// Z字ミノを生成する。
        /// </summary>
        /// <returns>Z字ミノ</returns>
        public static Tetrimino CreateZ()
        {
            return new Tetrimino
            {
                Fields = new BlockState[,]
                {
                    {BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, },
                    {BlockState.FILL, BlockState.FILL, BlockState.EMPTY, BlockState.EMPTY, },
                    {BlockState.EMPTY, BlockState.FILL, BlockState.FILL, BlockState.EMPTY, },
                    {BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, BlockState.EMPTY, },
                },
                Figure = TetriminoFigure.Z,
            };
        }

        /// <summary>
        /// 指定形状のミノを生成する。
        /// </summary>
        /// <param name="figure">形状</param>
        /// <returns>指定形状のミノ</returns>
        public static Tetrimino CreateMino(TetriminoFigure figure)
        {
            switch (figure)
            {
                case TetriminoFigure.I:
                    return Tetrimino.CreateI();
                case TetriminoFigure.O:
                    return Tetrimino.CreateO();
                case TetriminoFigure.T:
                    return Tetrimino.CreateT();
                case TetriminoFigure.J:
                    return Tetrimino.CreateJ();
                case TetriminoFigure.L:
                    return Tetrimino.CreateL();
                case TetriminoFigure.S:
                    return Tetrimino.CreateS();
                case TetriminoFigure.Z:
                    return Tetrimino.CreateZ();
                default:
                    return Tetrimino.CreateO();
            }
        }
    }

    /// <summary>
    /// ミノ形状
    /// </summary>
    public enum TetriminoFigure
    {
        I, O, T, J, L, S, Z
    }

}
