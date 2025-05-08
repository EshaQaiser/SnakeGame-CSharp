using Microsoft.EntityFrameworkCore;
using Snake.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snake
{

    public class GameState
    {
        private string playerName;

        public int Rows { get; }
        public int Columns { get; }
        public GridValue[,] Grid { get; }
        public Direction Dir { get; private set; }
        public int Score { get; private set; }
        public bool GameOver { get; private set; }
        public string PlayerName { get; }

        private readonly LinkedList<Direction> dirChanges = new LinkedList<Direction>();
        private readonly LinkedList<Position> snakePositions = new LinkedList<Position>();
        private readonly Random random = new Random();
        private Position head;
        private int turnsSinceLastFood;  // Added semicolon at the end

        public GameState(int rows, int cols, string playerName)
        {
            Rows = rows;
            Columns = cols;
            Grid = new GridValue[Rows, Columns];
            Dir = Direction.Right;

            AddSnake();
            AddFood();
            head = new Position(0, 0);  // Initialize head position (example, replace with actual initialization)

            turnsSinceLastFood = 0;
            this.PlayerName = playerName;
        }

        private void AddSnake()
        {
            int r = Rows / 2;

            for (int c = 1; c <= 3; c++)
            {
                Grid[r, c] = GridValue.Snake;
                snakePositions.AddFirst(new Position(r, c));
            }
        }

        private IEnumerable<Position> EmptyPositions()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    if (Grid[r, c] == GridValue.Empty)
                    {
                        yield return new Position(r, c);
                    }
                }
            }
        }

        private void AddFood()
        {
            List<Position> empty = new List<Position>(EmptyPositions());

            if (empty.Count == 0)
            {
                return;
            }

            Position pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Column] = GridValue.Food;
        }

        public Position HeadPosition()
        {
            return snakePositions.First.Value;
        }

        public Position TailPoition()
        {
            return snakePositions.Last.Value;
        }

        public IEnumerable<Position> SnakePositions()
        {
            return snakePositions;
        }

        private void AddHead(Position pos)
        {
            snakePositions.AddFirst(pos);
            Grid[pos.Row, pos.Column] = GridValue.Snake;
        }

        private void RemoveTail()
        {
            Position tail = snakePositions.Last.Value;
            Grid[tail.Row, tail.Column] = GridValue.Empty;
            snakePositions.RemoveLast();
        }

        private Direction GetLastDirection()
        {
            if (dirChanges.Count == 0)
            {
                return Dir;
            }
            return dirChanges.Last.Value;
        }

        private bool CanChangeDirection(Direction newDir)
        {
            if (dirChanges.Count == 2)
            {
                return false;
            }
            Direction lastDir = GetLastDirection();
            return newDir != lastDir && newDir != lastDir.Opposite();
        }

        public void ChangeDirection(Direction dir)
        {
            if (CanChangeDirection(dir))
            {
                dirChanges.AddLast(dir);
            }
        }

        private bool OutsideGrid(Position pos)
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Column < 0 || pos.Column >= Columns;
        }

        private GridValue WillHit(Position newHeadPos)
        {
            if (OutsideGrid(newHeadPos))
            {
                return GridValue.Outside;
            }

            if (newHeadPos == TailPoition())
            {
                return GridValue.Empty;
            }
            return Grid[newHeadPos.Row, newHeadPos.Column];
        }

        public void Move()
        {
            if (dirChanges.Count > 0)
            {
                Dir = dirChanges.First.Value;
                dirChanges.RemoveFirst();
            }
            Position newHeadPos = HeadPosition().Translate(Dir);
            GridValue hit = WillHit(newHeadPos);

            if (hit == GridValue.Outside || hit == GridValue.Snake)
            {
                GameOver = true;
            }
            else if (hit == GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPos);
                head = newHeadPos;  // Update the head position
            }
            else if (hit == GridValue.Food)
            {
                AddHead(newHeadPos);
                Score++;
                AddFood();
                head = newHeadPos;  // Update the head position
            }

            if (Grid[head.Row, head.Column] == GridValue.Food)
            {
                // Increment score and reset turnsSinceLastFood
                Score++;
                turnsSinceLastFood = 0;
                // ... (existing code)
            }
            else
            {
                turnsSinceLastFood++;  // Increment turns since last food
                // ... (existing code)
            }
        }

        public int TurnsSinceLastFood
        {
            get { return turnsSinceLastFood; }
        }


        public void EndGame()
        {
            var options = new DbContextOptionsBuilder<SnakeDbContext>()
                .UseSqlServer("Server=DESKTOP-BE80RDB\\SQLEXPRESS;Database=SnakeGameDB;Trusted_Connection=True;")
                .Options;

            using (var context = new SnakeDbContext(options))
            {
                var player = new Player
                {
                    Name = playerName, // Use the playerName obtained in MainWindow
                    Score = Score,
                    Turns = TurnsSinceLastFood,
                    HighestScore = GetHighestScore(playerName)
                };

                context.Players.Add(player);
                context.SaveChanges();
            }
        }


        private int GetHighestScore(string playerName)
        {
            var options = new DbContextOptionsBuilder<SnakeDbContext>()
                .UseSqlServer("Server=DESKTOP-BE80RDB\\SQLEXPRESS;Database=SnakeGameDB;Trusted_Connection=True;")
                .Options;

            using (var context = new SnakeDbContext(options))
            {
                var player = context.Players.FirstOrDefault(p => p.Name == playerName);
                return player?.HighestScore ?? 0;
            }
        }

    }
}
