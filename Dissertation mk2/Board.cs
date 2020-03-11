using System;
using System.Collections.Generic;

namespace Dissertation_mk2
{
    public class Board
    {
        [Serializable]
        public class Count
        {
            public int minimum;
            public int maximum;

            public Count(int min, int max)
            {
                minimum = min;
                maximum = max;
            }
        }

        public int columns = 25;
        public int rows = 25;
        public Count wallCount = new Count(24, 28);
        public Count itemCount = new Count(4, 5);
        public Count enemyCount = new Count(2, 4);

        public GameManager gameManager;
        public Markov markov = new Markov();
        public List<List<float>> board = new List<List<float>>();
        public List<List<int>> positions = new List<List<int>>();
        public List<List<int>> itemPositions = new List<List<int>>();
        public List<int> goalPos = new List<int>();

        public int score = 0;
        public int itemValue;

        public Board(GameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        public void BoardSetup()
        {
            goalPos.Add(0); goalPos.Add(columns - 1);
            for (int i = 0; i < columns; i++)
            {
                List<float> row = new List<float>();
                for (int j = 0; j < rows; j++)
                {
                    if (i == 0 || j == 0 || i == columns - 1 || j == rows - 1)
                    {
                        row.Add(0);
                    }
                    else
                    {
                        List<int> pos = new List<int> {i, j};
                        positions.Add(pos);
                        row.Add(0);
                    }
                }
                board.Add(row);
            }
        }

        private List<int> RandomPosition()
        {
            Random rand = new Random();
            int index = rand.Next(positions.Count - 1);
            List<int> pos = positions[index];
            positions.RemoveAt(index);
            return pos;
        }

        private void LayoutObjectAtRandom(float type, int minimum, int maximum)
        {
            Random rand = new Random();
            int objectCount = rand.Next(minimum, maximum + 1);

            for (int i = 0; i < objectCount; i++)
            {
                List<int> pos = RandomPosition();
                if ((Math.Abs(type - 4) < 1))
                {
                    float id = type + (i + 1f) / 10f;
                    board[pos[0]][pos[1]] = id;
                    Enemy enemy = new Enemy(this, id, pos);
                    gameManager.AddEnemyToList(enemy);
                }
                else if ((int)type == 2)
                {
                    board[pos[0]][pos[1]] = type;
                    itemPositions.Add(pos);
                }
                else
                {
                    board[pos[0]][pos[1]] = type;
                }
            }
        }

        private void InitialiseAllies()
        {
            int[][] allyPositions = { new[] { rows - 3, 0 }, new[] { rows - 2, 0 },
                new[] { rows - 1, 2 }, new[] { rows - 1, 1 }, new[] { rows - 1, 0 } };
            for (int i = 0; i < allyPositions.Length; i++)
            {
                List<int> allyPos = new List<int> {allyPositions[i][0], allyPositions[i][1]};
                float id = 5 + (i + 1f) / 10f;
                board[allyPos[0]][allyPos[1]] = id;
                Ally ally = new Ally(this, id, allyPos); 
                gameManager.AddAllyToList(ally);
            }
        }

        public float CheckPosition(List<int> position)
        {
            if (rows - 1 < position[0] || position[0] < 0 || columns - 1 < position[1] || position[1] < 0)
                Console.WriteLine(position[0] + " " + position[1]);
            if (position.Count == 2)
                return board[position[0]][position[1]];
            Console.WriteLine("Invalid CheckPosition, Count != 2");
            return 0f;
        }

        public void SetupScene()
        {
            //0=Floor,1=Wall,2=Item,3=goal,4=enemy,5=player
            BoardSetup();
            board[goalPos[0]][goalPos[1]] = 3;
            LayoutObjectAtRandom(1, wallCount.minimum, wallCount.maximum);
            LayoutObjectAtRandom(2, itemCount.minimum, itemCount.maximum);
            LayoutObjectAtRandom(4f, enemyCount.minimum, enemyCount.maximum);
            InitialiseAllies();
        }
    }

}

