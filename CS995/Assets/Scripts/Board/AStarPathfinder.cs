using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Board
{
    public class AStarPathfinder
    {
        private PathNode[,] _grid;
        private List<PathNode> _openList;
        private List<PathNode> _closedList;
        private readonly CellData[,] _cells;

        public AStarPathfinder(CellData[,] grid)
        {
            _grid = new PathNode[grid.GetLength(0), grid.GetLength(1)];
            _cells = grid;
        }

        public int GetTotalCost(Vector2Int start, Vector2Int end)
        {
            var path = FindPath(start, end);
            int totalCost = path.Count - 1;
            foreach (var pathNode in path)
            {
                totalCost += pathNode.TraversalCost;
            }
            return totalCost;
        }
        public Vector2Int GetNextCell(Vector2Int start, Vector2Int end)
        {
            var path = FindPath(start, end);
            return new Vector2Int( path[1].X, path[1].Y);
        }
        public List<PathNode> FindPath(Vector2Int start, Vector2Int end)
        {
            for (int i = 0; i < _grid.GetLength(0); i++)
            {
                for (int j = 0; j < _grid.GetLength(1); j++)
                {
                    PathNode pathNode = _grid[i, j] = new PathNode(_grid,i, j, _cells[i, j].ContainedObject is WallObject w ? w.HitPoints : 0, _cells[i, j].Passable);
                    pathNode.GCost = int.MaxValue;
                    pathNode.Parent = null;
                }
            }
            
            PathNode startNode = _grid[start.x, start.y];
            PathNode endNode = _grid[end.x, end.y];
            _openList = new List<PathNode> { startNode };
            _closedList = new List<PathNode>();

            startNode.GCost = 0;
            // startNode.HCost = ManhattanDistance(startNode, endNode);
            startNode.HCost = 0;

            while (_openList.Count > 0)
            {
                PathNode currentNode = GetLowestFCostPathNode(_openList);
                if (currentNode == endNode)
                {
                    return CalculatePath(endNode);
                }
                _openList.Remove(currentNode);
                _closedList.Add(currentNode);

                foreach (var neighbor in GetNeighborList(currentNode))
                {
                    if(_closedList.Contains(neighbor)) continue;
                    if (!neighbor.Passable)
                    {
                        _closedList.Add(neighbor);
                        continue;
                    }

                    if (neighbor.TraversalCost > 0)
                    {
                        ;
                    }
                    int possibleGCost = currentNode.GCost + 1 + neighbor.TraversalCost;
                    
                    if (possibleGCost >= neighbor.GCost) continue;
                    
                    neighbor.Parent = currentNode;
                    neighbor.GCost = possibleGCost;
                    // neighbor.HCost = ManhattanDistance(neighbor, endNode);
                    neighbor.HCost = 0;
                    if(!_openList.Contains(neighbor)) _openList.Add(neighbor);
                }
            }

            return null;
        }

        private List<PathNode> GetNeighborList(PathNode node)
        {
            List<PathNode> neighbors = new List<PathNode>();
            if (node.X - 1 >= 0)
            {
                neighbors.Add(_grid[node.X - 1, node.Y]);
            }

            if (node.X + 1 < _grid.GetLength(0))
            {
                neighbors.Add(_grid[node.X + 1, node.Y]);
            }

            if (node.Y - 1 >= 0)
            {
                neighbors.Add(_grid[node.X, node.Y - 1]);
            }

            if (node.Y + 1 < _grid.GetLength(1))
            {
                neighbors.Add(_grid[node.X, node.Y + 1]);
            }
            return neighbors;
        }
        private List<PathNode> CalculatePath(PathNode endNode)
        {
            List<PathNode> path = new List<PathNode>();
            path.Add(endNode);
            PathNode currentNode = endNode;
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
            PathNode lowestCostNode = pathNodeList[0];
            foreach (var pathNode in pathNodeList.Where(pathNode => pathNode.FCost < lowestCostNode.FCost))
            {
                lowestCostNode = pathNode;
            }
            return lowestCostNode;
        }
    }

    public class PathNode
    {
        private PathNode[,] _grid;
        public int X;
        public int Y;
        public bool Passable = true;
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
                FCost = value + HCost;
            }
        }
        public int FCost;

        public PathNode Parent;

        public PathNode(PathNode[,] grid, int x, int y, int cost, bool passable)
        {
            _grid = grid;
            X = x;
            Y = y;
            TraversalCost = cost;
            Passable = passable;
        }
    }
}