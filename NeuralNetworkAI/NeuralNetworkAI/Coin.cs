using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetworkAI {
    class Coin {
        private int width;
        private int height;
        private int x;
        private int y;
        private int pointValue;
        private Color color;

        //default coin
        public Coin() : this(10, 10, 0, 0, 1, Color.Yellow) { }

        //this constructor is here if we want to do anything crazy, like do things with points and different points are different colors
        public Coin(int width, int height, int x, int y, int pointValue, Color color) {
            this.width = width;
            this.height = height;
            this.x = x;
            this.y = y;
            this.pointValue = pointValue;
            this.color = color;
        }

        private void setLocation(int x, int y) {
            this.x = x;
            this.y = y;
        }

        private int getX() {
            return x;
        }

        private int getY() {
            return y;
        }

        private void setSize(int width, int height) {
            this.width = width;
            this.height = height;
        }

        private int getWidth() {
            return width;
        }

        private int getHeight() {
            return height;
        }

        private void setPointValue(int pointValue) {
            this.pointValue = pointValue;
        }

        private int getPointValue() {
            return pointValue;
        }

        private void setColor(Color color) {
            this.color = color;
        }

        private Color getColor() {
            return color;
        }

        private void draw(Graphics g) {
            g.FillRectangle(new SolidBrush(color), x, y, width, height);
        }
    }
}
