using System;
using System.Collections.Generic;
using System.Threading;

namespace NeuralNetworkAI {
    public class TrainerProgress {
        public int generation;
        public int population;
        public double bestScore;
        public double averageScore;
    }

    // Genetic-algorithm trainer. Each generation, every agent plays the same set of
    // seeded games and its fitness is the average score. Top survivors become parents
    // for the next generation; offspring are produced by crossover + mutation.
    class Trainer {
        public readonly int[] topology;
        public readonly int populationSize;
        public readonly int elitismCount;
        public readonly int gamesPerAgent;
        public readonly int columns;
        public readonly int rows;
        public readonly int cellWidth;
        public readonly int cellHeight;
        public readonly int simFps;
        public readonly int maxTicks;
        public readonly double mutationRate;
        public readonly double mutationScale;

        private List<Agent> population;
        private Random rnd;

        public Agent Best { get; private set; }
        public double BestScore { get; private set; }
        public int Generation { get; private set; }

        public Trainer(
            int[] topology, int columns, int rows, int cellWidth, int cellHeight,
            int populationSize = 30, int elitismCount = 6, int gamesPerAgent = 3,
            int simFps = 30, int maxTicks = 3000,
            double mutationRate = 0.1, double mutationScale = 0.3,
            int seed = 0
        ) {
            this.topology = topology;
            this.columns = columns;
            this.rows = rows;
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight;
            this.populationSize = populationSize;
            this.elitismCount = elitismCount;
            this.gamesPerAgent = gamesPerAgent;
            this.simFps = simFps;
            this.maxTicks = maxTicks;
            this.mutationRate = mutationRate;
            this.mutationScale = mutationScale;

            this.rnd = seed == 0 ? new Random() : new Random(seed);
            this.population = new List<Agent>(populationSize);
            for (int i = 0; i < populationSize; i++) {
                population.Add(Agent.CreateRandom(topology, rnd));
            }
        }

        public void RunGenerations(int generations, Action<TrainerProgress> onGeneration, CancellationToken ct) {
            for (int g = 0; g < generations; g++) {
                if (ct.IsCancellationRequested) return;
                StepGeneration();
                if (onGeneration != null) {
                    onGeneration(new TrainerProgress {
                        generation = Generation,
                        population = populationSize,
                        bestScore = BestScore,
                        averageScore = 0,
                    });
                }
            }
        }

        public void StepGeneration() {
            double[] fitness = new double[population.Count];
            // Same seeds for every agent this generation so comparisons are fair.
            int[] gameSeeds = new int[gamesPerAgent];
            for (int i = 0; i < gamesPerAgent; i++) gameSeeds[i] = rnd.Next(1, int.MaxValue);

            double totalScore = 0;
            int bestIdx = 0;
            double bestFitness = double.NegativeInfinity;
            for (int i = 0; i < population.Count; i++) {
                double score = 0;
                for (int s = 0; s < gamesPerAgent; s++) {
                    score += EvaluateAgent(population[i], gameSeeds[s]);
                }
                fitness[i] = score / gamesPerAgent;
                totalScore += fitness[i];
                if (fitness[i] > bestFitness) {
                    bestFitness = fitness[i];
                    bestIdx = i;
                }
            }

            Best = population[bestIdx].Clone();
            BestScore = bestFitness;
            Generation++;

            // Sort population by fitness descending to pick elites.
            int[] order = new int[population.Count];
            for (int i = 0; i < order.Length; i++) order[i] = i;
            Array.Sort(order, (x, y) => fitness[y].CompareTo(fitness[x]));

            List<Agent> nextGen = new List<Agent>(populationSize);
            for (int i = 0; i < elitismCount; i++) {
                nextGen.Add(population[order[i]].Clone());
            }

            while (nextGen.Count < populationSize) {
                Agent p1 = TournamentSelect(order, fitness);
                Agent p2 = TournamentSelect(order, fitness);
                Agent child = Agent.Crossover(p1, p2, rnd);
                child.Mutate(rnd, mutationRate, mutationScale);
                nextGen.Add(child);
            }
            population = nextGen;
        }

        private Agent TournamentSelect(int[] order, double[] fitness) {
            const int tournamentSize = 3;
            int bestIdx = -1;
            double bestFit = double.NegativeInfinity;
            for (int k = 0; k < tournamentSize; k++) {
                int candidate = order[rnd.Next(Math.Max(1, order.Length / 2))];
                if (fitness[candidate] > bestFit) {
                    bestFit = fitness[candidate];
                    bestIdx = candidate;
                }
            }
            return population[bestIdx];
        }

        // Reward is points earned plus a small survival bonus so agents that live
        // longer but haven't found coins yet still score above the ones that die instantly.
        private double EvaluateAgent(Agent agent, int seed) {
            GameSimulation sim = new GameSimulation(seed, columns, rows, cellWidth, cellHeight, simFps, maxTicks);
            while (!sim.done) {
                int action = agent.Decide(sim.GetFeatures());
                if (!sim.Step(action)) break;
            }
            return sim.points * 10 + sim.ticks * 0.01;
        }
    }
}
