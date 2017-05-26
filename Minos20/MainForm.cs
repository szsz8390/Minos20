using System.Windows.Forms;

namespace Minos20
{
    using Minos20.Classes;

    /// <summary>
    /// メインフォーム
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>ゲーム本体</summary>
        private MainGame mainGame;
        /// <summary>ゲーム開始フラグ</summary>
        private bool start;
        /// <summary>NEXT表示ラベル</summary>
        private Label[] lblNexts;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            init();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void init()
        {
            mainGame = new MainGame();
            lblArea.Text = mainGame.GetDrawFieldString();
            start = false;
            lblInfo.Text = "Press Space Key To Start";
            lblNexts = new Label[] { lblNext, lblNext2, lblNext3 };
        }

        /// <summary>
        /// キー押下時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                // 左
                case Keys.Left:
                    if (!start) return;
                    mainGame.IdouMino(HorizontalDirection.LEFT);
                    repaint();
                    break;
                // 右
                case Keys.Right:
                    if (!start) return;
                    mainGame.IdouMino(HorizontalDirection.RIGHT);
                    repaint();
                    break;
                // Z
                case Keys.Z:
                    if (!start) return;
                    mainGame.RotateMino(HorizontalDirection.LEFT);
                    repaint();
                    break;
                // X
                case Keys.X:
                    if (!start) return;
                    mainGame.RotateMino(HorizontalDirection.RIGHT);
                    repaint();
                    break;
                // 上
                case Keys.Up:
                    if (!start) return;
                    lblInfo.Text = mainGame.MinoKakutei();
                    mainGame.PutNewMino();
                    repaint();
                    if (mainGame.IsGameOver)
                    {
                        start = false;
                        lblInfo.Text = "Game Over";
                    }
                    break;
                // スペース
                case Keys.Space:
                    if (!start)
                    {
                        start = true;
                        lblInfo.Text = "";
                        mainGame = new MainGame();
                        mainGame.PutNewMino();
                        repaint();
                    }
                    repaint();
                    break;
                // Esc
                case Keys.Escape:
                    if (start)
                    {
                        mainGame.GiveUp();
                        start = false;
                        lblInfo.Text = "Game Over";
                    }
                    break;
                default:
                    // LShift
                    if (e.Shift)
                    {
                        mainGame.Hold();
                        repaint();
                    }
                    break;
            }
        }

        /// <summary>
        /// 画面を再描画する。
        /// </summary>
        private void repaint()
        {
            lblArea.Text = mainGame.GetDrawFieldString();
            lblLevel.Text = mainGame.Level.ToString();
            for (int i = 0; i < lblNexts.Length; i++)
            {
                lblNexts[i].Text = mainGame.NextMinos[i].ToString();
            }
            if (mainGame.HoldMino != null)
            {
                lblHold.Text = mainGame.HoldMino.ToString();
            }
        }

        /// <summary>
        /// ヘルプボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBox.Show(""
                + "Left Key : Move Left\r\n"
                + "Right Key : Move Right\r\n"
                + "Up Key : Hard Drop\r\n"
                + "Z : Rotate Left\r\n"
                + "X : Rotate Right\r\n"
                + "L-Shift : Hold\r\n"
                + "\r\n"
                + "Space Key : Start Game\r\n"
                + "ESC Key : Give Up\r\n"
                + "", "Minos20 Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            e.Cancel = true;
        }
    }
}
