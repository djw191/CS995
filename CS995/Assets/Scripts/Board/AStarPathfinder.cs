using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Board
{
    public class AStarPathfinder
    {
        private readonly PathNode[,] _grid;
        private List<PathNode> _openList;
        private List<PathNode> _closedList;
        private readonly CellData[,] _cells;
        private readonly int _attack;

        // TODO handle negative weights in food, pass in the lowest known weight and compensate by automatically adding/subtracting
        // DON'T Do this for bots, since, they don't need to eat and only seek the player
        public AStarPathfinder(CellData[,] grid, int attack)
        {
            _grid = new PathNode[grid.GetLength(0), grid.GetLength(1)];
            _cells = grid;
            _attack = attack;
        }

        public int GetTotalCost(Vector2Int start, Vector2Int end, int lowestWeight)
        {
            var path = FindPath(start, end, lowestWeight);

            var totalCost = path.Count - 1;
            foreach (var pathNode in path)
            {
                totalCost += pathNode.TraversalCost / _attack;
#if UNITY_EDITOR
                var bm = GameManager.Instance.BoardManager;
                bm.OutlineTilemap.SetTile(new Vector3Int(pathNode.X, pathNode.Y, 0), bm.DebugTile);
#endif
            }

            return totalCost;
        }

        public Vector2Int GetNextCell(Vector2Int start, Vector2Int end)
        {
            var path = FindPath(start, end);
            if (path == null || path.Count < 2)
                return start;
            return new Vector2Int(path[1].X, path[1].Y);
        }

        private List<PathNode> FindPath(Vector2Int start, Vector2Int end)
        {
            for (var i = 0; i < _grid.GetLength(0); i++)
            for (var j = 0; j < _grid.GetLength(1); j++)
            {
                var passable = _cells[i, j].Passable;
                if (GameManager.DoesObjectHaveAttribute<NotPathable>(_cells[i, j].ContainedObject)) passable = false;
                var pathNode = _grid[i, j] = new PathNode(i, j,
                    _cells[i, j].ContainedObject is WallObject w ? w.HitPoints : 0, passable);
                pathNode.GCost = int.MaxValue;
                pathNode.Parent = null;
            }

            var startNode = _grid[start.x, start.y];
            var endNode = _grid[end.x, end.y];
            _openList = new List<PathNode> { startNode };
            _closedList = new List<PathNode>();

            startNode.GCost = 0;
            startNode.HCost = ManhattanDistance(startNode, endNode);

            while (_openList.Count > 0)
            {
                var currentNode = GetLowestFCostPathNode(_openList);
                if (currentNode == endNode) return CalculatePath(endNode);
                _openList.Remove(currentNode);
                _closedList.Add(currentNode);

                foreach (var neighbor in GetNeighborList(currentNode))
                {
                    if (_closedList.Contains(neighbor)) continue;
                    if (!neighbor.Passable)
                    {
                        _closedList.Add(neighbor);
                        continue;
                    }

                    var possibleGCost = currentNode.GCost + 1 + neighbor.TraversalCost / _attack;

                    if (possibleGCost >= neighbor.GCost) continue;

                    neighbor.Parent = currentNode;
                    neighbor.GCost = possibleGCost;
                    neighbor.HCost = ManhattanDistance(neighbor, endNode);
                    if (!_openList.Contains(neighbor)) _openList.Add(neighbor);
                }
            }

            return null;
        }

        private List<PathNode> FindPath(Vector2Int start, Vector2Int end, int lowestWeight)
        {
            //Use the lowest weight to find the shortest path
            for (var i = 0; i < _grid.GetLength(0); i++)
            for (var j = 0; j < _grid.GetLength(1); j++)
            {
                var edgeWeight = -lowestWeight;
                if (_cells[i, j].ContainedObject is WallObject w)
                    edgeWeight += w.HitPoints;
                else if (_cells[i, j].ContainedObject is FoodObject f) edgeWeight -= f.FoodAmount;
                var pathNode = _grid[i, j] = new PathNode(i, j, edgeWeight, _cells[i, j].Passable);
                pathNode.GCost = int.MaxValue;
                pathNode.Parent = null;
            }

            var startNode = _grid[start.x, start.y];
            var endNode = _grid[end.x, end.y];
            _openList = new List<PathNode> { startNode };
            _closedList = new List<PathNode>();

            startNode.GCost = 0;
            startNode.HCost = ManhattanDistance(startNode, endNode);

            while (_openList.Count > 0)
            {
                var currentNode = GetLowestFCostPathNode(_openList);
                if (currentNode == endNode) return CalculatePath(endNode);
                _openList.Remove(currentNode);
                _closedList.Add(currentNode);

                foreach (var neighbor in GetNeighborList(currentNode))
                {
                    if (_closedList.Contains(neighbor)) continue;
                    if (!neighbor.Passable)
                    {
                        _closedList.Add(neighbor);
                        continue;
                    }

                    var possibleGCost = currentNode.GCost + 1 + neighbor.TraversalCost / _attack;

                    if (possibleGCost >= neighbor.GCost) continue;

                    neighbor.Parent = currentNode;
                    neighbor.GCost = possibleGCost;
                    neighbor.HCost = ManhattanDistance(neighbor, endNode);
                    if (!_openList.Contains(neighbor)) _openList.Add(neighbor);
                }
            }

            return null;
        }

        private List<PathNode> GetNeighborList(PathNode node)
        {
            var neighbors = new List<PathNode>();
            if (node.X - 1 >= 0) neighbors.Add(_grid[node.X - 1, node.Y]);

            if (node.X + 1 < _grid.GetLength(0)) neighbors.Add(_grid[node.X + 1, node.Y]);

            if (node.Y - 1 >= 0) neighbors.Add(_grid[node.X, node.Y - 1]);

            if (node.Y + 1 < _grid.GetLength(1)) neighbors.Add(_grid[node.X, node.Y + 1]);
            return neighbors;
        }

        private List<PathNode> CalculatePath(PathNode endNode)
        {
            var path = new List<PathNode> { endNode };
            var currentNode = endNode;
            while (currentNode.Parent != null)
            {
                path.Add(currentNode.Parent);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }

        private int ManhattanDistance(PathNode a, PathNode b)
        {
            return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
        }

        private PathNode GetLowestFCostPathNode(List<PathNode> pathNodeList)
        {
            var lowestCostNode = pathNodeList[0];
            foreach (var pathNode in pathNodeList.Where(pathNode => pathNode.FCost < lowestCostNode.FCost))
                lowestCostNode = pathNode;
            return lowestCostNode;
        }
    }

    public class PathNode
    {
        public readonly int X;
        public readonly int Y;
        public readonly bool Passable;
        public int TraversalCost { get; private set; }

        private int _gCost;

        public int GCost
        {
            get => _gCost;
            set
            {
                _gCost = value;
                FCost = value + HCost;
            }
        }

        private int _hCost;

        public int HCost
        {
            get => _hCost;
            set
            {
                _hCost = value;
                FCost = value + GCost;
            }
        }

        public int FCost;

        public PathNode Parent;

        public PathNode(int x, int y, int cost, bool passable)
        {
            X = x;
            Y = y;
            TraversalCost = cost;
            Passable = passable;
        }
    }
}