using System;
using System.Collections.Generic;
using System.Linq;

namespace Dissertation_mk2
{
    public class Enemy : Unit
    {
        public Enemy(Board board, float id, List<int> pos)
        {
            this.board = board;
            this.id = id;
            this.pos = pos;
        }

        public void TakeTurn()
        {
            CheckMoves(pos, range + 1);
            Console.WriteLine(id + " " + moves.Count);
            Console.WriteLine(alliesInRange.Count);
            Move();
        }

        private void Move()
        {
            List<int> move = null;
            if (alliesInRange != null)
            {
                Ally unit = FindTarget();
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
            else
            {
                move = BestMove();
                if (move != null)
                    SwapPosition(move);
            }
        }

        private List<int> BestMove()
        {
            int lowestHp = 5;
            Ally target = null;
            IEnumerable<Ally> enemies = CheckEnemiesInRange(range * 2 + 1);

            foreach (var enemy in enemies.Where(enemy => enemy.hp <= lowestHp))
            {
                lowestHp = enemy.hp;
                target = enemy;
            }

            if (target != null)
            {
                return FindMove(target.pos);
            }

            return null;
        }

        protected Ally FindTarget()
        {
            List<Ally> targets = new List<Ally>();

            List<float> enemyIds = alliesInRange.Select(position => board.CheckPosition(position)).ToList();

            targets.AddRange(from id in enemyIds
                from unit in board.gameManager.allies
                where id == unit.id
                select unit);

            return LowestHp(targets);
        }

        private static Ally LowestHp(IEnumerable<Ally> targets)
        {
            var lowestHp = 5;
            bool engagedEnemy = false;
            Ally enemy = null;
            foreach (var unit in targets)
            {
                if (unit.hp == 1)
                {
                    enemy = unit;
                    lowestHp = unit.hp;
                }
                else if (unit.engaged && lowestHp != 1)
                {
                    engagedEnemy = true;
                    enemy = unit;
                }
                else if (!engagedEnemy && unit.hp <= lowestHp)
                {
                    lowestHp = unit.hp;
                    enemy = unit;
                }
            }

            return enemy;
        }

        public void Attack(Ally unit)
        {
            Console.WriteLine(id + " attacking " + unit.id + ", units hp is " + unit.hp);
            if (unit.engaged)
            {
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

        private IEnumerable<Ally> CheckEnemiesInRange(int shortestDistance)
        {
            List<Ally> enemies = new List<Ally>();

            enemies.AddRange(from enemy in board.gameManager.allies
                let enemyDist = CheckDistance(pos, enemy.pos)
                where enemyDist <= shortestDistance
                select enemy);

            return enemies;
        }

        public void CheckIfDead(Ally ally)
        {
            if (ally == null || ally.hp > 0) return;
            board.board[ally.pos[0]][ally.pos[1]] = 0;
            ally.isDead = true;
            if (board.gameManager.CheckIfGameOver())
            {
                board.gameManager.GameOver();
            }
        }
        public void EndTurn()
        {
            if (isDead)
            {
                board.gameManager.RemoveEnemyFromList(this);
            }
            else
            {
                moves.Clear();
                itemsInRange.Clear();
                enemiesInRange.Clear();
                alliesInRange.Clear();
                goalInRange = false;
            }
        }
    }
}
