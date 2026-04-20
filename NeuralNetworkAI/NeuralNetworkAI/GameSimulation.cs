using System;
using System.Collections.Generic;

namespace NeuralNetworkAI {
    // Pure game logic with no rendering or form state. Shared between the visible
    // WinForms game loop and the headless GA trainer so both reach identical outcomes
    // given the same seed and actions.
    class GameSimulation {
        public const int ACTION_STILL = 0;
        public const int ACTION_LEFT = 1;
        public const int ACTION_RIGHT = 2;

        public const int FEATURE_COUNT = 5;

        public readonly int columns;
        public readonly int rows;
        public readonly int cellWidth;
        public readonly int cellHeight;
        public readonly int maxTicks;

        public int playerX { get; private set; }
        public int playerY { get; private set; }
        public int points { get; private set; }
        public int ticks { get; private set; }
        public bool done { get; private set; }

        public List<Coin> coins { get; private set; }
        public List<Bomb> bombs { get; private set; }

        private readonly Random rnd;
        private readonly int fps;
        private readonly int coinEvent;
        private readonly int bombEvent;
        private readonly int playerSpeedEvent;

        private int coinTick;
        private int bombTick;
        private int coinGenerationTick;
        private int bombGenerationTick;
        private int coinGenerationEvent;
        private int bombGenerationEvent;
        private int playerSpeedTick;

        public GameSimulation(int seed, int columns, int rows, int cellWidth, int cellHeight, int fps, int maxTicks) {
            this.rnd = new Random(seed);
            this.columns = columns;
            this.rows = rows;
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight;
            this.fps = fps;
            this.coinEvent = 50;
            this.bombEvent = 50;
            this.playerSpeedEvent = 5;
            this.maxTicks = maxTicks;

            this.coins = new List<Coin>();
            this.bombs = new List<Bomb>();

            this.playerX = (columns / 2) * cellWidth;
            this.playerY = (rows / 3) * cellHeight;

            this.coins.Add(new Coin(rnd.Next(columns) * cellWidth, (rows * cellHeight) - cellHeight));
            this.coinGenerationEvent = rnd.Next(200, 3000);
            this.bombGenerationEvent = rnd.Next(200, 3000);
        }

        // Advance one tick. Returns true while the game is still running.
        public bool Step(int action) {
            if (done) return false;

            int dt = 1000 / fps;
            ticks++;

            coinTick += dt;
            if (coinTick >= coinEvent) {
                coinTick = 0;
                for (int i = coins.Count - 1; i >= 0; i--) {
                    coins[i].setLocation(coins[i].getX(), coins[i].getY() - cellHeight);
                    if (coins[i].getY() < 0) coins.RemoveAt(i);
                }
            }

            bombTick += dt;
            if (bombTick >= bombEvent) {
                bombTick = 0;
                for (int i = bombs.Count - 1; i >= 0; i--) {
                    bombs[i].setLocation(bombs[i].getX(), bombs[i].getY() - cellHeight);
                    if (bombs[i].getY() < 0) bombs.RemoveAt(i);
                }
            }

            bool coinGenerated = false;
            int coinXLocation = 0;
            coinGenerationTick += dt;
            if (coinGenerationTick >= coinGenerationEvent) {
                coinXLocation = rnd.Next(0, columns) * cellWidth;
                coins.Add(new Coin(coinXLocation, (rows * cellHeight) - cellHeight));
                coinGenerationTick = 0;
                coinGenerationEvent = rnd.Next(200, 3000);
                coinGenerated = true;
            }

            bombGenerationTick += dt;
            if (bombGenerationTick >= bombGenerationEvent) {
                int bombXLocation = rnd.Next(0, columns) * cellWidth;
                if (coinGenerated && columns > 1) {
                    while (bombXLocation == coinXLocation) {
                        bombXLocation = rnd.Next(0, columns) * cellWidth;
                    }
                }
                bombs.Add(new Bomb(bombXLocation, (rows * cellHeight) - cellHeight));
                bombGenerationTick = 0;
                bombGenerationEvent = rnd.Next(200, 3000);
            }

            playerSpeedTick += dt;
            if (playerSpeedTick >= playerSpeedEvent) {
                playerSpeedTick = 0;
                if (action == ACTION_LEFT && playerX - cellWidth >= 0) {
                    playerX -= cellWidth;
                } else if (action == ACTION_RIGHT && (playerX / cellWidth) + 1 < columns) {
                    playerX += cellWidth;
                }
            }

            for (int i = coins.Count - 1; i >= 0; i--) {
                if (coins[i].getX() == playerX && coins[i].getY() == playerY) {
                    points += coins[i].getPointValue();
                    coins.RemoveAt(i);
                }
            }

            foreach (Bomb bomb in bombs) {
                if (bomb.getX() == playerX && bomb.getY() == playerY) {
                    done = true;
                    return false;
                }
            }

            if (ticks >= maxTicks) {
                done = true;
                return false;
            }

            return true;
        }

        // Features normalized to roughly [-1, 1] so the sigmoid network gets useful inputs.
        // Order: player x, nearest coin dx, nearest coin dy, nearest bomb dx, nearest bomb dy.
        // When no coin/bomb is on the board, defaults keep inputs neutral.
        public double[] GetFeatures() {
            double boardW = columns * cellWidth;
            double boardH = rows * cellHeight;

            double coinDx = 0, coinDy = 1, bombDx = 0, bombDy = 1;

            int nx = 0, ny = 0;
            bool found = FindNearestCoin(out nx, out ny);
            if (found) {
                coinDx = (nx - playerX) / boardW;
                coinDy = (ny - playerY) / boardH;
            }

            found = FindNearestBomb(out nx, out ny);
            if (found) {
                bombDx = (nx - playerX) / boardW;
                bombDy = (ny - playerY) / boardH;
            }

            return new double[] {
                (playerX / boardW) * 2 - 1,
                coinDx,
                coinDy,
                bombDx,
                bombDy,
            };
        }

        private bool FindNearestCoin(out int x, out int y) {
            x = 0; y = 0;
            int bestDist = int.MaxValue;
            bool found = false;
            foreach (Coin c in coins) {
                int ix = c.getX();
                int iy = c.getY();
                if (iy < playerY) continue;
                int dx = ix - playerX;
                int dy = iy - playerY;
                int dist = dx * dx + dy * dy;
                if (dist < bestDist) {
                    bestDist = dist; x = ix; y = iy; found = true;
                }
            }
            return found;
        }

        private bool FindNearestBomb(out int x, out int y) {
            x = 0; y = 0;
            int bestDist = int.MaxValue;
            bool found = false;
            foreach (Bomb b in bombs) {
                int ix = b.getX();
                int iy = b.getY();
                if (iy < playerY) continue;
                int dx = ix - playerX;
                int dy = iy - playerY;
                int dist = dx * dx + dy * dy;
                if (dist < bestDist) {
                    bestDist = dist; x = ix; y = iy; found = true;
                }
            }
            return found;
        }
    }
}
