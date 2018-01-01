using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetworkAI {
    class Player {
        private int width;
        private int height;
        private int x;
        private int y;
        private Color color;
        private int points;

        public Player() : this(0, 0) { }

        public Player(int x, int y) : this(10, 10, x, y, Color.Red) { }

        public Player(int width, int height, int x, int y, Color color) {
            this.width = width;
            this.height = height;
            this.x = x;
            this.y = y;
            this.color = color;
        }

        public Color getColor() {
            return color;
        }

        public void setColor(Color color) {
            this.color = color;
        }

        public int getY() {
            return y;
        }

        public void setY(int y) {
            this.y = y;
        }

        public int getX() {
            return x;
        }

        public void setX(int x) {
            this.x = x;
        }

        public int getHeight() {
            return height;
        }

        public void setHeight(int height) {
            this.height = height;
        }

        public int getWidth() {
            return width;
        }

        public void setWidth(int width) {
            this.width = width;
        }

        public int getPoints() {
            return points;
        }

        public void addPoints(int points) {
            this.points += points;
        }

        public void clearPoints() {
            points = 0;
        }

        public bool eatCoin(Coin coin) {
            if (coin.getX() == x && coin.getY() == y) {
                return true;
            }
            return false;
        }

        public void moveLeft() {
            x -= width;
        }

        public void moveRight() {
            x += width;
        }

        public void draw(Graphics g) {
            g.FillRectangle(new SolidBrush(color), x, y, width, height);
        }
    }
}
