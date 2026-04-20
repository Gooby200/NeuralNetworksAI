using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace NeuralNetworkAI {
    // Feedforward sigmoid MLP used as the game's AI player. Weights and biases are
    // plain double arrays so the GA trainer can read/mutate them cheaply.
    class Agent {
        public readonly int[] topology;
        public double[][][] weights; // [layer][outputNode][inputNode]
        public double[][] biases;    // [layer][outputNode]

        public Agent(int[] topology) {
            if (topology == null || topology.Length < 2) {
                throw new ArgumentException("Topology needs at least input and output layers.");
            }
            this.topology = topology;
            int layers = topology.Length - 1;
            weights = new double[layers][][];
            biases = new double[layers][];
            for (int l = 0; l < layers; l++) {
                int outN = topology[l + 1];
                int inN = topology[l];
                weights[l] = new double[outN][];
                biases[l] = new double[outN];
                for (int o = 0; o < outN; o++) {
                    weights[l][o] = new double[inN];
                }
            }
        }

        public static Agent CreateRandom(int[] topology, Random rnd) {
            Agent a = new Agent(topology);
            for (int l = 0; l < a.weights.Length; l++) {
                for (int o = 0; o < a.weights[l].Length; o++) {
                    for (int i = 0; i < a.weights[l][o].Length; i++) {
                        a.weights[l][o][i] = 2 * rnd.NextDouble() - 1;
                    }
                    a.biases[l][o] = 2 * rnd.NextDouble() - 1;
                }
            }
            return a;
        }

        public Agent Clone() {
            Agent c = new Agent(topology);
            for (int l = 0; l < weights.Length; l++) {
                for (int o = 0; o < weights[l].Length; o++) {
                    Array.Copy(weights[l][o], c.weights[l][o], weights[l][o].Length);
                }
                Array.Copy(biases[l], c.biases[l], biases[l].Length);
            }
            return c;
        }

        public double[] Forward(double[] input) {
            double[] activations = input;
            for (int l = 0; l < weights.Length; l++) {
                double[] next = new double[weights[l].Length];
                for (int o = 0; o < weights[l].Length; o++) {
                    double z = biases[l][o];
                    double[] row = weights[l][o];
                    for (int i = 0; i < row.Length; i++) {
                        z += row[i] * activations[i];
                    }
                    next[o] = NeuralNetwork.sigmoid(z);
                }
                activations = next;
            }
            return activations;
        }

        public int Decide(double[] input) {
            double[] output = Forward(input);
            int best = 0;
            for (int i = 1; i < output.Length; i++) {
                if (output[i] > output[best]) best = i;
            }
            return best;
        }

        // Gaussian-ish mutation: perturb each parameter with probability `rate` by a
        // value drawn from a narrow normal distribution. Good enough for GA.
        public void Mutate(Random rnd, double rate, double scale) {
            for (int l = 0; l < weights.Length; l++) {
                for (int o = 0; o < weights[l].Length; o++) {
                    double[] row = weights[l][o];
                    for (int i = 0; i < row.Length; i++) {
                        if (rnd.NextDouble() < rate) row[i] += SampleNormal(rnd) * scale;
                    }
                    if (rnd.NextDouble() < rate) biases[l][o] += SampleNormal(rnd) * scale;
                }
            }
        }

        public static Agent Crossover(Agent a, Agent b, Random rnd) {
            Agent child = new Agent(a.topology);
            for (int l = 0; l < a.weights.Length; l++) {
                for (int o = 0; o < a.weights[l].Length; o++) {
                    for (int i = 0; i < a.weights[l][o].Length; i++) {
                        child.weights[l][o][i] = rnd.NextDouble() < 0.5
                            ? a.weights[l][o][i]
                            : b.weights[l][o][i];
                    }
                    child.biases[l][o] = rnd.NextDouble() < 0.5 ? a.biases[l][o] : b.biases[l][o];
                }
            }
            return child;
        }

        private static double SampleNormal(Random rnd) {
            // Box-Muller
            double u1 = 1.0 - rnd.NextDouble();
            double u2 = 1.0 - rnd.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }

        // Simple human-readable format: first line is comma-separated topology,
        // then one line per layer with "w:..." and "b:..." values joined by commas.
        public void SaveToFile(string path) {
            StringBuilder sb = new StringBuilder();
            sb.Append("topology:");
            for (int i = 0; i < topology.Length; i++) {
                if (i > 0) sb.Append(",");
                sb.Append(topology[i].ToString(CultureInfo.InvariantCulture));
            }
            sb.AppendLine();
            for (int l = 0; l < weights.Length; l++) {
                sb.Append("w").Append(l).Append(":");
                bool first = true;
                for (int o = 0; o < weights[l].Length; o++) {
                    for (int i = 0; i < weights[l][o].Length; i++) {
                        if (!first) sb.Append(",");
                        sb.Append(weights[l][o][i].ToString("R", CultureInfo.InvariantCulture));
                        first = false;
                    }
                }
                sb.AppendLine();
                sb.Append("b").Append(l).Append(":");
                for (int o = 0; o < biases[l].Length; o++) {
                    if (o > 0) sb.Append(",");
                    sb.Append(biases[l][o].ToString("R", CultureInfo.InvariantCulture));
                }
                sb.AppendLine();
            }
            File.WriteAllText(path, sb.ToString());
        }

        public static Agent LoadFromFile(string path) {
            string[] lines = File.ReadAllLines(path);
            int[] topology = null;
            Dictionary<string, double[]> values = new Dictionary<string, double[]>();
            foreach (string raw in lines) {
                string line = raw.Trim();
                if (line.Length == 0) continue;
                int colon = line.IndexOf(':');
                if (colon < 0) continue;
                string key = line.Substring(0, colon);
                string rest = line.Substring(colon + 1);
                if (key == "topology") {
                    string[] parts = rest.Split(',');
                    topology = new int[parts.Length];
                    for (int i = 0; i < parts.Length; i++) {
                        topology[i] = int.Parse(parts[i], CultureInfo.InvariantCulture);
                    }
                } else {
                    string[] parts = rest.Split(',');
                    double[] nums = new double[parts.Length];
                    for (int i = 0; i < parts.Length; i++) {
                        nums[i] = double.Parse(parts[i], CultureInfo.InvariantCulture);
                    }
                    values[key] = nums;
                }
            }
            if (topology == null) throw new InvalidDataException("Missing topology line.");
            Agent a = new Agent(topology);
            for (int l = 0; l < a.weights.Length; l++) {
                double[] w = values["w" + l];
                int idx = 0;
                for (int o = 0; o < a.weights[l].Length; o++) {
                    for (int i = 0; i < a.weights[l][o].Length; i++) {
                        a.weights[l][o][i] = w[idx++];
                    }
                }
                double[] bs = values["b" + l];
                for (int o = 0; o < bs.Length; o++) {
                    a.biases[l][o] = bs[o];
                }
            }
            return a;
        }
    }
}
