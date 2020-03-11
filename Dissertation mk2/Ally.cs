using System;
using System.Collections.Generic;
using System.Linq;

namespace Dissertation_mk2
{
    public class Ally : Unit
    {
        public int initialHp;
        private List<List<int>> pathToGoal;
        private List<List<List<int>>> enemyPaths = new List<List<List<int>>>();
        private List<List<List<int>>> itemPaths = new List<List<List<int>>>();

        public Ally(Board board, float id, List<int> pos)
        {
            this.board = board;
            this.id = id;
            this.pos = pos;
            initialHp = hp;
        }

        //Take turn unless checking the anxiety presented by a future turn.
        public void TakeTurn(bool anxietyCheck = false)
        {
            Console.WriteLine(id);
            TempCheckMoves();
            if (anxietyCheck) return;
            Move();
        }

        private void TempCheckMoves()
        {
            foreach (var enemy in board.gameManager.enemies)
            {
                if (enemy.hp <= 0) continue;
                var path = FindPath(pos, enemy.pos);
                enemyPaths.Add(path);
                if (path.Count < 6)
                {
                    enemiesInRange.Add(enemy.pos);
                    Console.WriteLine("Path to Enemy " + enemy.id + ":");
                    foreach (var node in path)
                    {
                        Console.WriteLine(node[0] + " " + node[1]);
                    }
                }
            }
            foreach (var item in board.itemPositions)
            {
                var path = FindPath(pos, item);
                itemPaths.Add(path);
                if (path.Count < 6)
                {
                    Console.WriteLine("item in range: " + item[0] + " " + item[1]);
                    itemsInRange.Add(item);
                }
            }

            pathToGoal = FindPath(pos, board.goalPos);
            if (pathToGoal.Count < 6)
            {
                Console.WriteLine("Path to goal:");
                foreach (var node in pathToGoal)
                {
                    Console.WriteLine(node[0] + " " + node[1]);
                }
                goalInRange = true;
            }
        }

        private void Move()
        {
            Console.WriteLine("enemies in range:" + enemiesInRange.Count);
            Console.WriteLine("item in range:" + itemsInRange.Count);
            if (goalInRange && board.markov.Aggressive)
            {
                SwapPosition(board.goalPos);
                board.gameManager.LevelComplete();
            }
            else if (enemiesInRange.Count > 0)
            {
                List<int> move;
                Enemy unit = FindTarget();
                if (unit != null)
                {
                    
                    move = FindMove(unit.pos);
                    if (move != null)
                        SwapPosition(move);
                    Attack(unit);
                }
                else
                {
                    move = BestMove();
                    if (move != null)
                        SwapPosition(move);
                }
            }
            else if (itemsInRange.Count > 0)
            {
                List<int> move = itemsInRange[0];
                itemsInRange.Remove(move);
                board.itemPositions.Remove(move);
                SwapPosition(move);
                board.score += board.itemValue;
            }
            else if (goalInRange)
            {
                SwapPosition(board.goalPos);
                board.gameManager.LevelComplete();
            }
            else
            {
                List<int> move = BestMove();
                if (move != null)
                    SwapPosition(move);
            }
        }

        /*Moves towards enemy with lowest Hp that's within 2 moves away*/
        /*If there are no enemies in range, moves towards the goal*/
        private List<int> BestMove()
        {
            if (board.markov.Aggressive)
            {
                int lowestHp = 5;
                Enemy target = null;

                var enemies = CheckEnemiesInRange(range * 2 + 1);

                foreach (var enemy in enemies.Where(enemy => enemy.hp <= lowestHp))
                {
                    lowestHp = enemy.hp;
                    target = enemy;
                }

                if (target == null && !goalInRange) return pathToGoal[range];
                return FindMove(target?.pos);
            }

            List<int> move = null;
            int pathLength = 160;
            if (board.markov.Explorer)
            {
                foreach (var path in itemPaths.Where(path => path.Count < pathLength))
                {
                    pathLength = path.Count;
                    move = path[range];
                }
            }
            return move ?? pathToGoal[range];
        }

        protected Enemy FindTarget()
        {
            List<Enemy> targets = new List<Enemy>();
            List<float> enemyIds = enemiesInRange.Select(position => board.CheckPosition(position)).ToList();

            targets.AddRange(from id in enemyIds
                from unit in board.gameManager.enemies
                where id == unit.id
                select unit);
            
            return LowestHp(targets);
        }

        private static Enemy LowestHp(IEnumerable<Enemy> targets)
        {
            var lowestHp = 5;
            bool engagedEnemy = false;
            Enemy enemy = null;
            foreach (var unit in targets)
            {
                if (unit.hp == 1)
                {
                    enemy = unit;
                    lowestHp = unit.hp;
                }
                else if (unit.engaged && lowestHp != 1 && !unit.isDead)
                {
                    engagedEnemy = true;
                    enemy = unit;
                }
                else if (!engagedEnemy && unit.hp <= lowestHp && !unit.isDead)
                {
                    lowestHp = unit.hp;
                    enemy = unit;
                }
            }

            return enemy;
        }

        private IEnumerable<Enemy> CheckEnemiesInRange(int shortestDistance)
        {
            List<Enemy> enemies = new List<Enemy>();

            enemies.AddRange(from enemy in board.gameManager.enemies let enemyDist = CheckDistance(pos, enemy.pos) 
                where enemyDist <= shortestDistance select enemy);

            return enemies;
        }

        public void Attack(Enemy unit)
        {
            Console.WriteLine(id + " attacking " + unit.id + ", enemy hp is " + unit.hp);
            if (unit.engaged)
            {
                Console.WriteLine("After attack " + id + " health is " + hp + " and "
                                  + unit.id + " health is " + unit.hp);
                unit.hp -= dmg;
                CheckIfDead(unit);
            }
            else
            {
                unit.hp -= dmg;
                if (unit.hp > 0)
                {
                    hp -= unit.dmg;
                    Console.WriteLine("After attack " + id + " health is " + hp + " and "
                                      + unit.id + " health is " + unit.hp);
                    unit.engaged = true;
                    unit.CheckIfDead(this);
                }
                else
                    CheckIfDead(unit);
            }
        }

        public void CheckIfDead(Enemy enemy)
        {
            if (enemy == null || enemy.hp > 0) return;
            Console.WriteLine("unit dead");
            board.board[enemy.pos[0]][enemy.pos[1]] = 0;
            enemy.isDead = true;
        }

        public void EndTurn()
        {
            if (isDead)
            {
                board.gameManager.RemoveAllyFromList(this);
            }
            else
            {
                moves.Clear();
                itemsInRange.Clear();
                enemiesInRange.Clear();
                alliesInRange.Clear();
                pathToGoal?.Clear();
                enemyPaths.Clear();
                itemPaths.Clear();
                goalInRange = false;
            }
        }

        public void UpdateHp()
        {
            initialHp = hp;
        }
    }
}