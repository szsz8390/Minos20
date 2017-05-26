using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Minos20.Classes
{
    /// <summary>
    /// 本体
    /// </summary>
    public class MainGame
    {
        /// <summary>フィールド</summary>
        public BlockState[,] PlayFields { get; private set; }
        /// <summary>落下中のミノ位置</summary>
        public Point MinoPoint { get; private set; }
        /// <summary>落下中のミノ</summary>
        public Tetrimino CurrentMino { get; private set; }
        /// <summary>ゲームオーバーかどうか</summary>
        public bool IsGameOver { get; private set; }
        /// <summary>レベル</summary>
        public int Level { get; private set; }
        /// <summary>NEXTのミノ</summary>
        public Tetrimino[] NextMinos { get; private set; }
        /// <summary>HOLDのミノ</summary>
        private Tetrimino holdMino;
        /// <summary>HOLDのミノ</summary>
        public Tetrimino HoldMino
        {
            get { return holdMino; }
            private set 
            {
                // 回転を初期状態にもどすのだ
                if (value == null)
                {
                    holdMino = value;
                }
                else
                {
                    holdMino = Tetrimino.CreateMino(value.Figure).DeepCopy();
                }
            }
        }
        /// <summary>HOLD可能かどうか</summary>
        private bool canHold;
        /// <summary>ミノ生成クラス</summary>
        private TetriminoSupplier tetriminoSupplier;
        /// <summary>RENカウント</summary>
        private int ren;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainGame()
        {
            PlayFields = new BlockState[Constants.V_BLOCKS, Constants.H_BLOCKS];
            MinoPoint = Point.Empty;
            IsGameOver = false;
            Level = 0;
            tetriminoSupplier = new TetriminoSupplier();
            NextMinos = new Tetrimino[Constants.NEXT_COUNT];
            for (int i = 0; i < NextMinos.Length; i++)
            {
                NextMinos[i] = tetriminoSupplier.Supply().DeepCopy();
            }
            HoldMino = null;
            canHold = true;
            ren = 0;
        }

        /// <summary>
        /// 新しいミノを投入する。
        /// </summary>
        public void PutNewMino()
        {
            // 新しいミノ
            Tetrimino minoCopy = tetriminoSupplier.Supply();
            // Nextを現在のミノに設定
            CurrentMino = NextMinos[0].DeepCopy();
            // Nextを繰り上げ
            for (int i = 1; i < NextMinos.Length; i++)
            {
                NextMinos[i - 1] = NextMinos[i].DeepCopy();
            }
            // 新しいミノ(のコピー)を最後のNextに設定
            NextMinos[NextMinos.Length - 1] = minoCopy.DeepCopy();
            procNewMino();
        }

        /// <summary>
        /// 新しいミノの処理
        /// </summary>
        private void procNewMino()
        {
            // 横の初期位置
            int initX = PlayFields.GetLength(1) / 2 - 2;

            // 出現位置でかぶる場合、終了
            if (isKaburu(slice(initX, 0), CurrentMino.Fields))
            {
                IsGameOver = true;
            }
            // 初期位置のPointを作成
            Point newPoint = new Point(initX, 0);
            // 落とせるところまで落とす
            while (canDown(newPoint))
            {
                newPoint = new Point(newPoint.X, newPoint.Y + 1);
            }
            MinoPoint = newPoint;

            // Level x99でない場合、1上げる
            if (Level % 100 != 99)
            {
                Level++;
            }
        }

        /// <summary>
        /// 現在のミノを落とせる場所まで落とす。
        /// </summary>
        private void recalcMinoPoint()
        {
            while (canDown(MinoPoint))
            {
                MinoPoint = new Point(MinoPoint.X, MinoPoint.Y + 1);
            }
        }

        /// <summary>
        /// ミノを回転させる
        /// </summary>
        /// <param name="rotate">回転方向</param>
        public void RotateMino(HorizontalDirection rotate)
        {
            // 左右を壁に挟まれている場合、キャンセル
            if (isSurroundedKabe())
            {
                return;
            }

            // とりあえず回転させる
            CurrentMino.Rotate(rotate);

            // 壁(フィールドの端)めり込みチェック
            MerikomiResult merikomi = isMerikomi();
            if (merikomi != MerikomiResult.NONE)
            {
                // めり込んだ方向により補正
                int downHosei = 0;
                switch (merikomi)
                {
                    case MerikomiResult.DOWN:
                        // 下にめり込む場合、一段上げる
                        // TODO: 要検証？ 意図しない動きをしていそう
                        MinoPoint = new Point(MinoPoint.X, MinoPoint.Y - 1);
                        downHosei++;
                        break;
                    case MerikomiResult.LEFT:
                        // 左にめり込む場合、右に移動
                        // TODO: 要検証？IdouMino使わないとダメ？
                        IdouMino(HorizontalDirection.RIGHT);
                        // 単純移動では解決せず
                        //MinoPoint = new Point(MinoPoint.X + 1, MinoPoint.Y);
                        break;
                    case MerikomiResult.RIGHT:
                        // 右にめり込む場合、左に移動
                        // TODO: 要検証？IdouMino使わないとダメ？
                        IdouMino(HorizontalDirection.LEFT);
                        break;
                    default:
                        break;
                }
                // もう一度判定
                merikomi = isMerikomi();
                if (CurrentMino.Figure == TetriminoFigure.I)
                {
                    // I字だけもう一度
                    switch (merikomi)
                    {
                        case MerikomiResult.DOWN:
                            MinoPoint = new Point(MinoPoint.X, MinoPoint.Y - 1);
                            break;
                        case MerikomiResult.LEFT:
                            IdouMino(HorizontalDirection.RIGHT);
                            break;
                        case MerikomiResult.RIGHT:
                            IdouMino(HorizontalDirection.LEFT);
                            break;
                        default:
                            break;
                    }
                    downHosei++;
                    merikomi = isMerikomi();
                }
                // めり込みが解消しなければ補正したのをもとにもどす
                for (int i = 0; i < downHosei; i++)
                {
                    switch (merikomi)
                    {
                        case MerikomiResult.DOWN:
                            MinoPoint = new Point(MinoPoint.X, MinoPoint.Y + 1);
                            break;
                        case MerikomiResult.LEFT:
                            IdouMino(HorizontalDirection.LEFT);
                            break;
                        case MerikomiResult.RIGHT:
                            IdouMino(HorizontalDirection.RIGHT);
                            break;
                        default:
                            break;
                    }
                }
            }
            // ブロックかぶりチェック
            bool kaburu = isKaburu(slice(MinoPoint), CurrentMino.Fields);
            if (kaburu)
            {
                int up = 0;
                MinoPoint = new Point(MinoPoint.X, MinoPoint.Y - 1);
                up++;
                kaburu = isKaburu(slice(MinoPoint), CurrentMino.Fields);
                if (kaburu)
                {
                    if (CurrentMino.Figure == TetriminoFigure.I)
                    {
                        MinoPoint = new Point(MinoPoint.X, MinoPoint.Y - 1);
                        up++;
                        kaburu = isKaburu(slice(MinoPoint), CurrentMino.Fields);
                        if (!kaburu)
                        {
                            up = 0;
                        }
                    }
                    MinoPoint = new Point(MinoPoint.X, MinoPoint.Y + up);
                }
            }
            // かぶるかめり込んでいる場合、回転をもとにもどす
            if (kaburu || merikomi != MerikomiResult.NONE)
            {
                switch (rotate)
                {
                    case HorizontalDirection.LEFT:
                        CurrentMino.Rotate(HorizontalDirection.RIGHT);
                        break;
                    default:
                        CurrentMino.Rotate(HorizontalDirection.LEFT);
                        break;
                }
            }

            recalcMinoPoint();
        }

        /// <summary>
        /// ミノ移動
        /// </summary>
        /// <param name="direction">移動方向</param>
        public void IdouMino(HorizontalDirection direction)
        {
            // 移動方向の最大/最小のミノの横位置を取得
            int side = CurrentMino.GetMaxSide(direction);
            Point newPoint;
            if (direction == HorizontalDirection.LEFT)
            {
                // 単純に左へ移動させた位置
                int newX = MinoPoint.X - 1;
                // 許容される横位置の限界
                // フィールド左端、ただしミノの左端が4*4の左端でないならはみ出せる
                int min = 0 - side;
                // 限界値を超えないようにする
                if (newX < min)
                {
                    newX = min;
                }
                newPoint = new Point(newX, MinoPoint.Y);
            }
            else
            {
                // 単純に右へ移動させた位置
                int newX = MinoPoint.X + 1;
                // 許容される横位置の限界
                // フィールド右端、ただしミノの右端が4*4の右端でないならはみ出せる
                int max = 9 - side;
                // 限界値を超えないようにする
                if (max < newX)
                {
                    newX = max;
                }
                newPoint = new Point(newX, MinoPoint.Y);
            }
            // 既存ブロックカベ判定
            if (!isKabe(newPoint, direction))
            {
                MinoPoint = newPoint;
                recalcMinoPoint();
            }
        }

        /// <summary>
        /// 新しい位置が、さらに1段下に移動できるか調べる
        /// </summary>
        /// <param name="newPoint">新しい位置</param>
        /// <returns>1段下へ移動できるかどうか</returns>
        private bool canDown(Point newPoint)
        {
            // ミノの底辺縦位置を取得
            int[] floors = CurrentMino.GetFloors();
            int x = newPoint.X;
            // 縦位置の1段下
            int y = newPoint.Y + 1;
            
            for (int i = 0; i < floors.Length; i++)
            {
                // フィールド外なら飛ばす
                if (x + i >= Constants.H_BLOCKS || x + i < 0)
                {
                    continue;
                }
                // 底辺縦位置が未定義なら空の列なので飛ばす
                if (y + floors[i] < 0)
                {
                    continue;
                }
                // 縦位置がフィールド最下段を超えるならfalse
                if (y + floors[i] >= Constants.V_BLOCKS)
                {
                    return false;
                }
                // フィールドの該当位置にブロックが既にあるならfalse
                if (PlayFields[y + floors[i], x + i] == BlockState.FILL)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 新しい位置にカベ(既存ブロック)があるかどうか調べる
        /// </summary>
        /// <param name="newPoint">新しい位置</param>
        /// <param name="direction">調べる方向</param>
        /// <returns>新しい位置にカベがある場合true</returns>
        private bool isKabe(Point newPoint, HorizontalDirection direction)
        {
            // 移動方向のミノの横位置を取得
            int[] xs = CurrentMino.GetSides(direction);
            for (int i = 0; i < xs.Length; i++)
            {
                // 未定義(int.MaxValue/int.MinValue)なら飛ばす
                if (xs[i] == int.MaxValue || xs[i] == int.MinValue) continue;
                // 新しい位置の横位置を加算してフィールド上の横位置にする
                xs[i] += newPoint.X;
            }
            // フィールド上の縦位置の最底辺
            int y = newPoint.Y + CurrentMino.GetFloors().Max();
            // 底から調べる
            foreach (int x in xs.Reverse())
            {
                // フィールド外なら飛ばす
                if (x < 0 || x >= Constants.H_BLOCKS) continue;
                // カベがあるならtrue
                if (PlayFields[y, x] == BlockState.FILL)
                {
                    return true;
                }
                y--;
            }
            return false;
        }

        /// <summary>
        /// カベに2ブロック以上挟まれているかどうか調べる。
        /// </summary>
        /// <returns></returns>
        private bool isSurroundedKabe()
        {
            // TODO: 2*2の穴に横幅2のミノがハマっている場合、回転不可にしたい
            // 1*1と2*2に対応するように抽象化

            // 横幅1ブロックの行を取得
            int[] widths = CurrentMino.GetWidths();
            List<int> checkLineList = new List<int>();
            for (int i = 0; i < widths.Length; i++)
            {
                if (widths[i] != 1) continue;
                checkLineList.Add(i);
            }
            bool once = false;
            foreach (int i in checkLineList)
            {
                // 横幅1ブロックの箇所を特定
                for (int x = 0; x < Constants.MINO_SIDES; x++)
                {
                    if (CurrentMino.Fields[i, x] == BlockState.FILL)
                    {
                        // 左
                        BlockState left = BlockState.EMPTY;
                        if (MinoPoint.X + x == 0)
                        {
                            left = BlockState.FILL;
                        }
                        else
                        {
                            left = PlayFields[MinoPoint.Y + i, MinoPoint.X + x - 1];
                        }
                        // 右
                        BlockState right = BlockState.EMPTY;
                        if (MinoPoint.X + x == 9)
                        {
                            right = BlockState.FILL;
                        }
                        else
                        {
                            right = PlayFields[MinoPoint.Y + i, MinoPoint.X + x + 1];
                        }
                        if (left == BlockState.FILL && right == BlockState.FILL)
                        {
                            if (once == true)
                            {
                                return true;
                            }
                            once = true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// ホールド
        /// </summary>
        public void Hold()
        {
            if (!canHold) return;
            if (HoldMino == null)
            {
                HoldMino = CurrentMino;
                PutNewMino();
            }
            else
            {
                Tetrimino temp = HoldMino;
                HoldMino = CurrentMino;
                CurrentMino = temp;
                procNewMino();
            }
            canHold = false;
        }

        /// <summary>
        /// 落下中のミノを確定させる。
        /// </summary>
        /// <returns>メッセージ</returns>
        public string MinoKakutei()
        {
            string msg = "";
            if (!MinoPoint.IsEmpty)
            {
                for (int i = MinoPoint.Y; i < MinoPoint.Y + Constants.MINO_SIDES; i++)
                {
                    for (int j = MinoPoint.X; j < MinoPoint.X + Constants.MINO_SIDES; j++)
                    {
                        // フィールド外なら飛ばす
                        if (i >= Constants.V_BLOCKS
                            || j >= Constants.H_BLOCKS
                            || i < 0 || j < 0) continue;
                        if (PlayFields[i, j] == BlockState.EMPTY)
                        {
                            // ミノのFILLをフィールドに設定する
                            PlayFields[i, j] = CurrentMino.Fields[i - MinoPoint.Y, j - MinoPoint.X];
                        }
                    }
                }
            }
            // ミノ位置を初期化
            MinoPoint = Point.Empty;
            // 消せる行があれば消し、レベルに反映
            int eraceCount = doErace();
            Level += eraceCount;
            switch (eraceCount)
            {
                case 1:
                    msg = "SINGLE !";
                    if (ren > 0)
                    {
                        msg += string.Format("  REN {0} !!", ren);
                    }
                    ren++;
                    break;
                case 2:
                    msg = "DOUBLE !";
                    if (ren > 0)
                    {
                        msg += string.Format("  REN {0} !!", ren);
                    }
                    ren++;
                    break;
                case 3:
                    msg = "TRIPLE !";
                    if (ren > 0)
                    {
                        msg += string.Format("  REN {0} !!", ren);
                    }
                    ren++;
                    break;
                case 4:
                    msg = "TxTRxS !!";
                    if (ren > 0)
                    {
                        msg += string.Format("  REN {0} !!", ren);
                    }
                    ren++;
                    break;
                default:
                    msg = "";
                    ren = 0;
                    break;
            }
            // HOLD実行可能にする
            canHold = true;
            return msg;
        }

        /// <summary>
        /// 行消去
        /// </summary>
        /// <returns>消去した行数</returns>
        private int doErace()
        {
            int count = 0;

            List<BlockState[]> tempFields = new List<BlockState[]>();

            for (int i = 0; i < PlayFields.GetLength(0); i++)
            {
                // 1行すべてがFILLかどうか調べる
                bool allFill = true;
                BlockState[] tmp = new BlockState[Constants.H_BLOCKS];
                for (int j = 0; j < PlayFields.GetLength(1); j++)
                {
                    tmp[j] = PlayFields[i, j];
                    // EMPTYがあれば消せない
                    if (PlayFields[i, j] == BlockState.EMPTY)
                    {
                        allFill = false;
                    }
                }
                if (allFill)
                {
                    // 消せる場合、カウントアップ
                    count++;
                }
                else
                {
                    // 消せない場合、リストにとっておく
                    tempFields.Add(tmp);
                }
            }
            // フィールド初期化
            PlayFields = new BlockState[Constants.V_BLOCKS, Constants.H_BLOCKS];
            for (int i = 0; i < tempFields.Count; i++)
            {
                for (int j = 0; j < tempFields[i].Length; j++)
                {
                    // 削除した行数ぶんを飛ばしてとっておいた消せない行を再設定
                    PlayFields[i + count, j] = tempFields[i][j];
                }
            }

            return count;
        }

        /// <summary>
        /// ギブアップ
        /// </summary>
        public void GiveUp()
        {
            IsGameOver = true;
        }

        /// <summary>
        /// 20170525 未使用っぽいよ？
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (PlayFields == null) return "";
            var sb = new StringBuilder();
            for (int i = 0; i < PlayFields.GetLength(0); i++)
            {
                for (int j = 0; j < PlayFields.GetLength(1); j++)
                {
                    BlockState f = PlayFields[i, j];
                    sb.Append(f.ToDispString());
                }
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 描画用文字列表現を取得する。
        /// FieldBlockStateの値により以下のように表示する。
        /// EMPTY:　(全角スペース)
        /// FILL: ■
        /// ACTIVE: □
        /// </summary>
        /// <returns>描画用文字列表現</returns>
        public string GetDrawFieldString()
        {
            BlockState[,] fields = new BlockState[Constants.V_BLOCKS, Constants.H_BLOCKS];
            // フィールド
            for (int i = 0; i < PlayFields.GetLength(0); i++)
            {
                for (int j = 0; j < PlayFields.GetLength(1); j++)
                {
                    fields[i, j] = PlayFields[i, j];
                }
            }
            if (!MinoPoint.IsEmpty)
            {
                // ミノ
                for (int i = MinoPoint.Y; i < MinoPoint.Y + Constants.MINO_SIDES && i < Constants.V_BLOCKS; i++)
                {
                    if (i < 0)
                    {
                        continue;
                    }
                    for (int j = MinoPoint.X; j < MinoPoint.X + Constants.MINO_SIDES && j < Constants.H_BLOCKS; j++)
                    {
                        if (j < 0)
                        {
                            continue;
                        }
                        if (fields[i, j] == BlockState.EMPTY
                            && CurrentMino.Fields[i - MinoPoint.Y, j - MinoPoint.X] == BlockState.FILL)
                        {
                            fields[i, j] = BlockState.ACTIVE;
                        }
                    }
                }
            }
            var sb = new StringBuilder();
            for (int i = 0; i < fields.GetLength(0); i++)
            {
                // 外壁を追加
                sb.Append("壁");
                for (int j = 0; j < fields.GetLength(1); j++)
                {
                    BlockState f = fields[i, j];
                    sb.Append(f.ToDispString());
                }
                sb.Append("壁");
                sb.Append("\r\n");
            }
            sb.Append("壁壁壁壁壁壁壁壁壁壁壁壁");
            return sb.ToString();
        }

        /// <summary>
        /// srcのFILLとdstのFILLが同じ位置にあるかどうか調べる。
        /// </summary>
        /// <param name="src">src</param>
        /// <param name="dst">dst</param>
        /// <returns>同じ位置にFILLがあればtrue</returns>
        private bool isKaburu(BlockState[,] src, BlockState[,] dst)
        {
            for (int i = 0; i < src.GetLength(0); i++)
            {
                for (int j = 0; j < src.GetLength(1); j++)
                {
                    if (dst[i, j] == BlockState.EMPTY)
                    {
                        continue;
                    }
                    if (src[i, j] == dst[i, j])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// めり込み判定を行う。
        /// </summary>
        /// <returns>めり込み判定結果</returns>
        private MerikomiResult isMerikomi()
        {
            if (!MinoPoint.IsEmpty)
            {
                for (int y = MinoPoint.Y; y < MinoPoint.Y + Constants.MINO_SIDES; y++)
                {
                    for (int x = MinoPoint.X; x < MinoPoint.X + Constants.MINO_SIDES; x++)
                    {
                        if (y >= Constants.V_BLOCKS
                            && CurrentMino.Fields[y - MinoPoint.Y, x - MinoPoint.X] == BlockState.FILL)
                        {
                            // 縦が上限を超えた箇所がFILLの場合、下にめり込み
                            return MerikomiResult.DOWN;
                        }
                        if (x < 0 && CurrentMino.Fields[y - MinoPoint.Y, x - MinoPoint.X] == BlockState.FILL)
                        {
                            // 横が下限を超えた箇所がFILLの場合、左にめり込み
                            return MerikomiResult.LEFT;
                        }
                        if (x >= Constants.H_BLOCKS && CurrentMino.Fields[y - MinoPoint.Y, x - MinoPoint.X] == BlockState.FILL)
                        {
                            // 横が上限を超えた箇所がFILLの場合、右にめり込み
                            return MerikomiResult.RIGHT;
                        }
                    }
                }
            }
            return MerikomiResult.NONE;
        }
        
        /// <summary>
        /// フィールドの指定位置から4*4の範囲をコピーした配列を取得する。
        /// </summary>
        /// <param name="p">指定位置</param>
        /// <returns>4*4のフィールド配列</returns>
        private BlockState[,] slice(Point p)
        {
            return slice(p.X, p.Y);
        }

        /// <summary>
        /// フィールドの指定位置から4*4の範囲をコピーした配列を取得する。
        /// </summary>
        /// <param name="x">指定位置:横</param>
        /// <param name="y">指定位置:縦</param>
        /// <returns>4*4のフィールド配列</returns>
        private BlockState[,] slice(int x, int y)
        {
            BlockState[,] sliced
                = new BlockState[Constants.MINO_SIDES, Constants.MINO_SIDES];
            int dstY = 0;
            for (int i = y; dstY < Constants.MINO_SIDES; i++)
            {
                int dstX = 0;
                for (int j = x; dstX < Constants.MINO_SIDES; j++)
                {
                    if (i < 0 || j < 0
                        || i >= Constants.V_BLOCKS
                        || j >= Constants.H_BLOCKS)
                    {
                        sliced[dstY, dstX] = BlockState.EMPTY;
                    }
                    else
                    {
                        sliced[dstY, dstX] = PlayFields[i, j];
                    }
                    dstX++;
                }
                dstY++;
            }

            return sliced;
        }
    }
}
