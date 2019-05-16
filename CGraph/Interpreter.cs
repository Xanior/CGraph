using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGraph
{
    class Interpreter
    {
        //TODO: asdasf
        private static String objDumpPath = "C:\\MinGW\\bin\\";
        public static string InterpretEXE(String filePath, String fileName)
        {
            String objDump = Path.Combine(objDumpPath, "objdump.exe");
            String fileToOpen = Path.Combine(filePath, fileName);

            Console.WriteLine(objDump);
            Console.WriteLine(fileToOpen);

            String args = "-d " + fileToOpen;

            Console.WriteLine(args);

            //Run objdump to disassemble the given executeable
            using (Process process = new Process())
            {
                process.StartInfo.FileName = objDump;
                process.StartInfo.Arguments = args;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                // Synchronously read the standard output of the spawned process. 
                StreamReader reader = process.StandardOutput;
                string output = reader.ReadToEnd();

                process.WaitForExit();
                return output;
            }
        }

        public static List<Node> CreateNodes(string asmCode, Graphics g)
        {
            List<Node> nodes = new List<Node>();

            string[] split = asmCode.Split('\n');

            LineOfCode[] lines = new LineOfCode[split.Length];
            for (int i = 0; i < split.Length; i++)
            {
                lines[i] = new LineOfCode(split[i]);
            }


            string label = "BEGINNING OF FILE";
            int counter = 0;

            for (int i = 0; i < split.Length; i++)
            {
                bool isNewBlock = false;
                //For labeling the blocks in the same segment
                //string line = split[i];
                LineOfCode loc = lines[i];
                if (loc.IsLabel())
                {
                    label = loc.getLabel();
                    isNewBlock = true;
                    counter = 0;
                }
                if(loc.IsJmp())
                {
                    isNewBlock = true;
                    counter++;
                }

                //Search for the end and create the new block
                if (isNewBlock)
                {
                    List<LineOfCode> blockCode = new List<LineOfCode>();
                    int j = i + 1;

                    while (!lines[j].IsLabel())  //!isLabel(split[j])
                    {
                        //Skip empty lines
                        if(lines[j].IsStatement()) blockCode.Add(lines[j]);
                        if (lines[j].IsJmp()) break;
                        j++;
                        if (j >= split.Length) break;
                    }

                    string blockLabel = loc.getAddress() + ": " + label;
                    if (counter != 0) blockLabel += " (" + counter.ToString() + ")";

                    Node newNode = new Node(blockLabel, blockCode.ToArray(), new Point(100, nodes.Count * 50), g);

                    nodes.Add(newNode);
                }

            }

            CreateConnections(ref nodes);

            return nodes;
        }

        /// <summary>
        /// Creates the connecting lines between the nodes
        /// </summary>
        /// <param name="nodes">The list of nodes to iterate through.</param>
        public static void CreateConnections(ref List<Node> nodes)
        {
            int size = nodes.Count;
            for (int i = 0; i < size; i++)
            {
                Node currentNode = nodes[i];
                LineOfCode[] lines = currentNode.getASMCode();

                //todo: Should remove them entirely from nodes
                if (lines.Length == 0 && i != size - 1)
                {
                    currentNode.ConnectToAsDefault(nodes[i + 1]);
                    continue;
                }

                //TODO
                //Connect it to the next node by default

                LineOfCode lastLine = lines[lines.Length - 1];
                if (i != size - 1 && !lastLine.IsUnconditionalJmp())
                {
                    currentNode.ConnectToAsDefault(nodes[i + 1]);
                }

                if (lastLine.IsValidJmpAddress()) {
                    Node jumpDestination = getDestinationNodeFromJMP(lastLine, nodes);
                    if (jumpDestination != null) currentNode.ConnectToByJump(jumpDestination);
                }
            }
        }

        private static Node getDestinationNodeFromJMP(LineOfCode line, List<Node> nodes)
        {
            foreach (Node n in nodes)
            {
                if (n.hasAddress(line.getJmpAddress())) return n;
            }
            return null;
        }

    }
}
