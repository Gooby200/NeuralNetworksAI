using System;

namespace NeuralNetworkAI {
    class NeuralNetwork {
        public static double sigmoid(double x, bool deriv = false) {
            return deriv ? x * (1 - x) : 1 / (1 + Math.Exp(-x));
        }

        public static double[][] sigmoid(double[][] x, bool deriv = false) {
            double[][] tmp = new double[x.Length][];
            for (int r = 0; r < x.Length; r++) {
                tmp[r] = new double[x[r].Length];
                for (int c = 0; c < x[r].Length; c++) {
                    tmp[r][c] = deriv ? x[r][c] * (1 - x[r][c]) : 1 / (1 + Math.Exp(-x[r][c]));
                }
            }
            return tmp;
        }

        public static double[][] randomWeight(int rows, int cols, Random rnd) {
            double[][] tmp = new double[rows][];
            for (int r = 0; r < rows; r++) {
                tmp[r] = new double[cols];
                for (int c = 0; c < cols; c++) {
                    tmp[r][c] = 2 * rnd.NextDouble() - 1;
                }
            }
            return tmp;
        }

        public static double[][] dot(double[][] a, double[][] b) {
            double[][] tmp = new double[a.Length][];
            for (int r = 0; r < a.Length; r++) {
                tmp[r] = new double[b[0].Length];
            }

            for (int i = 0; i < a.Length; i++) {
                for (int j = 0; j < b[0].Length; j++) {
                    double sum = 0;
                    for (int k = 0; k < a[0].Length; k++) {
                        sum += a[i][k] * b[k][j];
                    }
                    tmp[i][j] = sum;
                }
            }
            return tmp;
        }

        public static double[][] transpose(double[][] a) {
            double[][] tmp = new double[a[0].Length][];
            for (int r = 0; r < a[0].Length; r++) {
                tmp[r] = new double[a.Length];
                for (int c = 0; c < a.Length; c++) {
                    tmp[r][c] = a[c][r];
                }
            }
            return tmp;
        }

        public static double forward(double[][] inputs, double[][] weights, double b) {
            double z = b;
            for (int r = 0; r < inputs.Length; r++) {
                for (int c = 0; c < inputs[0].Length; c++) {
                    z += inputs[r][c] * weights[r][c];
                }
            }
            return sigmoid(z);
        }
    }
}
