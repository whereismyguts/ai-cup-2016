using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk {
    static class AStar {
        public static PointList FindPath(PointList field, Point start, Point goal) {
            // Шаг 1.
            var closedSet = new List<PathNode>();
            var openSet = new List<PathNode>();
            // Шаг 2.
            PathNode startNode = new PathNode() {
                Position = start,
                CameFrom = null,
                PathLengthFromStart = 0,
                HeuristicEstimatePathLength = GetHeuristicPathLength(start, goal)
            };
            openSet.Add(startNode);
            while(openSet.Count > 0) {
                // Шаг 3.
                var currentNode = openSet.OrderBy(node =>
                  node.EstimateFullPathLength).First();
                // Шаг 4.
                if(currentNode.Position.Equals(goal))
                    return GetPathForNode(currentNode);
                // Шаг 5.
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
                // Шаг 6.
                foreach(var neighbourNode in GetNeighbours(currentNode, goal, field)) {
                    // Шаг 7.
                    if(closedSet.Count(node => node.Position.Equals(neighbourNode.Position)) > 0)
                        continue;
                    var openNode = openSet.FirstOrDefault(node =>
                      node.Position.Equals(neighbourNode.Position));
                    // Шаг 8.
                    if(openNode == null)
                        openSet.Add(neighbourNode);
                    else
                        if(openNode.PathLengthFromStart > neighbourNode.PathLengthFromStart) {
                        // Шаг 9.
                        openNode.CameFrom = currentNode;
                        openNode.PathLengthFromStart = neighbourNode.PathLengthFromStart;
                    }
                }
            }
            // Шаг 10.
            return null;
        }
        private static int GetDistanceBetweenNeighbours() {
            return 1;
        }
        private static int GetHeuristicPathLength(Point from, Point to) {
            return (int)(Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y));
        }
        private static List<PathNode> GetNeighbours(PathNode pathNode, Point goal, PointList field) {
            var result = new List<PathNode>();

            // Соседними точками являются соседние по стороне клетки.
            Point[] neighbourPoints = new Point[8];
            neighbourPoints[0] = new Point(pathNode.Position.X + 1, pathNode.Position.Y);
            neighbourPoints[1] = new Point(pathNode.Position.X - 1, pathNode.Position.Y);
            neighbourPoints[2] = new Point(pathNode.Position.X, pathNode.Position.Y + 1);
            neighbourPoints[3] = new Point(pathNode.Position.X, pathNode.Position.Y - 1);

            neighbourPoints[4] = new Point(pathNode.Position.X + 1, pathNode.Position.Y + 1);
            neighbourPoints[5] = new Point(pathNode.Position.X - 1, pathNode.Position.Y - 1);
            neighbourPoints[6] = new Point(pathNode.Position.X - 1, pathNode.Position.Y + 1);
            neighbourPoints[7] = new Point(pathNode.Position.X + 1, pathNode.Position.Y - 1);

            foreach(var point in neighbourPoints) {
                // Проверяем, что не вышли за границы карты.
                if(point.X > field.Xmax || point.X < field.Xmin)
                    continue;
                if(point.Y > field.Ymax || point.Y < field.Ymin)
                    continue;
                // Проверяем, что по клетке можно ходить.
                if(field.Contains(new Point((int)point.X, (int)point.Y)))
                    continue;
                // Заполняем данные для точки маршрута.
                var neighbourNode = new PathNode() {
                    Position = point,
                    CameFrom = pathNode,
                    PathLengthFromStart = pathNode.PathLengthFromStart +
                      GetDistanceBetweenNeighbours(),
                    HeuristicEstimatePathLength = GetHeuristicPathLength(point, goal)
                };
                result.Add(neighbourNode);
            }
            return result;
        }
        private static PointList GetPathForNode(PathNode pathNode) {
            var result = new PointList();
            var currentNode = pathNode;
            while(currentNode != null) {
                result.Add(currentNode.Position);
                currentNode = currentNode.CameFrom;
            }
            result.Reverse();
            return result;
        }
    }
    public class PathNode {
        public Point Position { get; set; }
        // Длина пути от старта (G).
        public int PathLengthFromStart { get; set; }
        // Точка, из которой пришли в эту точку.
        public PathNode CameFrom { get; set; }
        // Примерное расстояние до цели (H).
        public int HeuristicEstimatePathLength { get; set; }
        // Ожидаемое полное расстояние до цели (F).
        public int EstimateFullPathLength {
            get {
                return this.PathLengthFromStart + this.HeuristicEstimatePathLength;
            }
        }
    }
}
