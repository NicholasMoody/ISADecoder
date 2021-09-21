using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISADecoder {
    class Instruction {
        public Type type;
        public Mnemonic mnemonic;
        public AddressingMode addressingMode = AddressingMode.None;
        public int r1 = -1;
        public int r2 = -1;
        public int op1 = -1;
        public int op2 = -1;
        public int instSize = -1; // instruction size in bytes (including operands). will be used for PC calculations 

        
        public override string ToString() {
            string output = mnemonic.ToString();
            if (r1 != -1) {
                output += ", R" + r1; 
            }
            return output + Environment.NewLine;
        }
    }
}
