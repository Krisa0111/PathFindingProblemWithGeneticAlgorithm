
using System.Windows;
using System.Windows.Controls;

namespace MazeSolver
{
    public class GeneticAlgorithm
    {
        public const int PopulationSize = 100;
        public const double MutationRate = 0.01;
        


        public MainWindow mainWindow;
        public TextBlock generationTextBlock;

        public GeneticAlgorithm(MainWindow mainWindow, TextBlock generationTextBlock)
        {
            this.mainWindow = mainWindow;
            this.generationTextBlock = generationTextBlock;
        }

        public List<int[]> Solve(Maze maze, int maxIterations)
        {
            List<int[]> population = InitializePopulation(maze);
            Random random = new Random();

            for (int generation = 0; generation < maxIterations; generation++)
            {
                List<int[]> nextGeneration = new List<int[]>();

                for (int i = 0; i < PopulationSize; i++)
                {
                    int[] parent1 = SelectParent(population, random);
                    int[] parent2 = SelectParent(population, random);
                    int[] child = Crossover(parent1, parent2, random);
                    Mutate(child, random);
                    nextGeneration.Add(child);
                }

                population = nextGeneration;

                // Visualize the current best path and generation
                int[] bestPath = population.OrderBy(p => CalculateFitness(p, maze)).First();
                VisualizePath(bestPath, maze, generation);
            }

            return population;
        }



        public List<int[]> InitializePopulation(Maze maze)
        {
            Random random = new Random();
            List<int[]> population = new List<int[]>();

            for (int i = 0; i < PopulationSize; i++)
            {
                int[] path = Enumerable.Repeat(0, maze.Grid.GetLength(0) * maze.Grid.GetLength(1)).Select(_ => random.Next(4)).ToArray();
                population.Add(path);
            }

            return population;
        }

        public int[] SelectParent(List<int[]> population, Random random)
        {
            return population[random.Next(population.Count)];
        }

        public int[] Crossover(int[] parent1, int[] parent2, Random random)
        {
            int crossoverPoint = random.Next(parent1.Length);
            int[] child = parent1.Take(crossoverPoint).Concat(parent2.Skip(crossoverPoint)).ToArray();
            return child;
        }

        public void Mutate(int[] path, Random random)
        {
            for (int i = 0; i < path.Length; i++)
            {
                if (random.NextDouble() < MutationRate)
                {
                    path[i] = random.Next(4);
                }
            }
        }

        public double CalculateFitness(int[] path, Maze maze)
        {
            int currentX = maze.StartX;
            int currentY = maze.StartY;

            foreach (int direction in path)
            {
                switch (direction)
                {
                    case 0:
                        currentX++;
                        break;
                    case 1:
                        currentX--;
                        break;
                    case 2:
                        currentY++;
                        break;
                    case 3:
                        currentY--;
                        break;
                }

                if (maze.Grid[currentX, currentY] == 1)
                {
                    return 0.0;
                }
            }

            double distance = Math.Sqrt(Math.Pow(currentX - maze.EndX, 2) + Math.Pow(currentY - maze.EndY, 2));
            return 1.0 / (distance + 1);
        }

        public void VisualizePath(int[] path, Maze maze, int generation)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                mainWindow.ClearCanvas();
                mainWindow.DrawMaze();

                int currentX = maze.StartX;
                int currentY = maze.StartY;

                for (int i = 0; i < path.Length; i++)
                {
                    int nextX = currentX;
                    int nextY = currentY;

                    switch (path[i])
                    {
                        case 0:
                            nextX++;
                            break;
                        case 1:
                            nextX--;
                            break;
                        case 2:
                            nextY++;
                            break;
                        case 3:
                            nextY--;
                            break;
                    }

                    // Check if the next move hits a wall
                    if (nextX >= 0 && nextX < maze.Grid.GetLength(0) &&
                        nextY >= 0 && nextY < maze.Grid.GetLength(1) &&
                        maze.Grid[nextX, nextY] != 1)
                    {
                        currentX = nextX;
                        currentY = nextY;
                    }

                    mainWindow.DrawMovement(currentX, currentY);
                    Thread.Sleep(1);
                }

                generationTextBlock.Text = $"Generation: {generation}";
            });
        }

    }

}
