using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeuralNetworkAI {
    public partial class frmMain : Form {
        public frmMain() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            //create and run a task that will draw into the picturebox (rename the picturebox though)
            //the task will constantly draw a grid. each cell will be 10px by 10px and the background will
            //be green in color (or whatever color). for the beginning, lets draw a border around each cell
            //just so we can see whats going on

            Game();
        }

        private void Game() {
            //http://gameprogrammingpatterns.com/game-loop.html

            Task t = Task.Run(() => {
                //change this true to a variable at some point so we can stop and start
                while (true) {
                    try {
                        //process inputs

                        //update everything

                        //render the game
                        render();

                        Thread.Sleep(100); //loop every 10 milliseconds (could change this depending on what we need and eventually even make this delta time)
                    } catch (Exception ex) {

                    }
                }
            });
        }

        private void render() {
            int cellWidth = 10;
            int cellHeight = 10;
            int columns = picGame.Width / cellWidth;
            int rows = picGame.Height / cellHeight;

            //redraw the form's background
            Graphics g = this.CreateGraphics();
            g.FillRectangle(new SolidBrush(Color.FromArgb(255, 64, 64, 64)), 0, 0, this.Width, this.Height);

            //add this buffer later on when the game is a little more developed in order to reduce lag
            //Bitmap buffer = new Bitmap(picGame.Width, picGame.Height);

            Rectangle rect = new Rectangle(0, 0, cellWidth, cellHeight);

            g = picGame.CreateGraphics();

            Pen border = new Pen(Color.Black);

            //clear the picturebox before we redraw to it
            //g.Clear(Color.DarkGreen);

            for (int col = 0; col < columns; col++) {
                for (int row = 0; row < rows; row++) {
                    g.FillRectangle(Brushes.DarkGreen, col * cellWidth, row * cellHeight, cellWidth, cellHeight);
                    g.DrawRectangle(border, col * cellWidth, row * cellHeight, cellWidth, cellHeight);
                }
            }
        }
    }
}
