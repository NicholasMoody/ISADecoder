using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ISADecoder {
    public partial class MainForm : Form {
        TextBox[] registers = new TextBox[16];
        List<Instruction> instructions = new List<Instruction>();
        byte[] memory = new byte[1048576]; // stores memory of program for simulation 
        bool done = false;
        short PCAddr = 0; // internal PC count so that register text can easily be updated at right time 

        public MainForm() {
            InitializeComponent();
            // build register array
            int rCount = 0;
            for (int i = 0; i < memory.Length; i++) {
                memory[i] = 74;
            }
            foreach (TextBox t in this.Controls.OfType<TextBox>()) {
                if (t.Name.StartsWith("tbR")) {
                    t.Text = "0x0000";
                    registers[rCount] = t;
                    rCount++;
                } 
            }
            Array.Reverse(registers);
        }

        private void btnDecode_Click(object sender, EventArgs e) {
            lbOutput.Items.Clear();
            tbInstructionDescription.Text = "";
            short pc = 0; 
            try {
                Decoder d = new Decoder(tbInput.Text);

                foreach (Instruction i in d.instructions) {
                    lbOutput.Items.Add("0x" + pc.ToString("X4") + " | " + i.ToString());
                    i.address = pc;
                    pc += i.instSize;
                }

                CalculateStats(d.instructions);

                instructions = d.instructions;
            }
            catch (ArgumentOutOfRangeException) {
                MessageBox.Show("Program must end with the STOP instruction (0x0000).", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalculateStats(List<Instruction> insts) {
            int controlFlow = 0;
            int memory = 0;
            int arith = 0;
            int data = 0;
            // addressing modes
            int secondReg = 0;
            int memLoc = 0;
            int immediate = 0;

            foreach (Instruction i in insts) {
                if (i.type == Type.Arithmetic)
                    arith++;
                else if (i.type == Type.ControlFlow)
                    controlFlow++;
                else if (i.type == Type.Memory)
                    memory++;
                else if (i.type == Type.DataHandling)
                    data++;

                if (i.addressingMode == AddressingMode.SecondRegister)
                    secondReg++;
                else if (i.addressingMode == AddressingMode.MemLoc)
                    memLoc++;
                else if (i.addressingMode == AddressingMode.Immediate)
                    immediate++;
            }

            string final = "Stats:\n----------------------------\n";
            final += "Total instructions: \t" + insts.Count;
            final += "\nControl Flow: \t" + controlFlow;
            final += "\nMemory: \t" + memory;
            final += "\nArithmetic: \t" + arith;
            final += "\nData Handling: \t" + data;

            final += "\n\nAddressing Modes: \n----------------------------";

            final += "\nSecond Register: \t" + secondReg;
            final += "\nMemory Location: \t" + memLoc;
            final += "\nImmediate: \t" + immediate;

            lblStats.Text = final;
        }

        private void lbOutput_SelectedIndexChanged(object sender, EventArgs e) {
            if (lbOutput.SelectedIndex < instructions.Count && lbOutput.SelectedIndex >= 0) {
                Instruction i = instructions[lbOutput.SelectedIndex];
                tbInstructionDescription.Text = i.GetDescription();
            }
        }

        /// <summary>
        /// Execute the given instruction 
        /// </summary>
        /// <param name="address">PC address of instruction</param>
        private void Step() {
            Instruction inst = new Instruction();

            ResetMemoryTouchedTB();

            foreach (Instruction i in instructions) {
                if (PCAddr == i.address) {
                    inst = i;
                    break;
                }
            }

            registers[14].Text = ToHexString(PCAddr); // update PC 

            PCAddr += inst.instSize; // update PC for next instruction. will be overwritten if branch. 

            short registerValue = 0;
            if (inst.r1 >= 0) 
                registerValue = Convert.ToInt16(registers[inst.r1].Text, 16); // value stored in current instruction's register
            int operandVal = GetOperandValByAddressingMode(inst); // may not be used for given instruction, doesn't matter
            switch (inst.mnemonic) {
                case Mnemonic.LD:
                    tbMemTouched.Text = ToHexString(inst.op1);
                    tbPrevValue.Text = ToHexString(memory[inst.op1]);
                    registers[inst.r1].Text = ToHexString(memory[inst.op1]); // load value in memory address to register
                    tbNewValue.Text = ToHexString(memory[inst.op1]);
                    break;
                case Mnemonic.ST:
                    tbMemTouched.Text = ToHexString(inst.op1);
                    tbPrevValue.Text = ToHexFromMemory(inst.op1);

                    memory[inst.op1] = (byte)(registerValue >> 8); // most significant byte of value
                    memory[inst.op1 + 1] = (byte)(registerValue & 0b11111111); // least significant byte of value

                    tbNewValue.Text = ToHexFromMemory(inst.op1);
                    break;
                case Mnemonic.MOV:
                    // moves operand into register depending on addressing mode 
                    registers[inst.r1].Text = ToHexString(operandVal);
                    break;
                case Mnemonic.COM:
                    // subtracts operand value from register value and sets flags
                    int comp = HexToInt(registers[inst.r1].Text) - operandVal;
                    int flag = 0;
                    if (comp == 0) {  
                        flag |= 0b100; // sets zero flag
                        flag &= 0b0111; // unsets negative flag
                    }
                    if (comp < 0) {
                        flag |= 0b1000; // sets negative flag
                        flag &= 0b1011; // unset zero flag
                    }
                    registers[15].Text = ToHexString(flag);
                    break;
                case Mnemonic.B:
                    PCAddr = inst.op1;
                    break;
                case Mnemonic.BL:
                    if (NFlagSet())
                        PCAddr = inst.op1;
                    break;
                case Mnemonic.BLE:
                    if (ZFlagSet() || NFlagSet())
                        PCAddr = inst.op1;
                    break;
                case Mnemonic.BG:
                    if (!NFlagSet())
                        PCAddr = inst.op1;
                    break;
                case Mnemonic.BGE:
                    if (ZFlagSet() || !NFlagSet())
                        PCAddr = inst.op1;
                    break;
                case Mnemonic.BE:
                    if (ZFlagSet())
                        PCAddr = inst.op1;
                    break;
                case Mnemonic.BNE:
                    if (!ZFlagSet())
                        PCAddr = inst.op1;
                    break;
                case Mnemonic.STOP:
                    done = true;
                    break;
                case Mnemonic.ADD:
                    registers[inst.r1].Text = ToHexString(registerValue + operandVal);
                    break;
                case Mnemonic.SUB:
                    registers[inst.r1].Text = ToHexString(registerValue - operandVal);
                    break;
                // NOTE: all arithmetic and logical shifts do the same thing here
                // either change how ints are handled or remove one type of shift (easier)
                case Mnemonic.ASL:
                    registers[inst.r1].Text = ToHexString(registerValue << operandVal);
                    break;
                case Mnemonic.LSR:
                    registers[inst.r1].Text = ToHexString(registerValue >> operandVal);
                    break;
                case Mnemonic.ASR:
                    registers[inst.r1].Text = ToHexString(registerValue >> operandVal);
                    break;
                case Mnemonic.LSL:
                    registers[inst.r1].Text = ToHexString(registerValue << operandVal);
                    break;
                case Mnemonic.MULT:
                    registers[inst.r1].Text = ToHexString(operandVal * registerValue);
                    break;
            }
        }

        private int GetOperandValByAddressingMode(Instruction inst) {
            int val = 0;
            if (inst.addressingMode == AddressingMode.Immediate)
                val = inst.op1;
            else if (inst.addressingMode == AddressingMode.MemLoc)
                val = memory[inst.op1];
            else if (inst.addressingMode == AddressingMode.SecondRegister)
                val = Convert.ToInt32(registers[inst.op1].Text, 16);
            return val;
        }

        private string ToHexString(int val) {
            return "0x" + val.ToString("X4");
        }

        private string ToHexFromMemory(int address) {
            return $"0x{memory[address]:X2}{memory[address + 1]:X2}";
        }

        private bool NFlagSet() {
            int flag = HexToInt(registers[15].Text) >> 3;
            return flag % 2 == 1;
        }

        private bool ZFlagSet() {
            int flag = HexToInt(registers[15].Text) >> 2;
            return flag % 2 == 1;
        }

        private int HexToInt(string hex) {
            return Convert.ToInt16(hex, 16);
        }

        private void btnRun_Click(object sender, EventArgs e) {
            lbOutput.SelectedIndex = 0;
            lbOutput.Enabled = false;
            btnStep.Enabled = true;
            btnRun.Enabled = false;
            Step();
            btnDecode.Enabled = false;
            done = false;
        }

        private void btnStep_Click(object sender, EventArgs e) {
            if (done) { // reset memes 
                lbOutput.SelectedIndex = 0;
                lbOutput.Enabled = true;
                btnStep.Enabled = false;
                btnRun.Enabled = true;
                btnDecode.Enabled = true;
                registers[14].Text = "0x0000"; // reset PC 
            }
            else if (lbOutput.SelectedIndex < lbOutput.Items.Count - 1) {
                // move LB selection to next instruction 
                for (int i = 0; i < instructions.Count; i++) {
                    if (instructions[i].address == PCAddr) {
                        lbOutput.SelectedIndex = i;
                        break;
                    }
                }
                Step();
            }
        }

        private void ResetMemoryTouchedTB() {
            tbMemTouched.Text = "n/a";
            tbPrevValue.Text = "n/a";
            tbNewValue.Text = "n/a";
        }
    }
}
