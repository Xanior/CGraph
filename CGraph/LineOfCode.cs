using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGraph
{
    class LineOfCode
    {
        private bool isStatement = false;
        private bool isJmp = false;
        private bool isUnconditionalJmp = false;
        private bool isLabel = false;

        private string label = "**ERROR INTERPRETING LABEL**";
        private string line;
        private string formattedLine;

        private string command = "**ERROR INTERPRETING COMMAND**";
        private string address = "**ERROR INTERPRETING ADDRESS**";
        private string jmpAddress = "**ERROR INTERPRETING ADDRESS**";
        private bool isValidAddress = false;
        private bool isValidJmpAddress = false;

        public LineOfCode(string line)
        {
            this.line = line;
            FormatLine();

            if (line.Length < 1) return;

            int minSize = 3;
            isStatement = true;


            string[] split = line.Split('\t');

            CalculateAddress();

            //Check if it's a label
            isLabel = line.Contains(">:");
            if (isLabel)
            {
                label = label = line.Split(' ')[1];
                return;
            }

            if (split.Length < minSize) return;

            //Get the command, and check if it's a jump
            InterpretCommand();
            if (command.StartsWith("j") || command.Equals("call")) isJmp = true;
            if (command.Equals("jmp")) isUnconditionalJmp = true;

            if (isJmp) CalculateJmpAddress();
        }

        public bool IsUnconditionalJmp()
        {
            return isUnconditionalJmp;
        }

        public bool IsLabel()
        {
            return isLabel;
        }

        public bool IsStatement()
        {
            return isStatement;
        }

        public bool IsJmp()
        {
            return isJmp;
        }

        public bool IsValidAddress()
        {
            return isValidAddress;
        }

        public bool IsValidJmpAddress()
        {
            return isValidJmpAddress;
        }

        public string getLabel()
        {
            return label;
        }

        public string getFormattedLine()
        {
            return formattedLine;
        }

        public string getAddress()
        {
            return address;
        }

        public string getJmpAddress()
        {
            return jmpAddress;
        }


        //Remove hex representation
        private void FormatLine()
        {
            string[] split = line.Split('\t');
            if (split.Length < 3)
            {
                formattedLine = line;
                return;
            }
            //  4010b1:	b8 ff ff ff ff       	mov    $0xffffffff,%eax    -> 4010b1:   mov   $0xffffffff,%eax
            string ret = split[0];
            ret += '\t';
            ret += split[2];

            formattedLine = ret;
        }

        private void CalculateAddress()
        {
            //Split at whitespaces
            string[] split = line.Split(null);

            for(int i = 0; i < split.Length; i++)
            {
                if(split[i].Length > 3)
                {
                    address = split[i].TrimEnd(':');
                    address = address.TrimStart('0');
                    isValidAddress = true;
                    return;
                }
            }
        }

        private void CalculateJmpAddress()
        {
            string[] split = line.Split('\t');
            string[] split2 = split[2].Split(' ');

            for (int i = 1; i < split2.Length; i++)
            {
                if (split2[i].Length > 1)
                {
                    jmpAddress = split2[i];
                    isValidJmpAddress = true;
                    //remove everything thats not a hex number or a *
                    jmpAddress = jmpAddress.Replace("[^A-Fa-f0-9*]", "");
                    return;
                }
            }
        }

        private void InterpretCommand()
        {
            string[] split = line.Split('\t');
            string tmp = split[2];
            command = tmp.Split(null)[0];
        }
    }
}
