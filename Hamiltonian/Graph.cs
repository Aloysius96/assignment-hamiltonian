using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hamiltonian
{
    public class Graph
    {
        private Random rand;

        public int Size => Nodes.Count;
        
        public int MaxEdges { get; }

        public List<Node> Nodes { get; set; }

        public Graph(int size) : this(size, size / 2) { }

        public Graph(int size, int maxEdges)
        {
            if (maxEdges >= size)
                throw new InvalidOperationException("Number of edges must be less than the number of nodes!");

            MaxEdges = maxEdges;
            rand = new Random();
            Nodes = new List<Node>();
            Generate(size);
            
        }

        public Graph(int size, int[][] nodePaths)
        {
            rand = new Random();
            Nodes = new List<Node>();
            Generate(size, nodePaths);
        }

        private void Generate(int size)
        {
            // Create Nodes
            for (var i = 0; i < size; i++)
            {
                var node = new Node(size, i);
                Nodes.Add(node);
                node.MaxPaths = rand.Next(1, MaxEdges + 1);
            }

            // Link Nodes to each other randomly
            // Use for loop and randomly select a few nodes to be linked
            foreach (var node in Nodes)
            {
                // Generate a number of paths connecting to nodes (1 <= number of paths <= Size - 1)
                // Iterate the call for LinkNode method for each selected to-be-linked nodes
                var pathIndexes = GetLinkedNodesIndex(size, node);
                foreach (var nodeIndex in pathIndexes)
                {
                    node.LinkNode(Nodes[nodeIndex]);
                }
            }
        }
        
        // Generate a graph with manual node connection paths
        private void Generate(int size, int[][] nodePaths)
        {
            // Create Nodes
            for (var i = 0; i < size; i++)
            {
                var node = new Node(size, i);
                Nodes.Add(node);
            }
            
            for (int i = 0; i < nodePaths.Length; i++)
            {
                // Generate a number of paths connecting to nodes (1 <= number of paths <= Size - 1)
                // Iterate the call for LinkNode method for each selected to-be-linked nodes
                
                foreach (var nodeIndex in nodePaths[i])
                {
                    Nodes[i].LinkNode(Nodes[nodeIndex]);
                }
            }
        }

        private int[] GetLinkedNodesIndex(int size, Node currentNode)
        {
            // Declare the amount of paths
            // The maximum amount of paths is reduced to prevent most nodes connecting to each other
            var paths = currentNode.MaxPaths;
            //var paths = 2; // test

            // Get an array of node position indexes
            // Example: Node connecting to node 0, 2 and 3 will have array of [0, 2, 3]

            // Create a list of possible nodes to be connected
            var nodeIndexes = new List<int>();

            // Deduct the total paths to reduce excessive paths (more paths than intended quantity)
            // Open paths = Total paths - existing paths
            var openPaths = paths - currentNode.GetPathCount();
            openPaths = openPaths < 0 ? 0 : openPaths;

            for (var i = 0; i < size; i++)
            {
                if (i != currentNode.Position) nodeIndexes.Add(i);
            }

            for (var i = size - 1; i > openPaths; i--)
            {
                // Generate random index for node position
                // Remove random nodes until it has the same number as the number of paths entered
                var index = rand.Next(nodeIndexes.Count);
                nodeIndexes.RemoveAt(index);

                // If generated position is current position or already in index then repeat
                //if (currentPosition == index)
                // Assign node index to array

            }

            return nodeIndexes.ToArray();
        }



        public override string ToString()
        {
            var edges = Nodes.Sum(node => node.GetPathCount()) / 2;
            var sb = new StringBuilder();
            sb.AppendLine("Generated graph with the following nodes:");

            foreach (var node in Nodes)
            {
                var onlyLinkedNodes = node.LinkedNodes.Where(n => n != null);

                sb.Append($"Node {node.Position}, connected to:\t");
                sb.Append(string.Join(", ", onlyLinkedNodes.Select(n => n.Position)));
                sb.AppendLine();
            }

            sb.AppendLine($"Graph has a total of {Size} nodes and {edges} edges.");

            return sb.ToString();
        }
    }

    public class Node
    {
        public int MaxPaths { get; set; }

        public int Position { get; set; }
        public Node[] LinkedNodes { get; set; } // [null, [Node], [Node], null, null]
        
        //public bool[] LinkedNodesBools { get; set; } // [false, true, true, false, false]
        
        public Node(int graphSize, int index)
        {
            // Determine the max number of links the node can have in the associated graph
            LinkedNodes = new Node[graphSize];
            Position = index;
        }

        public Node[] GetExistingLinkedNodes()
        {
            return LinkedNodes.Where(node => node != null).ToArray();
        }

        public int NumOfEdges()
        {
            return GetExistingLinkedNodes().Length;
        }

        public void LinkNode(Node targetNode)
        {
            if (LinkedNodes[targetNode.Position] == targetNode) return;
            // Link this node to target node
            // If node is already linked to target node then return
            // Then link the target node to this node

            LinkedNodes[targetNode.Position] = targetNode;
            targetNode.LinkNode(this);
        }

        public int GetPathCount()
        {
            // Return the number of connected nodes
            return LinkedNodes.Count(n => n != null);
        }
    }
}
