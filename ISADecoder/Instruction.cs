using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace ISADecoder {
    class Instruction {
        public Type type;
        public Mnemonic mnemonic;
        public AddressingMode addressingMode = AddressingMode.None;
        
        public short r1 = -1;
        public short r2 = -1;
        public short op1 = -1; // operand 1
        public short op2 = -1; // least significant byte of mem address
        public short instSize = -1; // instruction size in bytes (including operands). will be used for PC calculations 
        public short address = -1;

        
        public override string ToString() {
            string output = mnemonic.ToString();
            if (r1 != -1) {
                output += $" R{r1}"; 
            }
            if (op1 != -1 || addressingMode == AddressingMode.SecondRegister) {
                if (addressingMode == AddressingMode.Immediate)
                    output += $", {op1}";
                else if (addressingMode == AddressingMode.MemLoc)
                    output += $", 0x{op1:X3}{op2:X2}";
                else if (addressingMode == AddressingMode.SecondRegister)
                    output += $", R{r2}";
                else if (addressingMode == AddressingMode.None)
                    output += $", 0x{op1:X4}";
            }
            return output;
        }

        private string GetOperandFormatting() {
            switch (addressingMode) {
                case AddressingMode.SecondRegister:
                    return $"value in R{r2}";
                case AddressingMode.MemLoc:
                    return $"value in 0x{op1:X3}{op2:X2}";
                case AddressingMode.Immediate:
                    return $"{op1}";
            }
            return "";
        }

        public string GetDescription() {
            string s = "";

            switch (mnemonic) {
                case Mnemonic.LD:
                    s = $"Load value from {GetOperandFormatting()} to R{r1}";
                    if (addressingMode == AddressingMode.MemLoc)
                        s = $"Load value from {GetOperandFormatting()} to R{r1}";
                    else if (addressingMode == AddressingMode.SecondRegister)
                        s = $"Load value from address in {GetOperandFormatting()} to R{r1}";
                    else if (addressingMode == AddressingMode.Immediate)
                        s = $"Load value from 0x{op1:X4} to R{r1}";
                    break;
                case Mnemonic.ST:
                    if (addressingMode == AddressingMode.MemLoc)
                        s = $"Store value from R{r1} to {GetOperandFormatting()}";
                    else if (addressingMode == AddressingMode.SecondRegister)
                        s = $"Store value from R{r1} to address in {GetOperandFormatting()}";
                    else if (addressingMode == AddressingMode.Immediate)
                        s = $"Store value from R{r1} to 0x{op1:X4}";
                    break;
                case Mnemonic.MOV:
                    s = $"Move {GetOperandFormatting()} to R{r1}";
                    break;
                case Mnemonic.COM:
                    s = $"Compare value in R{r1} to {GetOperandFormatting()}";
                    break;
                case Mnemonic.B:
                    s = $"Branch unconditionally to 0x{op1:X4}";
                    break;
                case Mnemonic.BL:
                    s = $"Branch to 0x{op1:X4} if N";
                    break;
                case Mnemonic.BLE:
                    s = $"Branch to 0x{op1:X4} if N or Z";
                    break;
                case Mnemonic.BG:
                    s = $"Branch to 0x{op1:X4} if not N";
                    break;
                case Mnemonic.BGE:
                    s = $"Branch to 0x{op1:X4} if Z or not N";
                    break;
                case Mnemonic.BE:
                    s = $"Branch to 0x{op1:X4} if Z";
                    break;
                case Mnemonic.BNE:
                    s = $"Branch to 0x{op1:X4} if not Z";
                    break;
                case Mnemonic.STOP:
                    s = $"End program";
                    break;
                case Mnemonic.ADD:
                    s = $"Add {GetOperandFormatting()} to R{r1}";
                    break;
                case Mnemonic.SUB:
                    s = $"Subtract {GetOperandFormatting()} from R{r1}";
                    break;
                case Mnemonic.ASL:
                    s = $"Arithmetic shift left of R{r1} by {GetOperandFormatting()} bits";
                    break;
                case Mnemonic.LSR:
                    s = $"Logical shift right of R{r1} by {GetOperandFormatting()} bits";
                    break;
                case Mnemonic.ASR:
                    s = $"Arithmetic shift right of R{r1} by {GetOperandFormatting()} bits";
                    break;
                case Mnemonic.LSL:
                    s = $"Logical shift left of R{r1} by {GetOperandFormatting()} bits";
                    break;
                case Mnemonic.MULT:
                    s = $"Multiply register R{r1} by {GetOperandFormatting()}";
                    break;
            }
            return s;
        }
    }
}
