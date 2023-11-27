namespace PathFindingProblemWithGeneticAlgorithm
{
    class Program
    {
        static int gridSizeX = 10;
        static int gridSizeY = 10;
        static char[,] grid;
        static Random random = new Random();

        static void Main()
        {
            InitializeGrid();
            AddWalls();
            DisplayGrid();

            List<int[]> pathList = GeneticAlgorithm();
            int[] path = pathList.First();

            Console.WriteLine("\nPath found:");
            DisplayGridWithPath(path);

            Console.ReadKey();
        }

        // Step 1: Initialize the grid
        static void InitializeGrid()
        {
            grid = new char[gridSizeX, gridSizeY];
            for (int i = 0; i < gridSizeX; i++)
            {
                for (int j = 0; j < gridSizeY; j++)
                {
                    grid[i, j] = ' ';
                }
            }
        }

        // Step 2: Add walls to the grid
        static void AddWalls()
        {
            grid[2, 1] = '#';
            grid[2, 2] = '#';
            grid[2, 3] = '#';
            grid[5, 4] = '#';
            grid[5, 5] = '#';
            grid[5, 6] = '#';
        }

        // Step 3: Display the current state of the grid
        static void DisplayGrid()
        {
            Console.Clear();
            for (int i = 0; i < gridSizeX; i++)
            {
                for (int j = 0; j < gridSizeY; j++)
                {
                    Console.Write(grid[i, j] + " ");
                }
                Console.WriteLine();
            }
            Thread.Sleep(300);
        }

        // Step 4: Initialize the population of paths
        static List<int[]> InitializePopulation(int size)
        {
            List<int[]> population = new List<int[]>();

            for (int i = 0; i < size; i++)
            {
                int[] path = new int[gridSizeX * gridSizeY];
                for (int j = 0; j < path.Length; j++)
                {
                    path[j] = random.Next(4); // 0: up, 1: down, 2: left, 3: right
                }
                population.Add(path);
            }

            return population;
        }

        // Step 5: Evaluate the fitness of individuals in the population
        static int FitnessFunction(int[] path)
        {
            int x = 0, y = 0;
            int fitness = 0;

            foreach (var direction in path)
            {
                switch (direction)
                {
                    case 0: // Up
                        if (y > 0 && grid[x, y - 1] != '#')
                        {
                            y--;
                            fitness++;
                        }
                        break;
                    case 1: // Down
                        if (y < gridSizeY - 1 && grid[x, y + 1] != '#')
                        {
                            y++;
                            fitness++;
                        }
                        break;
                    case 2: // Left
                        if (x > 0 && grid[x - 1, y] != '#')
                        {
                            x--;
                            fitness++;
                        }
                        break;
                    case 3: // Right
                        if (x < gridSizeX - 1 && grid[x + 1, y] != '#')
                        {
                            x++;
                            fitness++;
                        }
                        break;
                }
            }

            return fitness;
        }

        // Step 6: Display the path
        static void DisplayGridWithPath(int[] path)
        {
            char[,] gridWithSolution = (char[,])grid.Clone(); // Create a copy of the original grid

            int x = 0, y = 0;

            foreach (var direction in path)
            {
                // Update coordinates based on the direction
                switch (direction)
                {
                    case 0: // Up
                        if (y > 0)
                            y--;
                        break;
                    case 1: // Down
                        if (y < gridSizeY - 1)
                            y++;
                        break;
                    case 2: // Left
                        if (x > 0)
                            x--;
                        break;
                    case 3: // Right
                        if (x < gridSizeX - 1)
                            x++;
                        break;
                }

                // Check bounds before updating the grid
                if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
                {
                    gridWithSolution[x, y] = '*'; // Mark the path on the grid
                }
            }

            // Display the final grid with the path as a line
            Console.Clear();
            for (int i = 0; i < gridSizeX; i++)
            {
                for (int j = 0; j < gridSizeY; j++)
                {
                    Console.Write(gridWithSolution[i, j] == '*' ? '*' : ' '); // Show only the connected line of '*'
                }
                Console.WriteLine();
            }
        }

        // Step 7: Implement the genetic algorithm
        static List<int[]> GeneticAlgorithm()
        {
            int populationSize = 100;
            int generations = 100;
            double mutationRate = 0.1;

            List<int[]> population = InitializePopulation(populationSize);

            for (int generation = 0; generation < generations; generation++)
            {
                var fitnessScores = population.Select(FitnessFunction).ToList();
                var selectedParents = SelectParents(population, fitnessScores);
                var newPopulation = CrossoverAndMutate(selectedParents, mutationRate);
                population = newPopulation;

                // Display intermediate results with a longer delay
                Console.WriteLine($"Generation {generation + 1}");
                DisplayGrid();
                System.Threading.Thread.Sleep(500); // Adjust the delay as needed
            }

            // Display the final grid with the best path
            var bestPath = population.OrderByDescending(FitnessFunction).First();
            DisplayGridWithPath(bestPath);

            // Return the best path found wrapped in a list
            return new List<int[]> { bestPath };
        }

        // Helper method: Select parents based on fitness scores
        static List<int[]> SelectParents(List<int[]> population, List<int> fitnessScores)
        {
            // Use tournament selection for simplicity
            int tournamentSize = 5;
            List<int[]> selectedParents = new List<int[]>();

            for (int i = 0; i < population.Count; i++)
            {
                int[] tournament = new int[tournamentSize];
                for (int j = 0; j < tournamentSize; j++)
                {
                    tournament[j] = random.Next(population.Count);
                }

                int winnerIndex = tournament.OrderByDescending(index => fitnessScores[index]).First();
                selectedParents.Add(population[winnerIndex]);
            }

            return selectedParents;
        }

        // Helper method: Crossover and mutate selected parents
        static List<int[]> CrossoverAndMutate(List<int[]> parents, double mutationRate)
        {
            List<int[]> newPopulation = new List<int[]>();

            for (int i = 0; i < parents.Count; i += 2)
            {
                int crossoverPoint = random.Next(parents[i].Length);

                int[] child1 = parents[i].Take(crossoverPoint).Concat(parents[i + 1].Skip(crossoverPoint)).ToArray();
                int[] child2 = parents[i + 1].Take(crossoverPoint).Concat(parents[i].Skip(crossoverPoint)).ToArray();

                Mutate(child1, mutationRate);
                Mutate(child2, mutationRate);

                newPopulation.Add(child1);
                newPopulation.Add(child2);
            }

            return newPopulation;
        }

        // Helper method: Mutate the path based on the mutation rate
        static void Mutate(int[] path, double mutationRate)
        {
            for (int i = 0; i < path.Length; i++)
            {
                if (random.NextDouble() < mutationRate)
                {
                    path[i] = random.Next(4); // Mutate by randomly changing direction
                }
            }
        }
    }

}
