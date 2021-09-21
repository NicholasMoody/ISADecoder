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
            return output;
        }

        public string GetDescription() {
            string s = "";

            switch (mnemonic) {
                case Mnemonic.LD:
                    s = $"Load value from memory address to R{r1}";
                    break;
                case Mnemonic.ST:
                    s = $"Store value from R{r1} to memory address";
                    break;
                case Mnemonic.MOV:
                    s = $"Move operand to R{r1}";
                    break;
                case Mnemonic.COM:
                    s = $"Compare value in R{r1} to operand";
                    break;
                case Mnemonic.B:
                    s = $"Branch unconditionally to operand";
                    break;
                case Mnemonic.BL:
                    s = $"Branch to operand if N";
                    break;
                case Mnemonic.BLE:
                    s = $"Branch to operand if N or Z";
                    break;
                case Mnemonic.BG:
                    s = $"Branch to operand if not N";
                    break;
                case Mnemonic.BGE:
                    s = $"Branch to operand if Z or not N";
                    break;
                case Mnemonic.BE:
                    s = $"Branch to operand if Z";
                    break;
                case Mnemonic.BNE:
                    s = $"Branch to operand if not Z";
                    break;
                case Mnemonic.STOP:
                    s = $"End program";
                    break;
                case Mnemonic.ADD:
                    s = $"Add value in R{r1} to operand";
                    break;
                case Mnemonic.SUB:
                    s = $"Subtract value in operand from R{r1}";
                    break;
                case Mnemonic.ASL:
                    s = $"Arithmetic shift left of value in R{r1} by operand bits";
                    break;
                case Mnemonic.LSR:
                    s = $"Logical shift right of value in R{r1} by operand bits";
                    break;
                case Mnemonic.ASR:
                    s = $"Arithmetic shift right of value in R{r1} by operand bits";
                    break;
                case Mnemonic.LSL:
                    s = $"Logical shift left of value in R{r1} by operand bits";
                    break;
                case Mnemonic.MULT:
                    s = $"Multiply register R{r1} by operand";
                    break;
            }
            return s;
        }
    }
}
