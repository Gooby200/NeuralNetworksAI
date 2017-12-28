using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeuralNetworkAI {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            //create and run a task that will draw into the picturebox (rename the picturebox though)
            //the task will constantly draw a grid. each cell will be 10px by 10px and the background will
            //be green in color (or whatever color). for the beginning, lets draw a border around each cell
            //just so we can see whats going on
        }
    }
}
