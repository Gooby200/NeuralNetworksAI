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
        public Coin() : this(0, 0) { }

        public Coin(int x, int y) : this(10, 10, x, y, 1, Color.Yellow) { }

        //this constructor is here if we want to do anything crazy, like do things with points and different points are different colors
        public Coin(int width, int height, int x, int y, int pointValue, Color color) {
            this.width = width;
            this.height = height;
            this.x = x;
            this.y = y;
            this.pointValue = pointValue;
            this.color = color;
        }

        public void setLocation(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public int getX() {
            return x;
        }

        public int getY() {
            return y;
        }

        public void setSize(int width, int height) {
            this.width = width;
            this.height = height;
        }

        public int getWidth() {
            return width;
        }

        public int getHeight() {
            return height;
        }

        public void setPointValue(int pointValue) {
            this.pointValue = pointValue;
        }

        public int getPointValue() {
            return pointValue;
        }

        public void setColor(Color color) {
            this.color = color;
        }

        public Color getColor() {
            return color;
        }

        public void draw(Graphics g) {
            g.FillRectangle(new SolidBrush(color), x, y, width, height);
        }
    }
}
