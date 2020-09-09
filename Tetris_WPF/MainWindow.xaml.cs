namespace Tetris_WPF
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Windows.Threading;

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int XMax = 10, YMax = 22;
        private const int Blocksize = 23;
        private const int LockDelay = 500;
        private const int WatchTimer = 100;
        private bool isRunning = false;
        private bool isPaused = false;
        private Random rand = new Random();
        private Rectangle[,] playfield;
        private Rectangle[,] preview;
        private Rectangle[,] hold;
        ////private bool[,] occupied;
        private int currentX = 4, currentY = 0;
        private int ghostY = 0;
        private int orientation = 0;
        private Pieces currentPiece;
        private Pieces nextPiece;
        private Pieces holdPiece = Pieces.Length;
        private bool recentHold = false;
        private List<Pieces> nextPieces;
        private int delay = 1000;
        private int[] pointsForLines;
        private Point[][] wallKickCW;
        private Point[][] wallKickCCW;
        private Point[][] wallKickICW;
        private Point[][] wallKickICCW;
        private int delLines = 0;
        private int checkLines = 10;
        private int lastLines = 0;
        private int comboCounter = 0;
        private DispatcherTimer gravityTimer, lockTimer, stopwatchTimer;
        private Stopwatch watch;
        private StatsViewModel stats;

        public MainWindow()
        {
            InitializeComponent();
            Create();
            this.DataContext = stats;
        }

        public enum Pieces
        {
            O, I, T, J, L, S, Z, Length
        }

        public enum Actions
        {
            ShiftLeft, ShiftRight, RotateCW, RotateCCW, Gravity, SoftDrop, HardDrop, Hold
        }

        public enum Scores
        {
            Single = 100, Double = 300, Triple = 500, Tetris = 800, B2BTetris = 1200, Combo = 50
        }

        private void Create()
        {
            stats = new StatsViewModel();

            grid_info.Visibility = Visibility.Hidden;

            pointsForLines = new int[4];
            pointsForLines[0] = 100;
            pointsForLines[1] = 300;
            pointsForLines[2] = 500;
            pointsForLines[3] = 800;

            // y from top down (unlike guideline), index is old orientation
            wallKickCW = new Point[4][];
            wallKickCW[0] = new Point[5] { new Point { X = 0, Y = 0 }, new Point { X = -1, Y = 0 }, new Point { X = -1, Y = -1 }, new Point { X = 0, Y = +2 }, new Point { X = -1, Y = +2 } };
            wallKickCW[1] = new Point[5] { new Point { X = 0, Y = 0 }, new Point { X = +1, Y = 0 }, new Point { X = +1, Y = +1 }, new Point { X = 0, Y = -2 }, new Point { X = +1, Y = -2 } };
            wallKickCW[2] = new Point[5] { new Point { X = 0, Y = 0 }, new Point { X = +1, Y = 0 }, new Point { X = +1, Y = -1 }, new Point { X = 0, Y = +2 }, new Point { X = +1, Y = +2 } };
            wallKickCW[3] = new Point[5] { new Point { X = 0, Y = 0 }, new Point { X = -1, Y = 0 }, new Point { X = -1, Y = +1 }, new Point { X = 0, Y = -2 }, new Point { X = -1, Y = -2 } };

            wallKickCCW = new Point[4][];
            wallKickCCW[0] = new Point[5] { new Point { X = 0, Y = 0 }, new Point { X = +1, Y = 0 }, new Point { X = +1, Y = -1 }, new Point { X = 0, Y = +2 }, new Point { X = +1, Y = +2 } };
            wallKickCCW[1] = new Point[5] { new Point { X = 0, Y = 0 }, new Point { X = +1, Y = 0 }, new Point { X = +1, Y = +1 }, new Point { X = 0, Y = -2 }, new Point { X = +1, Y = -2 } };
            wallKickCCW[2] = new Point[5] { new Point { X = 0, Y = 0 }, new Point { X = -1, Y = 0 }, new Point { X = -1, Y = -1 }, new Point { X = 0, Y = +2 }, new Point { X = -1, Y = +2 } };
            wallKickCCW[3] = new Point[5] { new Point { X = 0, Y = 0 }, new Point { X = -1, Y = 0 }, new Point { X = -1, Y = +1 }, new Point { X = 0, Y = -2 }, new Point { X = -1, Y = -2 } };

            wallKickICW = new Point[4][];
            wallKickICW[0] = new Point[5] { new Point { X = 0, Y = 0 }, new Point { X = -2, Y = 0 }, new Point { X = +1, Y = 0 }, new Point { X = -2, Y = +1 }, new Point { X = +1, Y = -2 } };
            wallKickICW[1] = new Point[5] { new Point { X = 0, Y = 0 }, new Point { X = -1, Y = 0 }, new Point { X = +2, Y = 0 }, new Point { X = -1, Y = -2 }, new Point { X = +2, Y = +1 } };
            wallKickICW[2] = new Point[5] { new Point { X = 0, Y = 0 }, new Point { X = +2, Y = 0 }, new Point { X = -1, Y = 0 }, new Point { X = +2, Y = -1 }, new Point { X = -1, Y = +2 } };
            wallKickICW[3] = new Point[5] { new Point { X = 0, Y = 0 }, new Point { X = +1, Y = 0 }, new Point { X = -2, Y = 0 }, new Point { X = +1, Y = +2 }, new Point { X = -2, Y = -1 } };

            wallKickICCW = new Point[4][];
            wallKickICCW[0] = new Point[5] { new Point { X = 0, Y = 0 }, new Point { X = -1, Y = 0 }, new Point { X = +2, Y = 0 }, new Point { X = -1, Y = -2 }, new Point { X = +2, Y = +1 } };
            wallKickICCW[1] = new Point[5] { new Point { X = 0, Y = 0 }, new Point { X = +2, Y = 0 }, new Point { X = -1, Y = 0 }, new Point { X = +2, Y = -1 }, new Point { X = -1, Y = +2 } };
            wallKickICCW[2] = new Point[5] { new Point { X = 0, Y = 0 }, new Point { X = +1, Y = 0 }, new Point { X = -2, Y = 0 }, new Point { X = +1, Y = +2 }, new Point { X = -2, Y = -1 } };
            wallKickICCW[3] = new Point[5] { new Point { X = 0, Y = 0 }, new Point { X = -2, Y = 0 }, new Point { X = +1, Y = 0 }, new Point { X = -2, Y = +1 }, new Point { X = +1, Y = -2 } };

            nextPieces = new List<Pieces>();

            watch = new Stopwatch();
            stopwatchTimer = new DispatcherTimer();
            stopwatchTimer.Tick += StopwatchTimer_Tick;

            gravityTimer = new DispatcherTimer();
            gravityTimer.Tick += GravityTimer_Tick;

            lockTimer = new DispatcherTimer();
            lockTimer.Tick += LockTimer_Tick;

            ////occupied = new bool[XMax, YMax];
            playfield = new Rectangle[XMax, YMax];
            for (int i = 0; i < playfield.GetLength(0); i++)
            {
                for (int j = 0; j < playfield.GetLength(1); j++)
                {
                    playfield[i, j] = new Rectangle();
                    playfield[i, j].VerticalAlignment = VerticalAlignment.Top;
                    playfield[i, j].HorizontalAlignment = HorizontalAlignment.Left;
                    playfield[i, j].Height = Blocksize;
                    playfield[i, j].Width = Blocksize;
                    playfield[i, j].StrokeThickness = 1;
                    ChangeSingleColor(playfield, i, j, Brushes.Black);
                    playfield[i, j].Margin = new Thickness((Blocksize * i) + rec_playfield.Margin.Left, (Blocksize * j) - 36, 0, 0);
                    ////images[i, j].ToolTip = i + "|" + j;
                    GridMain.Children.Add(playfield[i, j]);
                }
            }

            preview = new Rectangle[4, 4];
            CreateField(preview, grid_preview, rec_preview.Margin);

            hold = new Rectangle[4, 2];
            CreateField(hold, grid_hold, rec_hold.Margin);
        }

        private void CreateField(Rectangle[,] field, System.Windows.Controls.Grid gridOn, Thickness marginPos)
        {
            for (int i = 0; i < field.GetLength(0); i++)
            {
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    field[i, j] = new Rectangle();
                    field[i, j].VerticalAlignment = VerticalAlignment.Top;
                    field[i, j].HorizontalAlignment = HorizontalAlignment.Left;
                    field[i, j].Height = Blocksize;
                    field[i, j].Width = Blocksize;
                    field[i, j].StrokeThickness = 1;
                    ChangeSingleColor(field, i, j, Brushes.Black);
                    field[i, j].Margin = new Thickness((Blocksize * i) + marginPos.Left, (Blocksize * j) + marginPos.Top, 0, 0);
                    ////images[i, j].ToolTip = i + "|" + j;
                    gridOn.Children.Add(field[i, j]);
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (isRunning)
            {
                switch (e.Key)
                {
                    // shift left
                    case Key.NumPad4:
                    case Key.A: 
                    case Key.Left:
                        PerformAction(Actions.ShiftLeft);
                        break;

                    // shift right
                    case Key.NumPad6:
                    case Key.D:
                    case Key.Right:
                        PerformAction(Actions.ShiftRight);
                        break;

                    // soft drop
                    case Key.NumPad2:
                    case Key.S: 
                    case Key.Down:
                        PerformAction(Actions.SoftDrop);
                        break;

                    // hard drop
                    case Key.NumPad8:
                    case Key.Space:
                        PerformAction(Actions.HardDrop);
                        break;

                    // rotate clockwise
                    case Key.NumPad1:
                    case Key.NumPad5:
                    case Key.NumPad9:
                    case Key.W:
                    case Key.X:
                    case Key.Up:
                        PerformAction(Actions.RotateCW);
                        break;

                    // rotate counterclockwise
                    case Key.NumPad3:
                    case Key.NumPad7:
                    case Key.LeftCtrl:
                    case Key.Y:
                    case Key.Z:
                        PerformAction(Actions.RotateCCW);
                        break;
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                // hold WIP
                case Key.NumPad0:
                case Key.LeftShift:
                case Key.C:
                    if (holdPiece == Pieces.Length)
                    {
                        TurnPieceOff(hold, GetPiece(holdPiece, 1, 1, 0));
                        TurnPieceOff(playfield, GetPiece(currentPiece, currentX, currentY, orientation));
                        TurnPieceOff(playfield, GetPiece(currentPiece, currentX, ghostY, orientation));
                        holdPiece = currentPiece;
                        TurnPieceOn(hold, GetPiece(holdPiece, 1, 1, 0));
                        NextPiece();
                    }
                    else if (!recentHold)
                    {
                        TurnPieceOff(hold, GetPiece(holdPiece, 1, 1, 0));
                        TurnPieceOff(playfield, GetPiece(currentPiece, currentX, currentY, orientation));
                        TurnPieceOff(playfield, GetPiece(currentPiece, currentX, ghostY, orientation));
                        Pieces switcheridoo = currentPiece;
                        currentPiece = holdPiece;
                        holdPiece = switcheridoo;
                        TurnPieceOn(hold, GetPiece(holdPiece, 1, 1, 0));
                        SpawnPiece();

                        recentHold = true;
                    }

                    break;

                // pause
                case Key.F1:
                case Key.Escape:
                    GamePause();
                    break;
            }
        }

        private void ChangeSingleColor(Rectangle[,] field, int x, int y, Brush color)
        {
            // check if block inside of boundaries and apply color
            if (x >= 0 && x < field.GetLength(0) && y >= 0 && y < field.GetLength(1))
            {
                Color myColor = ((SolidColorBrush)color).Color;
                myColor.A = 235; // ~92%
                field[x, y].Fill = new SolidColorBrush(myColor);
                field[x, y].Stroke = color;
                ////occupied[x, y] = (color == Brushes.Black) ? false : true;
            }
        }

        private void TurnPieceOff(Rectangle[,] field, Piece piece)
        {
            for (int i = 0; i < 4; i++)
            {
                ChangeSingleColor(field, piece.PosX[i], piece.PosY[i], Brushes.Black);
            }
        }

        private void TurnPieceOn(Rectangle[,] field, Piece piece)
        {
            for (int i = 0; i < 4; i++)
            {
                ChangeSingleColor(field, piece.PosX[i], piece.PosY[i], piece.Color);
            }
        }

        private void TurnGhostPieceOn(Piece piece)
        {
            for (int i = 0; i < 4; i++)
            {
                ChangeSingleColor(playfield, piece.PosX[i], piece.PosY[i], Brushes.Gray);
            }
        }

        private Piece GetPiece(Pieces type, int x, int y, int orientation)
        {
            switch (type)
            {
                case Pieces.O: return GetOPiece(x, y);
                case Pieces.I: return GetIPiece(x, y, orientation);
                case Pieces.T: return GetTPiece(x, y, orientation);
                case Pieces.J: return GetJPiece(x, y, orientation);
                case Pieces.L: return GetLPiece(x, y, orientation);
                case Pieces.S: return GetSPiece(x, y, orientation);
                case Pieces.Z: return GetZPiece(x, y, orientation);
                default: return GetIPiece(x, y, orientation);
            }
        }

        private void PerformAction(Actions action)
        {
            bool lockPiece = false;
            if (isRunning && !isPaused)
            {
                isRunning = false;
                RemoveGhost();
                TurnPieceOff(playfield, GetPiece(currentPiece, currentX, currentY, orientation));

                switch (action)
                {
                    case Actions.ShiftLeft:
                        TestMove(currentX - 1, currentY, orientation, applyValues: true);
                        break;
                    case Actions.ShiftRight:
                        TestMove(currentX + 1, currentY, orientation, applyValues: true);
                        break;
                    case Actions.Gravity:
                        if (TestMove(currentX, currentY + 1, orientation))
                        {
                            currentY++;
                        }
                        else
                        {
                            lockPiece = true;
                        }

                        break;
                    case Actions.SoftDrop:
                        if (TestMove(currentX, currentY + 1, orientation))
                        {
                            currentY++;
                            AddPoints(1);
                            gravityTimer.Stop();
                            gravityTimer.Start();
                        }
                        else
                        {
                            lockPiece = true;
                        }

                        break;
                    case Actions.HardDrop:
                        AddPoints(2 * (ghostY - currentY));
                        currentY = ghostY;
                        lockPiece = true;

                        break;
                    case Actions.RotateCW:
                        switch (currentPiece)
                        {
                            case Pieces.I:
                                TestRotation(currentX, currentY, (orientation + 1) % 4, wallKickICW, true);
                                break;
                            case Pieces.O:
                                // ignore, since O has no rotation
                                break;
                            default:
                                TestRotation(currentX, currentY, (orientation + 1) % 4, wallKickCW, true);
                                break;
                        }

                        break;
                    case Actions.RotateCCW:
                        switch (currentPiece)
                        {
                            case Pieces.I:
                                TestRotation(currentX, currentY, (orientation - 1 + 4) % 4, wallKickICCW, true);
                                break;
                            case Pieces.O:
                                // ignore, since O has no rotation
                                break;
                            default:
                                TestRotation(currentX, currentY, (orientation - 1 + 4) % 4, wallKickCCW, true);
                                break;
                        }

                        break;
                }

                ShowGhost();
                TurnPieceOn(playfield, GetPiece(currentPiece, currentX, currentY, orientation));
                isRunning = true;

                if (lockPiece)
                {
                    // todo: implement lock delay
                    CheckLines();
                    if (currentY < YMax - 20)
                    {
                        GameOver();
                    }
                    else
                    {
                        NextPiece();
                    }
                }
            }
        }

        private bool TestMove(int testX, int testY, int testOrientation, bool applyValues = false)
        {
            Piece piece = GetPiece(currentPiece, testX, testY, testOrientation);

            // check boundary left
            if (piece.PosX[0] < 0 || piece.PosX[1] < 0 || piece.PosX[2] < 0 || piece.PosX[3] < 0)
            {
                return false;
            }

            // check boundary right
            if (piece.PosX[0] > playfield.GetLength(0) - 1 || piece.PosX[1] > playfield.GetLength(0) - 1 ||
                piece.PosX[2] > playfield.GetLength(0) - 1 || piece.PosX[3] > playfield.GetLength(0) - 1)
            {
                return false;
            }

            // check boundary down
            if (piece.PosY[0] > playfield.GetLength(1) - 1 || piece.PosY[1] > playfield.GetLength(1) - 1 ||
                piece.PosY[2] > playfield.GetLength(1) - 1 || piece.PosY[3] > playfield.GetLength(1) - 1)
            {
                return false;
            }

            // check if occupied
            if (playfield[piece.PosX[0], piece.PosY[0]].Stroke != Brushes.Black
                || playfield[piece.PosX[1], piece.PosY[1]].Stroke != Brushes.Black
                || playfield[piece.PosX[2], piece.PosY[2]].Stroke != Brushes.Black
                || playfield[piece.PosX[3], piece.PosY[3]].Stroke != Brushes.Black)
            ////if (occupied[piece.PosX[0], piece.PosY[0]]
            ////|| occupied[piece.PosX[1], piece.PosY[1]]
            ////|| occupied[piece.PosX[2], piece.PosY[2]]
            ////|| occupied[piece.PosX[3], piece.PosY[3]])
            {
                return false;
            }
            else
            {
                if (applyValues)
                {
                    currentX = testX;
                    currentY = testY;
                    orientation = testOrientation;
                }

                return true;
            }
        }

        private bool TestRotation(int testX, int testY, int testOrientation, Point[][] kickValues, bool applyValues = false)
        {
            // apply first compatible wallkick
            for (int i = 0; i < kickValues.Length; i++)
            {
                if (TestMove(testX + kickValues[orientation][i].X, testY + kickValues[orientation][i].Y, testOrientation, applyValues))
                {
                    return true;
                }
            }

            return false;
        }

        private int CheckStackingHeight()
        {
            int height = 0;
            for (int check_y = 0; check_y < YMax; check_y++)
            {
                for (int check_x = 0; check_x < XMax; check_x++)
                {
                    ////if (occupied[check_x, check_y])
                    if (playfield[check_x, check_y].Stroke != Brushes.Black)
                    {
                        height++;
                        break;
                    }
                }
            }

            return height;
        }

        private void PrepareLock()
        {
            // turn off gravity timer, turn on lock timer
        }

        private void ResetLock()
        {
            // reset the lock timer (move reset)
            if (lockTimer.IsEnabled)
            {
                lockTimer.Stop();
                lockTimer.Start();
            }
        }

        private void LockPiece()
        {
            // turn off lock timer, spawn new piece, checklines, checkstackingheight etc
        }

        private void LockTimer_Tick(object sender, EventArgs e)
        {
            LockPiece();
        }

        private void ShowGhost()
        {
            ghostY = currentY;
            
            while (TestMove(currentX, ghostY + 1, orientation))
            {
                ghostY++;
            }
            
            TurnGhostPieceOn(GetPiece(currentPiece, currentX, ghostY, orientation));
        }

        private void RemoveGhost()
        {
            TurnPieceOff(playfield, GetPiece(currentPiece, currentX, ghostY, orientation));
        }

        #region PIEZES

        private Piece GetOPiece(int x, int y)
        {
            Piece piece = new Piece
            {
                Color = Brushes.Yellow,
                PosX = new int[4],
                PosY = new int[4],
                Points = new Point[4]
            };
            piece.Points[0] = new Point { X = x + 0, Y = y - 1 };
            piece.Points[1] = new Point { X = x + 1, Y = y - 1 };
            piece.Points[2] = new Point { X = x + 0, Y = y + 0 };
            piece.Points[3] = new Point { X = x + 1, Y = y + 0 };
            piece.PosX[0] = x;      piece.PosY[0] = y - 1;
            piece.PosX[1] = x + 1;  piece.PosY[1] = y - 1;
            piece.PosX[2] = x;      piece.PosY[2] = y;
            piece.PosX[3] = x + 1;  piece.PosY[3] = y;
            return piece;
        }

        private Piece GetIPiece(int x, int y, int orientation)
        {
            Piece piece = new Piece
            {
                Color = Brushes.Turquoise,
                PosX = new int[4],
                PosY = new int[4]
            };
            switch (orientation)
            {
                case 0:
                    piece.PosX[0] = x - 1; piece.PosY[0] = y;
                    piece.PosX[1] = x;     piece.PosY[1] = y;
                    piece.PosX[2] = x + 1; piece.PosY[2] = y;
                    piece.PosX[3] = x + 2; piece.PosY[3] = y;
                    break;
                case 1:
                    piece.PosX[0] = x + 1; piece.PosY[0] = y - 1;
                    piece.PosX[1] = x + 1; piece.PosY[1] = y;
                    piece.PosX[2] = x + 1; piece.PosY[2] = y + 1;
                    piece.PosX[3] = x + 1; piece.PosY[3] = y + 2;
                    break;
                case 2:
                    piece.PosX[0] = x - 1; piece.PosY[0] = y + 1;
                    piece.PosX[1] = x;     piece.PosY[1] = y + 1;
                    piece.PosX[2] = x + 1; piece.PosY[2] = y + 1;
                    piece.PosX[3] = x + 2; piece.PosY[3] = y + 1;
                    break;
                case 3:
                    piece.PosX[0] = x; piece.PosY[0] = y - 1;
                    piece.PosX[1] = x; piece.PosY[1] = y;
                    piece.PosX[2] = x; piece.PosY[2] = y + 1;
                    piece.PosX[3] = x; piece.PosY[3] = y + 2;
                    break;
            }

            return piece;
        }

        private Piece GetTPiece(int x, int y, int orientation)
        {
            Piece piece = new Piece
            {
                Color = Brushes.DeepPink,
                PosX = new int[4],
                PosY = new int[4]
            };
            switch (orientation)
            {
                case 0:
                    piece.PosX[0] = x; piece.PosY[0] = y;
                    piece.PosX[1] = x - 1; piece.PosY[1] = y;
                    piece.PosX[2] = x + 1; piece.PosY[2] = y;
                    piece.PosX[3] = x; piece.PosY[3] = y - 1;
                    break;
                case 1:
                    piece.PosX[0] = x; piece.PosY[0] = y + 1;
                    piece.PosX[1] = x; piece.PosY[1] = y;
                    piece.PosX[2] = x; piece.PosY[2] = y - 1;
                    piece.PosX[3] = x + 1; piece.PosY[3] = y;
                    break;
                case 2:
                    piece.PosX[0] = x; piece.PosY[0] = y;
                    piece.PosX[1] = x - 1; piece.PosY[1] = y;
                    piece.PosX[2] = x + 1; piece.PosY[2] = y;
                    piece.PosX[3] = x; piece.PosY[3] = y + 1;
                    break;
                case 3:
                    piece.PosX[0] = x; piece.PosY[0] = y + 1;
                    piece.PosX[1] = x; piece.PosY[1] = y;
                    piece.PosX[2] = x; piece.PosY[2] = y - 1;
                    piece.PosX[3] = x - 1; piece.PosY[3] = y;
                    break;
            }

            return piece;
        }

        private Piece GetJPiece(int x, int y, int orientation)
        {
            Piece piece = new Piece
            {
                Color = Brushes.DarkBlue,
                PosX = new int[4],
                PosY = new int[4]
            };
            switch (orientation)
            {
                case 0:
                    piece.PosX[0] = x + 1; piece.PosY[0] = y;
                    piece.PosX[1] = x; piece.PosY[1] = y;
                    piece.PosX[2] = x - 1; piece.PosY[2] = y;
                    piece.PosX[3] = x - 1; piece.PosY[3] = y - 1;
                    break;
                case 1:
                    piece.PosX[0] = x; piece.PosY[0] = y + 1;
                    piece.PosX[1] = x; piece.PosY[1] = y;
                    piece.PosX[2] = x; piece.PosY[2] = y - 1;
                    piece.PosX[3] = x + 1; piece.PosY[3] = y - 1;
                    break;
                case 2:
                    piece.PosX[0] = x - 1; piece.PosY[0] = y;
                    piece.PosX[1] = x; piece.PosY[1] = y;
                    piece.PosX[2] = x + 1; piece.PosY[2] = y;
                    piece.PosX[3] = x + 1; piece.PosY[3] = y + 1;
                    break;
                case 3:
                    piece.PosX[0] = x; piece.PosY[0] = y - 1;
                    piece.PosX[1] = x; piece.PosY[1] = y;
                    piece.PosX[2] = x; piece.PosY[2] = y + 1;
                    piece.PosX[3] = x - 1; piece.PosY[3] = y + 1;
                    break;
            }

            return piece;
        }

        private Piece GetLPiece(int x, int y, int orientation)
        {
            Piece piece = new Piece
            {
                Color = Brushes.DarkOrange,
                PosX = new int[4],
                PosY = new int[4]
            };
            switch (orientation)
            {
                case 0:
                    piece.PosX[0] = x - 1; piece.PosY[0] = y;
                    piece.PosX[1] = x; piece.PosY[1] = y;
                    piece.PosX[2] = x + 1; piece.PosY[2] = y;
                    piece.PosX[3] = x + 1; piece.PosY[3] = y - 1;
                    break;
                case 1:
                    piece.PosX[0] = x; piece.PosY[0] = y - 1;
                    piece.PosX[1] = x; piece.PosY[1] = y;
                    piece.PosX[2] = x; piece.PosY[2] = y + 1;
                    piece.PosX[3] = x + 1; piece.PosY[3] = y + 1;
                    break;
                case 2:
                    piece.PosX[0] = x + 1; piece.PosY[0] = y;
                    piece.PosX[1] = x; piece.PosY[1] = y;
                    piece.PosX[2] = x - 1; piece.PosY[2] = y;
                    piece.PosX[3] = x - 1; piece.PosY[3] = y + 1;
                    break;
                case 3:
                    piece.PosX[0] = x; piece.PosY[0] = y + 1;
                    piece.PosX[1] = x; piece.PosY[1] = y;
                    piece.PosX[2] = x; piece.PosY[2] = y - 1;
                    piece.PosX[3] = x - 1; piece.PosY[3] = y - 1;
                    break;
            }

            return piece;
        }

        private Piece GetSPiece(int x, int y, int orientation)
        {
            Piece piece = new Piece
            {
                Color = Brushes.Lime,
                PosX = new int[4],
                PosY = new int[4]
            };
            switch (orientation)
            {
                case 0:
                    piece.PosX[0] = x - 1; piece.PosY[0] = y;
                    piece.PosX[1] = x; piece.PosY[1] = y;
                    piece.PosX[2] = x; piece.PosY[2] = y - 1;
                    piece.PosX[3] = x + 1; piece.PosY[3] = y - 1;
                    break;
                case 1:
                    piece.PosX[0] = x; piece.PosY[0] = y - 1;
                    piece.PosX[1] = x; piece.PosY[1] = y;
                    piece.PosX[2] = x + 1; piece.PosY[2] = y;
                    piece.PosX[3] = x + 1; piece.PosY[3] = y + 1;
                    break;
                case 2:
                    piece.PosX[0] = x + 1; piece.PosY[0] = y;
                    piece.PosX[1] = x; piece.PosY[1] = y;
                    piece.PosX[2] = x; piece.PosY[2] = y + 1;
                    piece.PosX[3] = x - 1; piece.PosY[3] = y + 1;
                    break;
                case 3:
                    piece.PosX[0] = x; piece.PosY[0] = y + 1;
                    piece.PosX[1] = x; piece.PosY[1] = y;
                    piece.PosX[2] = x - 1; piece.PosY[2] = y;
                    piece.PosX[3] = x - 1; piece.PosY[3] = y - 1;
                    break;
            }

            return piece;
        }

        private Piece GetZPiece(int x, int y, int orientation)
        {
            Piece piece = new Piece
            {
                Color = Brushes.Red,
                PosX = new int[4],
                PosY = new int[4]
            };
            switch (orientation)
            {
                case 0:
                    piece.PosX[0] = x + 1; piece.PosY[0] = y;
                    piece.PosX[1] = x; piece.PosY[1] = y;
                    piece.PosX[2] = x; piece.PosY[2] = y - 1;
                    piece.PosX[3] = x - 1; piece.PosY[3] = y - 1;
                    break;
                case 1:
                    piece.PosX[0] = x; piece.PosY[0] = y + 1;
                    piece.PosX[1] = x; piece.PosY[1] = y;
                    piece.PosX[2] = x + 1; piece.PosY[2] = y;
                    piece.PosX[3] = x + 1; piece.PosY[3] = y - 1;
                    break;
                case 2:
                    piece.PosX[0] = x - 1; piece.PosY[0] = y;
                    piece.PosX[1] = x; piece.PosY[1] = y;
                    piece.PosX[2] = x; piece.PosY[2] = y + 1;
                    piece.PosX[3] = x + 1; piece.PosY[3] = y + 1;
                    break;
                case 3:
                    piece.PosX[0] = x; piece.PosY[0] = y - 1;
                    piece.PosX[1] = x; piece.PosY[1] = y;
                    piece.PosX[2] = x - 1; piece.PosY[2] = y;
                    piece.PosX[3] = x - 1; piece.PosY[3] = y + 1;
                    break;
            }

            return piece;
        }

        #endregion

        private void GenerateRandomOrder()
        {
            while (nextPieces.Count < 3)
            {
                // generate random bag order (all pieces in random order) and add to nextpieces
                Pieces[] temp = new Pieces[(int)Pieces.Length];
                for (int i = 0; i < (int)Pieces.Length; i++)
                {
                    temp[i] = (Pieces)i;
                }

                Pieces[] tempArray = temp.OrderBy(x => rand.Next()).ToArray();

                foreach (Pieces p in tempArray)
                {
                    nextPieces.Add(p);
                }
            }

            nextPiece = nextPieces[0];
            nextPieces.Remove(nextPiece);
        }

        private void CreatePreview()
        {
            currentPiece = nextPiece;
            recentHold = false;

            // reset preview
            TurnPieceOff(preview, GetPiece(nextPiece, 1, 2, 0));
            
            GenerateRandomOrder();
            TurnPieceOn(preview, GetPiece(nextPiece, 1, 2, 0));
        }

        private void NextPiece()
        {
            CreatePreview();
            SpawnPiece();
        }

        private void SpawnPiece()
        {
            currentX = 4;
            currentY = YMax - 21; // Line 21
            orientation = 0;

            bool drop = TestMove(currentX, currentY + 1, orientation);
            ShowGhost();
            TurnPieceOn(playfield, GetPiece(currentPiece, currentX, currentY, orientation));

            // drop to line 20 instantly if able
            if (drop)
            {
                PerformAction(Actions.Gravity);
            }

            gravityTimer.Interval = TimeSpan.FromMilliseconds(delay);
            gravityTimer.Stop();
            gravityTimer.Start();
        }

        private void GravityTimer_Tick(object sender, EventArgs e)
        {
            PerformAction(Actions.Gravity);
        }

        private void CheckLines()
        {
            bool full = false;
            for (int checkY = 0; checkY < YMax; checkY++)
            {
                full = true;
                for (int checkX = 0; checkX < XMax; checkX++)
                {
                    ////if (!occupied[checkX, checkY])
                    if (playfield[checkX, checkY].Stroke == Brushes.Black)
                    {
                        full = false;
                        break;
                    }
                }

                if (full)
                {
                    RemoveLine(checkY);
                    delLines++;
                    stats.Lines++;
                    checkY = -1;              
                }
            }

            if (delLines > 0)
            {
                ChangeStats();
            }
        }

        private void RemoveLine(int selectY)
        {
            for (int removeY = selectY; removeY > 0; removeY--)
            {
                for (int removeX = 0; removeX < XMax; removeX++)
                {
                    ChangeSingleColor(playfield, removeX, removeY, playfield[removeX, removeY - 1].Stroke);
                }
            }
        }

        private void ChangeStats()
        {
            if (delLines > 0 && delLines < 5)
            {
                AddPoints(pointsForLines[delLines - 1]);
            }

            lastLines += delLines;
            if (lastLines >= checkLines)
            {
                lastLines -= checkLines;
                if (stats.Level == 10)
                {
                    GameOver(hasWon: true);
                }
                else
                {
                    stats.Level++;
                    ChangeDelayOfficial();
                }
            }

            delLines = 0;
        }

        private void AddPoints(int num)
        {
            stats.Points += num;
        }

        private void ChangeDelay()
        {
            delay = (delay * 4) / 5;
            stats.Speed = (double)1000 / (double)delay;
        }

        private void ChangeDelayOfficial()
        {
            double num = Math.Pow(0.8 - ((stats.Level - 1) * 0.007), stats.Level - 1);
            delay = (int)Math.Round(num * 1000.0);
            stats.Speed = (double)1000 / (double)delay;
        }

        private void ResetStats()
        {
            delay = 1000;
            stats.Speed = 1;
            stats.Level = 1;
            stats.Points = 0;
            stats.Lines = 0;
            comboCounter = 0;
        }

        private void StopwatchTimer_Tick(object sender, EventArgs e)
        {
            stats.Time = watch.Elapsed;
        }

        private void Start()
        {
            if (!isRunning)
            {
                watch.Reset();
                watch.Start();
                stopwatchTimer.Interval = TimeSpan.FromMilliseconds(WatchTimer);
                stopwatchTimer.Start();
                isPaused = false;
                lb_state.Content = "PAUSE";
                lb_state.Visibility = Visibility.Hidden;
                ResetStats();
                gravityTimer.Interval = TimeSpan.FromMilliseconds(delay);

                isRunning = true;
                ////lb_start.Visibility = Visibility.Hidden;
                bt_start_stop.Content = "Stop";
                for (int i = 0; i < XMax; i++)
                {
                    for (int j = 0; j < YMax; j++)
                    {
                        ChangeSingleColor(playfield, i, j, Brushes.Black);
                    }
                }

                CreatePreview();
                NextPiece();
            }
        }

        private void GameOver(bool hasWon = false)
        {
            gravityTimer.Stop();
            stopwatchTimer.Stop();
            watch.Stop();
            isPaused = false;
            lb_state.Visibility = Visibility.Visible;
            lb_state.Content = hasWon ? "GAME WON!" : "GAME OVER!";
            isRunning = false;
            ////lb_start.Visibility = Visibility.Visible;
            bt_start_stop.Content = "Start";
        }

        private void GamePause()
        {
            if (isRunning)
            {
                if (isPaused)
                {
                    watch.Start();
                    stopwatchTimer.Start();
                    gravityTimer.Start();
                }
                else
                {
                    watch.Stop();
                    stopwatchTimer.Stop();
                    gravityTimer.Stop();
                }

                lb_state.Visibility = isPaused ? Visibility.Hidden : Visibility.Visible;
                isPaused = !isPaused;
            }
        }

        private void Bt_start_stop_Click(object sender, RoutedEventArgs e)
        {
            if (!isRunning)
            {
                Start();
            }
            else
            {
                GameOver();
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            GamePause();
        }

        private void Bt_info_Click(object sender, RoutedEventArgs e)
        {
            GamePause();
            lb_version.Content = "Version: 0.16.0";
            grid_info.Visibility = Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            grid_info.Visibility = Visibility.Hidden;
        }

        public struct Piece
        {
            public Brush Color;
            public int[] PosX;
            public int[] PosY;
            public Point[] Points;
            ////PieceRotation[] rotations;
        }

        ////struct Piece
        ////{
        ////    Color color;
        ////    PieceRotation[] rotations;
        ////}

        ////struct PieceRotation
        ////{
        ////    Point[] points;
        ////}

        public struct Point
        {
            public int X, Y;
        }
    }
}