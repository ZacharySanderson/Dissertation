using System;
using System.Collections.Generic;
using System.Linq;

namespace Dissertation_mk2
{
    public abstract class Unit
    {
        public int hp = 5;
        public int dmg = 1;
        public int range = 5;
        public bool engaged;

        public Board board;
        public float id;
        public List<int> pos;
        readonly int[][] directions = new int[4][] { new int[2] { 1, 0 }, new int[2] { 0, -1 }, new int[2] { 0, 1 }, new int[2] { -1, 0 } };

        public List<List<int>> moves = new List<List<int>>();
        public List<List<int>> enemyAttackPositions = new List<List<int>>();
        public List<List<int>> allyAttackPositions = new List<List<int>>();
        public List<List<int>> itemsInRange = new List<List<int>>();
        public List<List<int>> enemiesInRange = new List<List<int>>();
        public List<List<int>> alliesInRange = new List<List<int>>();
        public bool goalInRange;
        public bool isDead = false;

        private static bool IsInList(IEnumerable<int> list1, IEnumerable<List<int>> listOfLists)
        {
            return listOfLists.Any(list1.SequenceEqual);
        }

        /*
        protected List<int> BestAttack(List<List<int>> attackPositions, List<int> enemyPos, int unitType)
        {
            List<List<int>> validPositions = (from position in attackPositions from direction in directions 
                select position.Select((t, i) => t + direction[i]).ToList() into move 
                where move.SequenceEqual(enemyPos) select move).ToList();

            int lowestNum = 5;
            List<int> bestPosition = null;
            List<float> enemyIds = new List<float>();
            foreach (List<int> position in validPositions)
            {
                CheckEnemiesInRange(position, range + 1, unitType, enemyIds);
                if (enemyIds.Count < lowestNum)
                {
                    lowestNum = enemyIds.Count;
                    bestPosition = position;
                }
            }

            return bestPosition;
        }
        */

        protected List<int> FindMove(List<int> targetPos)
        {
            List<int> move = null;
            Console.WriteLine(targetPos[0] + " " + targetPos[1]);
            List<List<int>> path = FindPath(pos, targetPos);
            if (path.Count > range)
                move = path[range];
            else if (path.Count >= 2)
                move = path[^1];

            return move;
        }

