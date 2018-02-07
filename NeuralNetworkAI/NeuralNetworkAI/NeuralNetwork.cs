using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace NeuralNetworkAI {
    class NeuralNetwork {
        public static double sigmoid(double x, bool deriv = false) {
            //return sigmod of individual value
            return deriv ? x * (1 - x) : 1 / (1 + Math.Exp(-x));
        }

        public static double[][] sigmoid(double[][] x, bool deriv = false) {
            double[][] tmp = new double[x[0].Length][];
            for (int r = 0; r < x[0].Length; r++) {
                tmp[r] = new double[x.Length];
            }

            for (int r = 0; r < tmp.Length; r++) {
                for (int c = 0; c < tmp[0].Length; c++) {
                    tmp[r][c] = deriv ? x[r][c] * (1 - x[r][c]) : 1 / (1 + Math.Exp(-x[r][c]));
                }
            }

            //return sigmod of a matrix
            return tmp;
        }

        public static double[][] randomWeight(int rows, int cols) {
            double[][] tmp = new double[rows][];
            for (int r = 0; r < rows; r++) {
                tmp[r] = new double[cols];
            }

            Random rnd = new Random(1);
            for (int r = 0; r < rows; r++) {
                for (int c = 0; c < cols; c++) {
                    tmp[r][c] = 2 * rnd.NextDouble() - 1;
                }
            }

            //return random weights in matrix
            return tmp;
        }

        public static double[][] dot(double[][] a, double[][] b) {
            double[][] tmp = new double[a.Length][];
            double sum = 0;

            for (int r = 0; r < a.Length; r++) {
                tmp[r] = new double[b[0].Length];
            }

            for (int i = 0; i < a.Length; i++) {
                for (int j = 0; j < b[0].Length; j++) {
                    sum = 0;
                    for (int k = 0; k < a[0].Length; k++) {
                        sum += a[i][k] * b[k][j];
                    }
                    tmp[i][j] = sum;
                }
            }

            //return the matrix multiplication
            return tmp;
        }

        public static double[][] transpose(double[][] a) {
            double[][] tmp = new double[a[0].Length][];
            for (int r = 0; r < a[0].Length; r++) {
                tmp[r] = new double[a.Length];
            }

            for (int r = 0; r < tmp.Length; r++) {
                for (int c = 0; c < tmp[0].Length; c++) {
                    tmp[r][c] = a[c][r];
                }
            }

            //return the transpose of the matrix
            return tmp;
        }

        //public static int[] twoLayerNeuralNetwork(double[][] dataset) {
        //    double[][] weights = NeuralNetwork.randomWeight(3, 1);
        //    double[][] X = dataset;
        //    double[][] l0;
        //    double[][] l1;
        //    double l1_error = 0;

        //    for (int i = 0; i < 10000; i++) {
        //        l0 = X;
        //        l1 = NeuralNetwork.sigmoid(NeuralNetwork.dot(l0, weights));
        //        l1_error = 
        //    }
        //}

        public static double foo(double[][] inputs, double[][] weights, double b) {
            double z = b;
            for (int r = 0; r < inputs.Length; r++) {
                for (int c = 0; c < inputs[0].Length; c++) {
                    z += inputs[r][c] * weights[r][c];
                }
            }

            return NeuralNetwork.sigmoid(z);
        }
    }
}
