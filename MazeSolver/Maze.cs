
namespace MazeSolver
{
    public class Maze
    {
        public int[,] Grid { get; set; }
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int EndX { get; set; }
        public int EndY { get; set; }

        public Maze(int[,] grid, int startX, int startY, int endX, int endY)
        {
            Grid = grid;
            StartX = startX;
            StartY = startY;
            EndX = endX;
            EndY = endY;
        }
    }

}
