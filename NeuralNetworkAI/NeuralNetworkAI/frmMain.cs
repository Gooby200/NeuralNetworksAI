using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeuralNetworkAI {
    public partial class frmMain : Form {
        //create array of coins here maybe 
        List<Coin> coins = new List<Coin>();
        List<Coin> coinRemoval = new List<Coin>();

        Random rnd = new Random();

        int cellWidth;
        int cellHeight;
        int columns;
        int rows;

        //global variable?
        int FPS = 30;
        
        int coinTick = 0;
        int coinEvent = 50; //coin event is really the speed for which the coin goes up

        int coinGenerationTick = 0;
        int coinGenerationEvent = 500;

        int gcTick = 0;
        int gcEvent = 1000;

        public frmMain() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            //create and run a task that will draw into the picturebox (rename the picturebox though)
            //the task will constantly draw a grid. each cell will be 10px by 10px and the background will
            //be green in color (or whatever color). for the beginning, lets draw a border around each cell
            //just so we can see whats going on
            cellWidth = 10;
            cellHeight = 10;
            columns = picGame.Width / cellWidth;
            rows = picGame.Height / cellHeight;

            //initialize first coin somewhere
            coins.Add(new Coin(rnd.Next(columns) * cellWidth, (rows * cellHeight) - cellHeight));

            Game();
        }

        private void Game() {
            //http://gameprogrammingpatterns.com/game-loop.html



            Task t = Task.Run(() => {
                //change this true to a variable at some point so we can stop and start
                while (true) {
                    try {
                        //process inputs (submit the user requests and let update handle the movement)

                        //render the game at the FPS
                        render();

                        //update everything
                        update();

                        //we leak memory, so lets fix that by doing garbage collection
                        collectGarbage();

                        Thread.Sleep(1000 / FPS); //(could change this depending on what we need and eventually even make this delta time)
                    } catch (Exception ex) {

                    }
                }
            });
        }

        private void collectGarbage() {
            gcTick += (1000 / FPS);
            if (gcTick >= gcEvent) {
                gcTick = 0;
                System.GC.Collect();
            }
        }

        private void update() {
            //move the coins up
            coinTick += (1000 / FPS);
            if (coinTick >= coinEvent) {
                //move the coins
                foreach (Coin coin in coins) {
                    coin.setLocation(coin.getX(), coin.getY() - cellHeight);

                    //the coin is already off the screen and we dont need to waste time drawing it.
                    //prepare it for removal
                    if (coin.getY() < 0) {
                        coinRemoval.Add(coin);
                    }
                }

                //remove the requested coins
                foreach (Coin coin in coinRemoval) {
                    coins.Remove(coin);
                }

                //clear our coin removal array
                coinRemoval.Clear();

                //reset tick
                coinTick = 0;
            }

            //generate new coin
            coinGenerationEvent = rnd.Next(200, 2000);
            coinGenerationTick += (1000 / FPS);
            if (coinGenerationTick >= coinGenerationEvent) {
                coins.Add(new Coin(rnd.Next(0, columns) * cellWidth, (rows * cellHeight) - cellHeight));
                coinGenerationTick = 0;
            }

            //move player if that's being requested

            //do some collision detection to check game status
        }

        private void render() {
            //redraw the form's background
            using (Graphics g = this.CreateGraphics()) {
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 64, 64, 64)), 0, 0, this.Width, this.Height);
            }            

            //create a buffer to draw to it first in order to reduce visible lag
            Bitmap buffer = new Bitmap(picGame.Width, picGame.Height);

            //create a cell
            Rectangle rect = new Rectangle(0, 0, cellWidth, cellHeight);

            //get the graphics for the buffer so we can draw to it
            using (Graphics g = Graphics.FromImage(buffer)) {
                Pen border = new Pen(Color.Black);

                //clear the buffer and fill it with the background
                g.Clear(Color.DarkGreen);

                //draw the coins from an array
                foreach (Coin coin in coins) {
                    coin.draw(g);
                }

                //draw the player

                //draw the border
                for (int col = 0; col < columns; col++) {
                    for (int row = 0; row < rows; row++) {
                        //draw the border so we can see the grid
                        g.DrawRectangle(border, col * cellWidth, row * cellHeight, cellWidth, cellHeight);
                    }
                }
            }

            //draw buffer to the picturebox
            using (Graphics g = picGame.CreateGraphics()) {
                g.DrawImage(buffer, 0, 0);
            }
        }
    }
}
