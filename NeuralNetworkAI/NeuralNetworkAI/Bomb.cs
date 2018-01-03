using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetworkAI {


    class Bomb {
        private int width;
        private int height;
        private int x;
        private int y;
        private Color color;

        //default bomb
        public Bomb() : this(0, 0) { }

        public Bomb(int x, int y) : this(10, 10, x, y, Color.Black) { }

        public Bomb(int width, int height, int x, int y, Color color) {
            this.width = width;
            this.height = height;
            this.x = x;
            this.y = y;
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
