using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dissertation_mk2
{
    public class GameManager
    {
        public Board board;
        public bool gameOver = false;
        public double P;

        public List<Enemy> enemies = new List<Enemy>();
        public List<Ally> allies = new List<Ally>();

        //For modelling flow
        public int anxiety;
        public int cDecay;

        public GameManager()
        {
            board = new Board(this);
            InitGame();
            PlayGame();
        }

        private void PlayGame()
        {
            while (!gameOver)
            {
                foreach (var ally in allies.Where(ally => !ally.isDead))
                {
                    if (gameOver) continue;
                    ally.engaged = false;
                    ally.TakeTurn();
                }

                Ally[] tempAlly = allies.ToArray();
                foreach (var ally in tempAlly)
                {
                    ally.EndTurn();
                }
                foreach (var enemy in enemies.Where(enemy => !enemy.isDead))
                {
                    if (gameOver) continue;
                    enemy.engaged = false;
                    enemy.TakeTurn();
                }

                Enemy[] tempEnemy = enemies.ToArray();
                foreach (var enemy in tempEnemy)
                {
                    enemy.EndTurn();
                }
                Console.WriteLine(Builder(board.board));
                //UpdateAnxiety();
                board.markov.Transition();
            }
        }

        private void InitGame()
        {
            enemies.Clear();
            allies.Clear();
            board.SetupScene();
            Random rand = new Random();
            P = rand.NextDouble();
            Console.WriteLine(Builder(board.board));
        }

        public void AddEnemyToList(Enemy script)
        {
            enemies.Add(script);
        }

        public void AddAllyToList(Ally script)
        {
            allies.Add(script);
        }

        public void RemoveEnemyFromList(Enemy script)
        {
            enemies.Remove(script);
        }

        public void RemoveAllyFromList(Ally script)
        {
            allies.Remove(script);
        }

        public bool CheckIfGameOver()
        {
            if (allies == null)
            {
                return true;
            }

            return false;
        }

        public void GameOver()
        {
            Console.WriteLine("Game Over");
            gameOver = true;
            LevelComplete(true);
        }

        public void LevelComplete(bool isGameOver = false)
        {
            if (isGameOver) gameOver = true;
            Console.WriteLine("Level Complete");
            if (gameOver)
            {
                //If gameOver store data a bit different

                //EvaluateFun(board);
                //StoreData();
                //InitGame();
            }
            else
            {
                gameOver = true;
                //EvaluateFun(board);
                //StoreData();
                //InitGame();
            }
        }

        private static string Builder(IEnumerable<List<float>> board)
        {
            StringBuilder builder = new StringBuilder();
            foreach (List<float> row in board)
            {
                foreach (float tile in row)
                {
                    builder.Append(tile + " ");
                }
                builder.AppendLine();
            }
            return builder.ToString();
        }






        //Gives a "fun" rating of the level of fun the player had with the level
        //over time modeled on the flow concept 
        private void EvaluateFun(Board board)
        {
            Console.WriteLine("Not Implemented");
        }

        //Store data for genetic algorithm
        private void StoreData()
        {
           Console.WriteLine("Not Implemented");
        }

        //Called each term to update the anxiety value;
        private void UpdateAnxiety()
        {
            foreach (Ally ally in allies)
            {
                CalculateAnxiety(ally);
            }
        }

        private void CalculateAnxiety(Ally ally)
        {
            //Increases anxiety based on how many enemies are in range to attack next turn
            ally.TakeTurn(true);
            if (ally.enemiesInRange != null)
            {
                for (int i = 1; i < ally.enemiesInRange.Count; i++)
                {
                    anxiety += i;
                }
            }

            //Increases anxiety relative to Hp lost this turn
            int loss = ally.initialHp - ally.hp;
            if (loss > 2)
            {
                anxiety += loss * 2;
            }
            else
            {
                anxiety += loss;
            }
            ally.UpdateHp();
        }
    }
}