        protected List<List<int>> FindPath(List<int> startPos, List<int> targetPos)
        {
            targetPos = FindReachable(targetPos);
            Node currentNode = new Node(startPos, 0, CheckDistance(startPos, targetPos), null, true);
            List<Node> closedList = new List<Node>();
            List<Node> openList = new List<Node> { currentNode };
            int iterations = 1;
            List<int> currentPos;
            bool targetFound = false;

            do
            {
                currentPos = currentNode.Pos;
                openList.Remove(currentNode);
                    closedList.Add(currentNode);

                    foreach (int[] direction in directions)
                {
                    List<int> move = currentPos.Select((t, i) => t + direction[i]).ToList();

                    if (move.SequenceEqual(targetPos))
                    {
                        targetFound = true;
                        continue;
                    }

                    if (board.rows - 1 < move[0] || move[0] < 0 || board.columns - 1 < move[1] || move[1] < 0) continue;

                    Node node = new Node(move, currentNode.G + 1, CheckDistance(move, targetPos), currentNode);

                    if (node.IsInClosedList(closedList)) continue;

                    if (node.IsInOpenList(openList)) continue;

                    float tile = board.CheckPosition(move);
                    int roundedTile = (int) Math.Floor(tile);

                    if (roundedTile == 0 || roundedTile == 2)
                        openList.Add(node);
                }

                if (!targetFound && openList.Count > 0)
                {
                    currentNode = ChooseNextNode(openList, targetPos);
                }
            } while (openList.Count > 0 && !targetFound && currentNode != null && iterations < 55);

            if (iterations >= 55)
            {
                var f = 100;
                foreach (var node in closedList.Where(node => node.F < f))
                {
                    currentNode = node;
                    currentPos = node.Pos;
                }
            }
            List<List<int>> path = new List<List<int>> { currentPos };
            while (currentNode?.Parent != null)
            {
                path.Add(currentNode.Parent.Pos);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }

        private static Node ChooseNextNode(IReadOnlyCollection<Node> openList, IReadOnlyCollection<int> targetPos)
        {
            foreach (var node in openList.Where(node => node.Pos.SequenceEqual(targetPos)))
            {
                return node;
            }
            Node nextNode = null;
            int f = 160;
            foreach (var node in openList)
            {
                if (node.F <= f)
                {
                    nextNode = node;
                    f = node.F;
                }
                else if (node.F > 160)
                    Console.WriteLine(node.F + " " + node.Pos[0] + " " + node.Pos[1]);
            }

            if (nextNode == null)
                Console.WriteLine("open list not empty, f is " + f);
            return nextNode;
        }

        protected static int CheckDistance(List<int> startPos, List<int> endPos)
        {
            int xDiff = Math.Abs(endPos[0] - startPos[0]);
            int yDiff = Math.Abs(endPos[1] - startPos[1]);
            if (xDiff + yDiff > 100)
                Console.WriteLine("xDiff:" + xDiff + " yDiff:" + yDiff);
            return xDiff + yDiff;
        }

        protected void SwapPosition(List<int> move)
        {
            if (CheckDistance(pos, move) < range + 1)
            {
                Console.WriteLine("Moving from: " + pos[0] + " " + pos[1] + " to:" + move[0] + " " + move[1]);
                board.board[pos[0]][pos[1]] = 0;
                board.board[move[0]][move[1]] = id;
                pos[0] = move[0];
                pos[1] = move[1];
            }
        }

        /*If intended target for A* is unreachable uses another
         reachable node close to the target instead*/
        private List<int> FindReachable(List<int> targetPos)
        {
            bool reachable = false;
            List<List<int>> adjacentMoves  = new List<List<int>>();
            List<int> move = null;
            List<int> currentMove;
            foreach (int[] direction in directions)
            {
                currentMove = targetPos.Select((t, i) => t + direction[i]).ToList();
                if (board.rows - 1 < currentMove[0] || currentMove[0] < 0 || 
                    board.columns - 1 < currentMove[1] || currentMove[1] < 0) continue;
                adjacentMoves.Add(currentMove);
                if ((int) Math.Floor(board.CheckPosition(currentMove)) == 0)
                    reachable = true;
            }

            if (reachable == false)
            {
                int shortestDistance = 100;
                foreach (var adjacentMove in adjacentMoves)
                {
                    foreach (var direction in directions)
                    {
                        currentMove = adjacentMove.Select((t, i) => t + direction[i]).ToList();
                        if (board.rows - 1 < currentMove[0] || currentMove[0] < 0 ||
                            board.columns - 1 < currentMove[1] || currentMove[1] < 0) continue;
                        var currentDistance = CheckDistance(pos, currentMove);

                        if ((int) Math.Floor(board.CheckPosition(currentMove)) == 0 &&
                            currentDistance < shortestDistance)
                        {
                            shortestDistance = currentDistance;
                            move = currentMove;
                        }
                    }
                }
            }

            return move ?? targetPos;
        }

        //Checks what possible moves a unit can make this turn.
        //Uses range+1 to cycle through as units can attack the space next to them,
        //however only enemy units are added when at range+1.
        protected void CheckMoves(List<int> currentPos, int moveRange)
        {
            if (moveRange <= 0) return;
            foreach (int[] direction in directions)
            {
                if (currentPos.Count != direction.Length) continue;

                List<int> move = currentPos.Select((t, i) => t + direction[i]).ToList();

                if (!(board.rows - 1 < move[0] || move[0] < 0 || board.columns - 1 < move[1] || move[1] < 0))
                {
                    float tile = board.CheckPosition(move);
                    int sc = (int)Math.Floor(tile);
                    switch (sc)
                    {
                        case 0:
                            if (moveRange > 1 && !IsInList(move, moves))
                            {
                                moves.Add(move);
                                CheckMoves(move, moveRange - 1);
                            }
                            break;
                        case 2:
                            if (moveRange > 1 && !IsInList(move, itemsInRange))
                            {
                                itemsInRange.Add(move);
                                CheckMoves(move, moveRange - 1);
                            }
                            break;
                        case 3:
                            if (moveRange > 1)
                                goalInRange = true;
                            break;
                        case 4:
                            if (moveRange > 1 && !IsInList(move, enemyAttackPositions))
                                enemyAttackPositions.Add(currentPos);
                            if (!IsInList(move, enemiesInRange))
                                enemiesInRange.Add(move);
                            break;
                        case 5:
                            if (moveRange > 1 && !IsInList(move, allyAttackPositions))
                                allyAttackPositions.Add(currentPos);
                            if (!IsInList(move, alliesInRange))
                                alliesInRange.Add(move);
                            break;
                    }
                }
            }
        }
    }
}