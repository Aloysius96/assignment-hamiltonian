using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hamiltonian
{
    class Program
    {
        static void Main(string[] args)
        {
            // Number of times to iterate the search algorithm to get optimal execution time
            const int execLoop = 20;
            const int tabuLoop = 1000;

            const int numOfNodes = 20;
            const int edgesPerNode = 5;

            Console.WriteLine("Generating graph...");
            // Graph(Number of nodes, max number of edges per node)
            var graph = new Graph(numOfNodes, edgesPerNode);
            Console.WriteLine(graph);

            var solver = new Solver(graph);

            do
            {
                var elapsedList = new List<TimeSpan>();

                Solver.Result result;
                Console.WriteLine("Choose one of the algorithm below:\n1 - Exhaustive Search\n" +
                                  "2 - Tabu Greedy Search\n3 - Tabu Random Search\nOther character - Exit");
                Console.Write("Enter a number: ");
                var option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        for (var i = 0; i < execLoop; i++)
                        {
                            Console.WriteLine($"Starting exhaustive search #{i+1}...");
                            result = solver.ExSearch();
                            elapsedList.Add(result.Elapsed);

                            var message = CompileResult(result);
                            Console.WriteLine($"Search completed. Results return:\n{message}");
                            if (i < execLoop - 1) Thread.Sleep(1000);
                        }
                        break;
                    case "2":
                        for (var i = 0; i < execLoop; i++)
                        {
                            Console.WriteLine($"Running Tabu-Greedy Search #{i+1}...");
                            result = solver.GreedyTabu(tabuLoop);
                            elapsedList.Add(result.Elapsed);

                            var message = CompileResult(result);
                            Console.WriteLine($"Search completed. Results return:\n{message}");
                            if (i < execLoop - 1) Thread.Sleep(1000);
                        }
                        break;
                    case "3":
                        for (var i = 0; i < execLoop; i++)
                        {
                            Console.WriteLine($"Running Tabu-Random Search #{i + 1}...");
                            result = solver.RandomTabu(tabuLoop);
                            elapsedList.Add(result.Elapsed);

                            var message = CompileResult(result);
                            Console.WriteLine($"Search completed. Results return:\n{message}");
                            if (i < execLoop - 1) Thread.Sleep(1000);
                        }
                        break;
                    default:
                        return;
                }

                Console.WriteLine(
                    "Search iteration ended\n" +
                    $"Average elapsed time: {elapsedList.Average(ts => ts.TotalMilliseconds)} ms\n" +
                    $"Minimum elapsed time: {elapsedList.Min(ts => ts.TotalMilliseconds)} ms\n" +
                    $"Maximum elapsed time: {elapsedList.Max(ts => ts.TotalMilliseconds)} ms");

            } while (true);

        }

        static string CompileResult(Solver.Result result)
        {
            var message = new StringBuilder();
            if (result.Solution != null)
            {
                message.AppendLine("A solution is found in this graph:");
                message.AppendLine(string.Join(", ", result.Solution));
                message.AppendLine($"Path length: {result.Solution.Count()}");
            }
            else
            {
                message.AppendLine("No solutions found in this graph");
            }

            message.AppendLine(result.Message ?? "");
            message.AppendLine($"Elapsed time: {result.Elapsed.Milliseconds} ms");
            return message.ToString();
        }
    }
}
