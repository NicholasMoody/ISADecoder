using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ISADecoder {
    class Decoder {
        public List<Instruction> instructions = new List<Instruction>();
        public List<byte> binInput = new List<byte>(); // string input parsed to bytes then interpreted via bitwise operations as per requirements
    
        public Decoder(string input) {
            ParseToBinary(input, binInput);
            BinaryToInstructions(binInput);
        }
        
        // where the memes happen
        private void BinaryToInstructions(List<byte> bytes) {
            byte msb;
            byte lsb;
            int loc = 0;

            do {
                // read instruction bytes 
                msb = bytes[loc];
                lsb = bytes[loc + 1];
                int instSpec = (msb >> 3); // value of the first 5 bits, used to identify instruction 
                // may not be used for all instructions, but the bits will exist regardless so w/e
                int addressMode = (msb & 0b00000111); // isolates last three bits of msb
                int r1 = (lsb >> 4); // most significant 4 bits of lsb
                int r2 = (lsb & 0b00001111); // least significant 4 bits of lsb

                Instruction inst = new Instruction();

                inst.instSize = 2; // every instruction will be at least 2 bytes, add more for operands

                // identify instruction based on spec and populate instruction object accordingly
                // comparisons done in binary for simplicity
                switch (instSpec) {
                    case 0b00000:
                        inst.mnemonic = Mnemonic.STOP;
                        inst.type = Type.ControlFlow;
                        break;
                    case 0b00001:
                        inst.mnemonic = Mnemonic.LD;
                        inst.type = Type.Memory;
                        inst.r1 = r1;
                        // pull memory loc here PLACEHOLDER
                        break;
                    case 0b00010:
                        inst.mnemonic = Mnemonic.ST;
                        inst.type = Type.Memory;
                        inst.r1 = r1;
                        // pull memory loc here PLACEHOLDER
                        break;
                    case 0b00011:
                        inst.mnemonic = Mnemonic.MOV;
                        inst.type = Type.DataHandling;
                        inst.r1 = r1;
                        inst.addressingMode = GetAddressingMode(addressMode);
                        // pull operand here PLACEHOLDER
                        break;
                    case 0b00100:
                        inst.mnemonic = Mnemonic.COM;
                        inst.type = Type.DataHandling;
                        inst.r1 = r1;
                        inst.addressingMode = GetAddressingMode(addressMode);
                        // pull operand here PLACEHOLDER
                        break;
                    case 0b00101:
                        inst.mnemonic = Mnemonic.B;
                        inst.type = Type.ControlFlow;
                        // pull operand here PLACEHOLDER
                        break;
                    case 0b00110:
                        inst.mnemonic = Mnemonic.BL;
                        inst.type = Type.ControlFlow;
                        // pull operand here PLACEHOLDER
                        break;
                    case 0b00111:
                        inst.mnemonic = Mnemonic.BLE;
                        inst.type = Type.ControlFlow;
                        // pull operand here PLACEHOLDER
                        break;
                    case 0b01000:
                        inst.mnemonic = Mnemonic.BG;
                        inst.type = Type.ControlFlow;
                        // pull operand here PLACEHOLDER
                        break;
                    case 0b01001:
                        inst.mnemonic = Mnemonic.BGE;
                        inst.type = Type.ControlFlow;
                        // pull operand here PLACEHOLDER
                        break;
                    case 0b01010:
                        inst.mnemonic = Mnemonic.BE;
                        inst.type = Type.ControlFlow;
                        // pull operand here PLACEHOLDER
                        break;
                    case 0b01011:
                        inst.mnemonic = Mnemonic.BNE;
                        inst.type = Type.ControlFlow;
                        // pull operand here PLACEHOLDER
                        break;
                    case 0b01100:
                        inst.mnemonic = Mnemonic.ADD;
                        inst.type = Type.Arithmetic;
                        inst.r1 = r1;
                        inst.addressingMode = GetAddressingMode(addressMode);
                        // pull operand here PLACEHOLDER
                        break;
                    case 0b01101:
                        inst.mnemonic = Mnemonic.SUB;
                        inst.type = Type.Arithmetic;
                        inst.r1 = r1;
                        inst.addressingMode = GetAddressingMode(addressMode);
                        // pull operand here PLACEHOLDER
                        break;
                    case 0b01110:
                        inst.mnemonic = Mnemonic.ASL;
                        inst.type = Type.Arithmetic;
                        inst.r1 = r1;
                        inst.addressingMode = GetAddressingMode(addressMode);
                        // pull operand here PLACEHOLDER
                        break;
                    case 0b01111:
                        inst.mnemonic = Mnemonic.LSR;
                        inst.type = Type.Arithmetic;
                        inst.r1 = r1;
                        inst.addressingMode = GetAddressingMode(addressMode);
                        // pull operand here PLACEHOLDER
                        break;
                    case 0b10000:
                        inst.mnemonic = Mnemonic.ASR;
                        inst.type = Type.Arithmetic;
                        inst.r1 = r1;
                        inst.addressingMode = GetAddressingMode(addressMode);
                        // pull operand here PLACEHOLDER
                        break;
                    case 0b10001:
                        inst.mnemonic = Mnemonic.LSL;
                        inst.type = Type.Arithmetic;
                        inst.r1 = r1;
                        inst.addressingMode = GetAddressingMode(addressMode);
                        // pull operand here PLACEHOLDER
                        break;
                    case 0b10010:
                        inst.mnemonic = Mnemonic.MULT;
                        inst.type = Type.Arithmetic;
                        inst.r1 = r1;
                        inst.addressingMode = GetAddressingMode(addressMode);
                        // pull operand here PLACEHOLDER
                        break;
                    default:
                        throw new Exception("Invalid instruction specifier");
                }
                // all instructions other than STOP require an operand
                if (inst.mnemonic != Mnemonic.STOP) {
                    if (loc + 3 >= bytes.Count)
                        throw new Exception("Invalid instruction: cannot fetch operand");
                    inst.instSize += 2;
                    // fetch operand and shove into int 
                    int operand = bytes[loc + 2];
                    operand <<= 8;
                    operand += bytes[loc + 3];
                    inst.op1 = operand;
                    loc += 2;
                }

                if (inst.addressingMode == AddressingMode.SecondRegister && inst.op1 > 0b1111) {
                    string message = $"Invalid instruction for: {inst.ToString()}" + Environment.NewLine;
                    message += $"Cannot access register 0x{inst.op1:X4} on 16 register ISA (addressing mode: second register).";
                    throw new Exception(message);
                }

                if (inst.addressingMode == AddressingMode.Invalid)
                    throw new Exception($"Invalid addressing mode for instruction {inst} (0x{msb:X2}{lsb:X2})");
                instructions.Add(inst);
                loc += 2;
            } while (!(msb == 0 && lsb == 0)); // loop until stop instruction (or we fail out for invalid input)
        }

        private void ParseToBinary(string input, List<byte> output) {
            input = input.Replace(" ", "");
            input = input.Replace("\n\n", "");
            input = input.Replace(Environment.NewLine, "");
            // parse to binary array 
            for (int i = 0; i < input.Length; i += 2) {
                if (CharToHex(input[i]) == -1 || CharToHex(input[i + 1]) == -1)
                    throw new Exception("Invalid input character/s, please try again.");
                byte val = (byte)CharToHex(input[i]); // get hex value of first char of byte
                val <<= 4; // shift bits to left of byte
                val += (byte)CharToHex(input[i + 1]); // fill up 4 least significant bits of byte 
                binInput.Add(val);
            }
        }

        /// <summary>
        /// Converts hex char to its corresponding 0-15 hex value
        /// </summary>
        /// <param name="c">the character</param>
        /// <returns>hex value if valid, -1 if invalid hex char</returns>
        private int CharToHex (char c) {
            if (c >= '0' && c <= '9')
                return (c - 48);
            else if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                return (char.ToUpper(c) - 55);
            return -1;
        }

        private AddressingMode GetAddressingMode(int val) {
            if (val == 0)
                return AddressingMode.SecondRegister;
            else if (val == 1)
                return AddressingMode.MemLoc;
            else if (val == 2)
                return AddressingMode.Immediate;
            else
                return AddressingMode.Invalid;
        }
    }
}
