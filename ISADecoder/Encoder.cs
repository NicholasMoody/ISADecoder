using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISADecoder
{
    public static class Encoder
    {
        /// <summary>
        /// Encodes an instruction string into a binary value 
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <returns>The binary value</returns>
        public static uint InstrToBinary(string instruction)
        {
            uint binary = 0b0;

            //Remove any commas from the instruction
            instruction = instruction.Replace(",", "");

            //Split the instruction into 3 parts: instruction proper; operand 1; operand 2 (optional)
            List<string> instructions;
            instructions = instruction.Split(' ').ToList();

            //If instruction is stop, immediately return 0
            if(instructions[0] == "STOP")
            {
                binary = 0b0;
                return binary;
            }    

            //Set the first 5 bits based on the instruction
            switch(instructions[0])
            {
                case "LD":
                    binary += 0b00001;
                    break;
                case "ST":
                    binary += 0b00010;
                    break;
                case "B":
                    binary += 0b00101;
                    break;
                case "BL":
                    binary += 0b00110;
                    break;
                case "BLE":
                    binary += 0b00111;
                    break;
                case "BG":
                    binary += 0b01000;
                    break;
                case "BGE":
                    binary += 0b01001;
                    break;
                case "BE":
                    binary += 0b01010;
                    break;
                case "BNE":
                    binary += 0b01011;
                    break;
                case "ADD":
                    binary += 0b01100;
                    break;
                case "SUB":
                    binary += 0b01101;
                    break;
                case "ASL":
                    binary += 0b01110;
                    break;
                case "LSL":
                    binary += 0b10001;
                    break;
                case "ASR":
                    binary += 0b01111;
                    break;
                case "LSR":
                    binary += 0b10000;
                    break;
                case "MULT":
                    binary += 0b10010;
                    break;
                case "MOV":
                    binary += 0b00011;
                    break;
                case "COM":
                    binary += 0b00100;
                    break;
                default:
                    break;
            }

            /*First Operand in instruction*/
            //Instruction has no register: Only touches memory
            if(instructions[1].Contains("0x"))
            {
                //Remove the hex identifier from the string
                instructions[1] = instructions[1].Replace("0x", "");

                //Shift 7 bits: 3 for the operand type (000), 4 for unused bits
                binary <<= 7;

                //Set bits for each hex character in address
                foreach (char character in instructions[1])
                {
                    //Shift 4 bits to make space for binary representation of hex character
                    binary <<= 4;
                    binary += HexCharToBin(character);
                }

                //Instruction converted to binary
                return binary;
            }
            //Register used in instruction
            else
            {
                //Shift 3 bits to make space for the operand type (will be determined later)
                binary <<= 3;

                //Remove register identifier from string
                instructions[1] = instructions[1].Replace("R", "");

                //Shift 4 bits to make space for the register's binary value
                binary <<= 4;
                binary += HexCharToBin(instructions[1][0]);
            }


            /*Second Operand in instruction*/
            //Second operand is a register 000
            if (instructions[2].Contains("R"))
            {
                //Remove register identifier from string
                instructions[2] = instructions[2].Replace("R", "");

                //Shift bits 4 to make space for second register
                binary <<= 4;
                binary += HexCharToBin(instructions[2][0]);

                //Shift 16 bits to make upper 16 bits the instruction
                binary <<= 16;
            }
            //Second operand is a memory address 001
            else if(instructions[2].Contains("0x"))
            {
                //Adjust bits for second operand type
                binary += 0b0010000;

                //Remove hex identifier from string
                instructions[2] = instructions[2].Replace("0x", "");

                //Convert address from hex characters to binary; adjust binary string
                foreach (char character in instructions[2])
                {
                    //Shift 4 bits to make space for binary representation of hex character
                    binary <<= 4;
                    binary += HexCharToBin(character);
                }
            }
            //Second operand is an immediate 010
            else
            {
                //Adjust bits for second operand type
                binary += 0b0100000;

                //Shift 4 for unused bits
                binary <<= 4;

                //Convert immediate from hex characters to binary; adjust binary string
                foreach (char character in instructions[2])
                {
                    //Shift 4 bits to make space for binary representation of hex character
                    binary <<= 4;
                    binary += HexCharToBin(character);
                }
            }
            return binary;
        }



        /// <summary>
        /// Convert an instruction from a 
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <returns></returns>
        public static string InstrToHex(string instruction)
        {
            return BinInstrToHex(InstrToBinary(instruction));
        }



        /// <summary>
        /// Takes a string version of a hexadecimal character and converts it into a binary representation
        /// </summary>
        /// <param name="hexChar">The hexadecimal character.</param>
        /// <returns>The binary value</returns>
        public static uint HexCharToBin(char hexChar)
        {
            uint binary = 0b0;

            switch (hexChar)
            {
                case '0':
                    binary = 0b0000;
                    break;
                case '1':
                    binary = 0b0001;
                    break;
                case '2':
                    binary = 0b0010;
                    break;
                case '3':
                    binary = 0b0011;
                    break;
                case '4':
                    binary = 0b0100;
                    break;
                case '5':
                    binary = 0b0101;
                    break;
                case '6':
                    binary = 0b0110;
                    break;
                case '7':
                    binary = 0b0111;
                    break;
                case '8':
                    binary = 0b1000;
                    break;
                case '9':
                    binary = 0b1001;
                    break;
                case 'A':
                case 'a':
                    binary = 0b1010;
                    break;
                case 'B':
                case 'b':
                    binary = 0b1011;
                    break;
                case 'C':
                case 'c':
                    binary = 0b1100;
                    break;
                case 'D':
                case 'd':
                    binary = 0b1101;
                    break;
                case 'E':
                case 'e':
                    binary = 0b1110;
                    break;
                case 'F':
                case 'f':
                    binary = 0b1111;
                    break;
                default:
                    break;
            }

            return binary;
        }



        /// <summary>
        /// Takes a binary instruction and converts it into a hex string representation of the instruction
        /// </summary>
        /// <param name="binary">The binary instruction.</param>
        /// <returns>The hex string representation</returns>
        public static string BinInstrToHex(uint binary)
        {
            string instruction = "";

            //Shift the binary right 27 bits to get the instruction
            uint instr = binary >> 27;

            //Shift the binary right 24 bits and apply a bit mask to get the operand type
            uint operandType = binary >> 24;
            operandType &= 0b00000111;

            //Uint to hold number of bits to convert to hex
            uint bitCount = 16;

            //Get number of bits to convert from instruction and operand type
            switch (instr)
            {
                //Stop Instruction
                case 0b00000:
                    bitCount = 16;
                    break;
                //Load and Store Instructions
                case 0b00001:
                case 0b00010:
                //Branching Instructions
                case 0b00101:
                case 0b00110:
                case 0b00111:
                case 0b01000:
                case 0b01001:
                case 0b01010:
                case 0b01011:
                    bitCount = 32;
                    break;
                //Arithmetic Instructions & Move and Compare Instructions
                case 0b01100:
                case 0b01101:
                case 0b01110:
                case 0b10001:
                case 0b01111:
                case 0b10000:
                case 0b10010:
                case 0b00011:
                case 0b00100:
                    if(operandType == 0b000)
                        bitCount = 16;
                    else
                        bitCount = 32;
                    break;
                //Default: Do nothing
                default:
                    break;
            }

            //Bit mask for conversions
            uint bitmask = 0b11110000000000000000000000000000;
            //Distance to shift target bits right to get byte in least significant 4 bits
            int shiftDist = 28;
            //Check nibbles converted for formatting purposes
            int nibblesConverted = 0;

            //Convert binary to hex string
            for(uint i = 0; i < bitCount; i += 4)
            {
                uint byteToConvert = binary & bitmask;
                byteToConvert >>= shiftDist;

                //Adjust bitmask and shiftDist for next nibble
                bitmask >>= 4;
                shiftDist -= 4;

                instruction += BinNibbleToHex(byteToConvert);
                nibblesConverted += 1;

                if (shiftDist >= 0 && i+4 != bitCount && nibblesConverted == 2)
                {
                    instruction += " ";
                    nibblesConverted = 0;
                }
            }

            return instruction;
        }



        /// <summary>
        /// Takes the lower 4 bits of a uint and converts it into the corresponding hex character (as a string)
        /// </summary>
        /// <param name="binary">The uint with the nibble to convert</param>
        /// <returns>The hex character</returns>
        public static string BinNibbleToHex(uint binary)
        {
            string hex = "";

            switch (binary)
            {
                case 0b0000:
                    hex = "0";
                    break;
                case 0b0001:
                    hex = "1";
                    break;
                case 0b0010:
                    hex = "2";
                    break;
                case 0b0011:
                    hex = "3";
                    break;
                case 0b0100:
                    hex = "4";
                    break;
                case 0b0101:
                    hex = "5";
                    break;
                case 0b0110:
                    hex = "6";
                    break;
                case 0b0111:
                    hex = "7";
                    break;
                case 0b1000:
                    hex = "8";
                    break;
                case 0b1001:
                    hex = "9";
                    break;
                case 0b1010:
                    hex = "A";
                    break;
                case 0b1011:
                    hex = "B";
                    break;
                case 0b1100:
                    hex = "C";
                    break;
                case 0b1101:
                    hex = "D";
                    break;
                case 0b1110:
                    hex = "E";
                    break;
                case 0b1111:
                    hex = "F";
                    break;
            }

            return hex;
        }
    }
}
