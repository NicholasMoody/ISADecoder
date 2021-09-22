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
        int[] memory = new int[1048576]; // stores memory of program for simulation 

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
            int pc = 0; 
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
        private void Step(int address) {
            Instruction inst = new Instruction(); 

            foreach (Instruction i in instructions) {
                if (address == i.address) {
                    inst = i;
                    break;
                }
            }

            switch (inst.mnemonic) {
                case Mnemonic.LD:
                    registers[inst.r1].Text = "0x" + memory[inst.op1].ToString("X4"); // load value in memory address to register
                    break;
                case Mnemonic.ST:
                    memory[inst.op1] = Convert.ToInt32(registers[inst.r1].Text, 16); // store value from register in memory address 
                    break;
                case Mnemonic.MOV:
                    // moves operand into register depending on addressing mode 
                    if (inst.addressingMode == AddressingMode.Immediate) {
                        registers[inst.r1].Text = "0x" + inst.op1.ToString("X4");
                    }
                    else if (inst.addressingMode == AddressingMode.MemLoc) {
                        registers[inst.r1].Text = "0x" + memory[inst.op1].ToString("X4");
                    }
                    else if (inst.addressingMode == AddressingMode.SecondRegister) {
                        registers[inst.r1].Text = registers[inst.op1].Text;
                    }
                    break;
                case Mnemonic.COM:
                    break;
                case Mnemonic.B:
                    break;
                case Mnemonic.BL:
                    break;
                case Mnemonic.BLE:
                    break;
                case Mnemonic.BG:
                    break;
                case Mnemonic.BGE:
                    break;
                case Mnemonic.BE:
                    break;
                case Mnemonic.BNE:
                    break;
                case Mnemonic.STOP:
                    break;
                case Mnemonic.ADD:
                    break;
                case Mnemonic.SUB:
                    break;
                case Mnemonic.ASL:
                    break;
                case Mnemonic.LSR:
                    break;
                case Mnemonic.ASR:
                    break;
                case Mnemonic.LSL:
                    break;
                case Mnemonic.MULT:
                    break;
            }
        }

        private void btnRun_Click(object sender, EventArgs e) {
            lbOutput.SelectedIndex = 0;
            lbOutput.Enabled = false;
            btnStep.Enabled = true;
            Step(0);
            btnDecode.Enabled = false;
        }

        private void btnStep_Click(object sender, EventArgs e) {
            if (lbOutput.SelectedIndex < lbOutput.Items.Count - 1) {
                lbOutput.SelectedIndex++;
                Step(instructions[lbOutput.SelectedIndex].address);
            }
        }
    }
}
