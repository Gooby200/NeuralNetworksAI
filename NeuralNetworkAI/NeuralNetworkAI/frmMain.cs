using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeuralNetworkAI {
    public partial class frmMain : Form {
        enum Mode { Manual, AI, Training }

        // Network topology is kept in one place so load/save and the trainer agree.
        static readonly int[] Topology = new int[] { GameSimulation.FEATURE_COUNT, 8, 3 };

        Mode mode = Mode.Manual;
        bool running;
        int moveDirection = GameSimulation.ACTION_STILL;

        GameSimulation sim;
        Agent agent;

        int cellWidth = 10;
        int cellHeight = 10;
        int columns;
        int rows;
        int FPS = 30;

        CancellationTokenSource trainingCts;
        Task trainingTask;

        public frmMain() {
            InitializeComponent();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (keyData == Keys.F1) {
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Form1_Load(object sender, EventArgs e) {
            picGame.Focus();

            int seed = numberGenerator(8);
            txtSeed.Text = seed.ToString();

            columns = picGame.Width / cellWidth;
            rows = picGame.Height / cellHeight;

            StartNewGame();
        }

        private void StartNewGame() {
            int s;
            int seed = int.TryParse(txtSeed.Text, out s) ? s : Environment.TickCount;
            sim = new GameSimulation(seed, columns, rows, cellWidth, cellHeight, FPS, 60 * FPS * 10);
            moveDirection = GameSimulation.ACTION_STILL;
            lblPoints.Text = "0";
            running = true;
            RunGameLoop();
        }

        private int numberGenerator(int length) {
            Random num = new Random();
            string rndNumbers = "";
            for (int i = 0; i < length; i++) {
                int rndNumber = num.Next(10);
                while (i == 0 && rndNumber == 0) {
                    rndNumber = num.Next(10);
                }
                rndNumbers += rndNumber.ToString();
            }
            return int.Parse(rndNumbers);
        }

        private void RunGameLoop() {
            Task.Run(() => {
                while (running) {
                    try {
                        int action = moveDirection;
                        if (mode == Mode.AI && agent != null) {
                            action = agent.Decide(sim.GetFeatures());
                        }

                        if (!sim.Step(action)) {
                            running = false;
                            break;
                        }

                        UpdatePointsLabel(sim.points);
                        render();
                        Thread.Sleep(1000 / FPS);
                    } catch (Exception) {
                    }
                }

                if (!this.IsDisposed) {
                    this.BeginInvoke((MethodInvoker)(() => {
                        if (mode == Mode.AI) {
                            StartNewGame();
                        } else if (mode == Mode.Manual) {
                            MessageBox.Show(this, "Game Over. Score: " + sim.points, "Game Over");
                        }
                    }));
                }
            });
        }

        private void UpdatePointsLabel(int points) {
            if (lblPoints.IsHandleCreated) {
                lblPoints.BeginInvoke((MethodInvoker)(() => lblPoints.Text = points.ToString()));
            }
        }

        private void render() {
            if (picGame.IsDisposed || !picGame.IsHandleCreated) return;

            Bitmap buffer = new Bitmap(picGame.Width, picGame.Height);
            using (Graphics g = Graphics.FromImage(buffer)) {
                g.Clear(Color.DarkGreen);
                foreach (Coin coin in sim.coins) coin.draw(g);
                foreach (Bomb bomb in sim.bombs) bomb.draw(g);
                using (SolidBrush brush = new SolidBrush(Color.Red)) {
                    g.FillRectangle(brush, sim.playerX, sim.playerY, cellWidth, cellHeight);
                }
            }
            picGame.BeginInvoke((MethodInvoker)(() => {
                using (Graphics g = picGame.CreateGraphics()) {
                    g.DrawImage(buffer, 0, 0);
                }
            }));
        }

        private void button1_Click(object sender, EventArgs e) {
            Clipboard.SetText(txtSeed.Text);
        }

        private void frmMain_KeyDown(object sender, KeyEventArgs e) {
            if (mode != Mode.Manual) return;
            if (e.KeyCode == Keys.A) {
                moveDirection = GameSimulation.ACTION_LEFT;
            } else if (e.KeyCode == Keys.D) {
                moveDirection = GameSimulation.ACTION_RIGHT;
            }
        }

        private void frmMain_KeyUp(object sender, KeyEventArgs e) {
            if (mode != Mode.Manual) return;
            moveDirection = GameSimulation.ACTION_STILL;
        }

        private void btnPlayAI_Click(object sender, EventArgs e) {
            if (agent == null) {
                MessageBox.Show(this, "No trained agent loaded. Train or load weights first.", "AI Play");
                return;
            }
            mode = Mode.AI;
            lblMode.Text = "Mode: AI";
            running = false;
            Thread.Sleep(1000 / FPS + 20);
            StartNewGame();
        }

        private void btnManual_Click(object sender, EventArgs e) {
            mode = Mode.Manual;
            lblMode.Text = "Mode: Manual";
            running = false;
            Thread.Sleep(1000 / FPS + 20);
            StartNewGame();
            picGame.Focus();
        }

        private void btnTrain_Click(object sender, EventArgs e) {
            if (trainingTask != null && !trainingTask.IsCompleted) {
                trainingCts.Cancel();
                btnTrain.Text = "Train";
                return;
            }

            // Pause the visible game while training so rendering doesn't compete for CPU.
            running = false;
            mode = Mode.Training;
            lblMode.Text = "Mode: Training...";
            btnTrain.Text = "Stop";

            trainingCts = new CancellationTokenSource();
            CancellationToken ct = trainingCts.Token;
            Trainer trainer = new Trainer(
                Topology, columns, rows, cellWidth, cellHeight,
                populationSize: 30, elitismCount: 6, gamesPerAgent: 3,
                simFps: FPS, maxTicks: 2000,
                mutationRate: 0.1, mutationScale: 0.3, seed: 0);

            int targetGenerations = 50;
            trainingTask = Task.Run(() => {
                trainer.RunGenerations(targetGenerations, progress => {
                    this.BeginInvoke((MethodInvoker)(() => {
                        lblMode.Text = "Training gen " + progress.generation + "/" + targetGenerations
                            + " best=" + progress.bestScore.ToString("F1");
                    }));
                }, ct);

                if (trainer.Best != null) agent = trainer.Best;

                this.BeginInvoke((MethodInvoker)(() => {
                    btnTrain.Text = "Train";
                    mode = Mode.AI;
                    lblMode.Text = "Mode: AI (trained, gen " + trainer.Generation
                        + ", best " + trainer.BestScore.ToString("F1") + ")";
                    running = false;
                    Thread.Sleep(1000 / FPS + 20);
                    StartNewGame();
                }));
            }, ct);
        }

        private void btnSave_Click(object sender, EventArgs e) {
            if (agent == null) {
                MessageBox.Show(this, "No agent to save. Train first.", "Save");
                return;
            }
            using (SaveFileDialog dlg = new SaveFileDialog()) {
                dlg.Filter = "Agent weights (*.txt)|*.txt";
                dlg.FileName = "agent.txt";
                if (dlg.ShowDialog(this) == DialogResult.OK) {
                    try {
                        agent.SaveToFile(dlg.FileName);
                    } catch (Exception ex) {
                        MessageBox.Show(this, "Save failed: " + ex.Message, "Save");
                    }
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e) {
            using (OpenFileDialog dlg = new OpenFileDialog()) {
                dlg.Filter = "Agent weights (*.txt)|*.txt";
                if (dlg.ShowDialog(this) == DialogResult.OK) {
                    try {
                        agent = Agent.LoadFromFile(dlg.FileName);
                        lblMode.Text = "Loaded: " + Path.GetFileName(dlg.FileName);
                    } catch (Exception ex) {
                        MessageBox.Show(this, "Load failed: " + ex.Message, "Load");
                    }
                }
            }
        }
    }
}
