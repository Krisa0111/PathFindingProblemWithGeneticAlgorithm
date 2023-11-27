using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MazeSolver
{
    public partial class MainWindow : Window
    {
        public const int MazeSize = 10;
        public const int CellSize = 30;
        public bool stopAlgorithm = false;
        private int[] bestSolution;

        public Maze maze;
        public GeneticAlgorithm geneticAlgorithm;
        private TextBlock generationTextBlock;
        public MainWindow()
        {
            InitializeComponent();
            InitializeMaze();

            generationTextBlock = GenerationText;

            InitializeGeneticAlgorithm();
        }

        public void InitializeMaze()
        {
            int[,] grid = new int[MazeSize, MazeSize];

            // Initialize maze with walls (1), open cells (0), start (2), and end (3)
            for (int i = 0; i < MazeSize; i++)
            {
                for (int j = 0; j < MazeSize; j++)
                {
                    grid[i, j] = (i == 0 || i == MazeSize - 1 || j == 0 || j == MazeSize - 1) ? 1 : 0;
                }
            }

            // Add walls to the maze
            grid[3, 2] = 1;
            grid[4, 2] = 1;
            grid[5, 2] = 1;
            grid[6, 2] = 1;
            grid[6, 3] = 1;
            grid[6, 4] = 1;
            grid[6, 5] = 1;
            grid[5, 5] = 1;

            grid[2, 2] = 1;
            grid[7, 2] = 1;
            grid[5, 3] = 1;
            grid[1, 4] = 1;
            grid[2, 4] = 1;
            grid[3, 4] = 1;
            grid[8, 4] = 1;
            grid[3, 5] = 1;
            grid[5, 6] = 1;
            grid[7, 6] = 1;
            grid[2, 7] = 1;
            grid[3, 7] = 1;
            grid[4, 7] = 1;
            grid[5, 7] = 1;
            grid[7, 8] = 1;

            maze = new Maze(grid, 1, 1, MazeSize - 2, MazeSize - 2);
            DrawMaze();
        }


        public void InitializeGeneticAlgorithm()
        {
            geneticAlgorithm = new GeneticAlgorithm(this, GenerationText);
        }

        public void DrawMaze()
        {
            MazeCanvas.Children.Clear();

            for (int i = 0; i < MazeSize; i++)
            {
                for (int j = 0; j < MazeSize; j++)
                {
                    Rectangle rect = new Rectangle
                    {
                        Width = CellSize,
                        Height = CellSize,
                        Fill = maze.Grid[i, j] == 1 ? Brushes.Black : Brushes.White,
                        Stroke = Brushes.Gray,
                        StrokeThickness = 1
                    };

                    Canvas.SetLeft(rect, j * CellSize);
                    Canvas.SetTop(rect, i * CellSize);

                    MazeCanvas.Children.Add(rect);
                }
            }

            // Highlight the start and destination points
            DrawHighlight(maze.StartX, maze.StartY, Brushes.Green);
            DrawHighlight(maze.EndX, maze.EndY, Brushes.Red);
        }
        public void DrawHighlight(int x, int y, Brush color)
        {
            Ellipse ellipse = new Ellipse
            {
                Width = CellSize / 2,
                Height = CellSize / 2,
                Fill = color,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };

            Canvas.SetLeft(ellipse, y * CellSize + CellSize / 4);
            Canvas.SetTop(ellipse, x * CellSize + CellSize / 4);

            MazeCanvas.Children.Add(ellipse);
        }

        public async void SolveMazeButton_Click(object sender, RoutedEventArgs e)
        {
            PrepareVisualization();
            await SolveMazeAsync(maze); // Adjust the number of iterations as needed
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            stopAlgorithm = true;
        }


        private async Task SolveMazeAsync(Maze maze)
        {
            List<int[]> population = geneticAlgorithm.InitializePopulation(maze);
            Random random = new Random();
            int generation = 0;
            int[] bestSolution = null;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100); // Adjust the delay here (e.g., 100 milliseconds)
            timer.Tick += async (s, e) =>
            {
                if (!stopAlgorithm)
                {
                    List<int[]> nextGeneration = new List<int[]>();

                    for (int i = 0; i < GeneticAlgorithm.PopulationSize; i++)
                    {
                        int[] parent1 = geneticAlgorithm.SelectParent(population, random);
                        int[] parent2 = geneticAlgorithm.SelectParent(population, random);
                        int[] child = geneticAlgorithm.Crossover(parent1, parent2, random);
                        geneticAlgorithm.Mutate(child, random);
                        nextGeneration.Add(child);
                    }

                    population = nextGeneration;

                    // Update the best solution
                    int[] currentBestPath = population.OrderBy(p => geneticAlgorithm.CalculateFitness(p, maze)).First();
                    if (bestSolution == null || geneticAlgorithm.CalculateFitness(currentBestPath, maze) < geneticAlgorithm.CalculateFitness(bestSolution, maze))
                    {
                        bestSolution = currentBestPath;
                    }

                    // Visualize the current best path and generation
                    geneticAlgorithm.VisualizePath(currentBestPath, maze, generation);

                    generation++;
                }
                else
                {
                    timer.Stop();

                    // Visualize the best solution found after stopping
                    if (bestSolution != null && HasReachedDestination(bestSolution, maze))
                    {
                        geneticAlgorithm.VisualizePath(bestSolution, maze, generation);
                    }
                }
            };

            timer.Start();

            // Wait for the timer to complete
            await Task.Delay(Timeout.Infinite);
        }

        private bool HasReachedDestination(int[] path, Maze maze)
        {
            int currentX = maze.StartX;
            int currentY = maze.StartY;

            for (int i = 0; i < path.Length; i++)
            {
                switch (path[i])
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

                if (currentX == maze.EndX && currentY == maze.EndY)
                {
                    return true;
                }

                if (currentX < 0 || currentX >= maze.Grid.GetLength(0) ||
                    currentY < 0 || currentY >= maze.Grid.GetLength(1) ||
                    maze.Grid[currentX, currentY] == 1)
                {
                    // Hit a wall or went out of bounds
                    return false;
                }
            }

            return false; // Destination not reached
        }



        public void PrepareVisualization()
        {
            ClearCanvas();
            DrawMaze();
        }

        public void ClearCanvas()
        {
            MazeCanvas.Children.Clear();
        }

        public void VisualizePath(int[] path)
        {
            int currentX = maze.StartX;
            int currentY = maze.StartY;

            for (int i = 0; i < path.Length; i++)
            {
                switch (path[i])
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

                DrawMovement(currentX, currentY);
                Thread.Sleep(100); // Add a delay for visualization
            }
        }

        public void DrawMovement(int x, int y)
        {
            DrawHighlight(x, y, Brushes.Red);
        }
    }
}
