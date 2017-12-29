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

                        Thread.Sleep(33); //loop every 10 milliseconds (could change this depending on what we need and eventually even make this delta time)
                    } catch (Exception ex) {

                    }
                }
            });
        }

        private void render() {
            //we should probably make these global variables as they wont change. no need to keep calling new variables and reusing cpu power
            int cellWidth = 10;
            int cellHeight = 10;
            int columns = picGame.Width / cellWidth;
            int rows = picGame.Height / cellHeight;

            //redraw the form's background
            Graphics g = this.CreateGraphics();
            g.FillRectangle(new SolidBrush(Color.FromArgb(255, 64, 64, 64)), 0, 0, this.Width, this.Height);

            //create a buffer to draw to it first in order to reduce visible lag
            Bitmap buffer = new Bitmap(picGame.Width, picGame.Height);

            //create a cell
            Rectangle rect = new Rectangle(0, 0, cellWidth, cellHeight);

            //get the graphics for the buffer so we can draw to it
            g = Graphics.FromImage(buffer);

            Pen border = new Pen(Color.Black);

            //clear the buffer and fill it with the background
            g.Clear(Color.DarkGreen);

            for (int col = 0; col < columns; col++) {
                for (int row = 0; row < rows; row++) {
                    //draw the border so we can see what is going on
                    g.DrawRectangle(border, col * cellWidth, row * cellHeight, cellWidth, cellHeight);

                    //add a function here to draw the player

                    //add a function here to draw the coins from an array. the update function will take care of placing the coins and adding coins to array
                }
            }

            //draw buffer to the picturebox
            g = picGame.CreateGraphics();
            g.DrawImage(buffer, 0, 0);

        }
    }
}
