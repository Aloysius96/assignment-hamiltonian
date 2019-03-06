using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Hamiltonian
{
    class Solver
    {
        // Use graph object to solve problem
        // Use different algorithms, separated by methods
        public Graph Graph { get; }

        private readonly Random _rand;
        private readonly Stopwatch _stopwatch;

        public Solver(Graph graph)
        {
            Graph = graph;
            _rand = new Random();
            _stopwatch = new Stopwatch();
        }

        public class Result
        {
            public IEnumerable<string> Solution { get; set; }
            public TimeSpan Elapsed { get; set; }
            public string Message { get; set; }
        }

        // Exhaustive search (DFS)
        // If there is at least an array consisting of all the nodes in the graph, a match is found.
        public Result ExSearch()
        {
            _stopwatch.Reset();
            _stopwatch.Start(); // Starts timer

            foreach (var rootNode in Graph.Nodes)
            {
                // Possible combination path of nodes
                var path = new List<Node>();
                
                // unvisitedEdges is a stack that stores number of edges that have not been visited
                // path and unvisitedEdges share the same index
                // Serves as an alternative to avoid the use of recursive functions

                // Eg: path = [Node 0, Node 2, Node 4], unvisitedEdges = [1, 4, 3]
                // Indicates that Node 0 has 1 unvisited edge, Node 2 has 4, Node 4 has 3, ...
                var unvisitedEdges = new List<int>();

                if (rootNode.GetExistingLinkedNodes().Length < 1)
                {
                    _stopwatch.Stop();
                    return new Result
                    {
                        Elapsed = _stopwatch.Elapsed,
                        Message = "Certain nodes has 0 edges, therefore no Hamiltonian path"
                    };
                }

                // Add the root node (as the first node) to the possible combination path
                path.Add(rootNode);
                unvisitedEdges.Add(rootNode.GetExistingLinkedNodes().Length);

                //Console.WriteLine($"Switching to Node {rootNode.Position} as root node...");

                while (unvisitedEdges[0] > 0)
                {
                    var currentNode = path[path.Count-1];
                    var currentIndex = currentNode.GetExistingLinkedNodes().Length - unvisitedEdges[unvisitedEdges.Count - 1];
                    //Console.WriteLine($"DEBUG: [{string.Join(", ", path.Select(node => node.Position))}]");
                    if (unvisitedEdges[unvisitedEdges.Count - 1] > 0)
                    {
                        var nextNode = currentNode.GetExistingLinkedNodes()[currentIndex];
                        // Iterate through the node's child nodes and see if it's already one
                        // of the nodes in the combination list
                        if (path.Contains(nextNode))
                        {
                            unvisitedEdges[unvisitedEdges.Count - 1]--;
                            continue;
                        }

                        unvisitedEdges[unvisitedEdges.Count - 1]--;
                        path.Add(nextNode);
                        unvisitedEdges.Add(nextNode.GetExistingLinkedNodes().Length);
                    }
                    else if (path.Count == Graph.Size)
                    {
                        // Return the list if the quantity of the combination
                        _stopwatch.Stop(); // Stops timer
                        return new Result
                        {
                            Solution = path.Select(node => node.Position.ToString()),
                            Elapsed = _stopwatch.Elapsed
                        };
                    }
                    else
                    {
                        unvisitedEdges.RemoveAt(unvisitedEdges.Count - 1);
                        path.RemoveAt(path.Count-1);
                    }
                }
            }

            _stopwatch.Stop(); // Stops timer
            return new Result
            {
                Elapsed = _stopwatch.Elapsed
            };
        }

        public List<Node> Greedy()
        {
            var greedyList = new List<Node>();

            var rootNode = MaxEdgeNode(Graph.Nodes.ToArray(), null);
            var currentNode = rootNode;
            greedyList.Add(rootNode);

            while (greedyList.Count < Graph.Size)
            {
                // Pick the child node with most number of edges, and repeat for the child node
                // until the MaxEdgeNode function returns null (no more child nodes)
                var nextNode = MaxEdgeNode(currentNode.GetExistingLinkedNodes(), greedyList.ToArray());
                if (nextNode == null) break;

                greedyList.Add(nextNode);
                currentNode = nextNode;
            }

            return greedyList;
        }

        // Get node with max edges and does not exist in the selectedNodes list
        // Returns the node with most edges or null
        public Node MaxEdgeNode(Node[] nodes, Node[] selectedNodes)
        {
            if (nodes == null || !nodes.Any())
                throw new InvalidOperationException("List is empty.");

            Node bigNode = null;
            foreach (var node in nodes)
            {
                // If the node already existed in the list of selected nodes, skip the comparison.
                if (selectedNodes != null && selectedNodes.Contains(node)) continue;

                bigNode = bigNode == null || bigNode.NumOfEdges() < node.NumOfEdges() ? node : bigNode;
            }

            return bigNode;
        }

        public Node RandomNode(Node[] nodes, Node[] selectedNodes)
        {
            if (nodes == null || !nodes.Any())
                throw new InvalidOperationException("List is empty.");

            var nodeClone = new List<Node>(nodes);
            while (nodeClone.Any())
            {
                var randIndex = _rand.Next(0, nodeClone.Count);
                var randNode = nodeClone[randIndex];

                if (!selectedNodes.Contains(randNode))
                    return randNode;
                nodeClone.RemoveAt(randIndex);
            }

            return null;
        }

        public Result GreedyTabu(int iterations)
        {
            return Tabu(iterations, MaxEdgeNode);
        }

        public Result RandomTabu(int iterations)
        {
            return Tabu(iterations, RandomNode);
        }

        public Result Tabu(int iterations, Func<Node[], Node[], Node> searchAlgoFunc)
        {
            // Starts or restart timer
            _stopwatch.Reset();
            _stopwatch.Start();

            // Set an initial candidate solution
            var candidateSolution = Greedy();
            //Console.WriteLine($"Initial candidate:\n{string.Join(", ", candidateSolution.Select(node => node.Position.ToString()))}");

            // Create a Tabu List consist of node sequence in string
            // using NodesToStringSequence method
            var tabuList = new List<string>();
            
            // Randomly pick a node
            // Iterate through the Tabu search until the iteration limit is reached or a satisfiable
            // solution is created
            var counter = 0;
            for (; counter < iterations && candidateSolution.Count < Graph.Size; counter++)
            {
                // Select a random node from the candidate solution
                var randomIndex = _rand.Next(0, candidateSolution.Count);
                var sourceNode = candidateSolution[randomIndex];

                var newPath = new List<Node>();
                // randomIndex + 1 means to include the sourceNode itself in the range
                // We do not want the child node to select its parent node as a possible path
                var selectedNodes = new List<Node>(candidateSolution.GetRange(0, randomIndex + 1));

                var newPathNode = searchAlgoFunc(sourceNode.GetExistingLinkedNodes(), candidateSolution.ToArray());
                newPath.Add(sourceNode);

                // Create a new (neighbor) path beginning from the selected node in the candidate solution
                while (newPathNode != null)
                {
                    newPath.Add(newPathNode);
                    selectedNodes.Add(newPathNode);
                    newPathNode = searchAlgoFunc(newPathNode.GetExistingLinkedNodes(), selectedNodes.ToArray());
                }

                // Check if the new path exists in the tabu list
                // If yes, skip to next iteration
                var isTabu = tabuList.Any(path => path.Contains(NodesToStringSequence(newPath)));
                if (isTabu) continue;

                // Form a neighbor solution from the modified path
                var neighborSolution = candidateSolution.GetRange(0, randomIndex);
                neighborSolution.AddRange(newPath);
                
                // Check if the neighbor solution is better than the candidate
                // If yes, then assign it as the candidate solution, and add the original into tabu list
                // Otherwise add the neighbor into Tabu list instead
                if (neighborSolution.Count > candidateSolution.Count)
                {
                    tabuList.Add(NodesToStringSequence(candidateSolution));
                    candidateSolution = neighborSolution;
                }
                else
                {
                    tabuList.Add(NodesToStringSequence(neighborSolution));
                }
            }

            _stopwatch.Stop();
            var msg = (candidateSolution.Count == Graph.Size
                          ? "Best solution is found"
                          : "No perfect solution found, best approximate alternate solution obtained") +
                      $" after {counter} iterations";
            return new Result
            {
                Solution = candidateSolution.Select(node => node.Position.ToString()),
                Message = msg,
                Elapsed = _stopwatch.Elapsed
            };
        }

        // Convert a sequence of nodes to a string sequence of their positions
        // Eg: [Node 4, Node 15, Node 8, Node 0] becomes 4 15 8 0
        public string NodesToStringSequence(IEnumerable<Node> nodes)
        {
            var nodePositions = nodes.Select(node => node.Position.ToString());
            return string.Join(" ", nodePositions);
        }
    }
}
