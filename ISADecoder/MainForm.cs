using System;
using System.Collections.Generic;
<<<<<<< HEAD
using System.IO;
=======
using System.Drawing;
>>>>>>> f73675241c1087692c95fec7c3211eb63063f99d
using System.Linq;
using System.Windows.Forms;

namespace ISADecoder {
    public partial class MainForm : Form {
        TextBox[] registers = new TextBox[16];
        // negative values do wierd shit and do not convert to hex conveniently 
        // thus, i should stop being stupid and actually keep the values of registers as shorts, not just as fucking text lmao 
        short[] registerVals = new short[16];
        List<Instruction> instructions = new List<Instruction>();
        int memSize = 1048576;
        byte[] memory; // stores memory of program for simulation 
        bool done = false;
        short PCAddr = 0; // internal PC count so that register text can easily be updated at right time 
        int PCreg = 14;
        int FlagsReg = 15;

        public MainForm() {
            InitializeComponent();
            // build register array
            int rCount = 0;

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
            // reset things 
            lbOutput.Items.Clear();
            tbInstructionDescription.Text = "";
            registerVals = new short[16];
            foreach (TextBox tb in registers) {
                tb.Text = "0x0000";
            }
            PCAddr = 0;
            memory = new byte[memSize];
            short pc = 0; 

            try {
                Decoder d = new Decoder(tbInput.Text, memory);

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

            foreach (Instruction i in instructions) {
                if (PCAddr == i.address) {
                    inst = i;
                    break;
                }
            }
            //reset register colors
            foreach (TextBox tb in registers) {
                tb.BackColor = SystemColors.Control;
            }

            registers[PCreg].Text = ToHexString(PCAddr); // update PC 
            registerVals[PCreg] = PCAddr;

            PCAddr += inst.instSize; // update PC for next instruction. will be overwritten if branch. 

            short registerValue = 0;
            if (inst.r1 >= 0) 
                registerValue = registerVals[inst.r1]; // value stored in current instruction's register

            short operandVal = GetOperandValByAddressingMode(inst); // may not be used for given instruction, doesn't matter
            switch (inst.mnemonic) {
                case Mnemonic.LD:
                    registerVals[inst.r1] = operandVal;
                    ViewMemoryAt(inst.op1);
                    break;
                case Mnemonic.ST:
                    memory[operandVal] = (byte)(registerValue >> 8); // most significant byte of value
                    memory[operandVal + 1] = (byte)(registerValue & 0b11111111); // least significant byte of value
                    ViewMemoryAt(operandVal);
                    break;
                case Mnemonic.MOV:
                    // moves operand into register depending on addressing mode 
                    registerVals[inst.r1] = operandVal;
                    break;
                case Mnemonic.COM:
                    // subtracts operand value from register value and sets flags
                    int comp = registerVals[inst.r1] - operandVal;
                    short flag = 0;
                    if (comp == 0) {  
                        flag |= 0b100; // sets zero flag
                        flag &= 0b0111; // unsets negative flag
                    }
                    else if (comp < 0) {
                        flag |= 0b1000; // sets negative flag
                        flag &= 0b1011; // unset zero flag
                    }
                    registerVals[FlagsReg] = flag;
                    registers[FlagsReg].Text = ToHexString(flag);
                    registers[FlagsReg].BackColor = Color.LightGreen;
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
                    registerVals[inst.r1] = (short)(registerValue + operandVal);
                    break;
                case Mnemonic.SUB:
                    registerVals[inst.r1] = (short)(registerValue - operandVal);
                    break;
                // NOTE: all arithmetic and logical shifts do the same thing here
                // either change how ints are handled or remove one type of shift (easier)
                case Mnemonic.ASL:
                    registerVals[inst.r1] = (short)(registerValue << operandVal);
                    break;
                case Mnemonic.LSR:
                    registerVals[inst.r1] = (short)(registerValue >> operandVal);
                    break;
                case Mnemonic.ASR:
                    registerVals[inst.r1] = (short)(registerValue >> operandVal);
                    break;
                case Mnemonic.LSL:
                    registerVals[inst.r1] = (short)(registerValue << operandVal);
                    break;
                case Mnemonic.MULT:
                    registerVals[inst.r1] = (short)(operandVal * registerValue);
                    break;
            }
            if (inst.r1 >= 0) {
                registers[inst.r1].Text = ToHexString(registerVals[inst.r1]);
                registers[inst.r1].BackColor = Color.LightGreen;
            }
        }


        // fill textbox with memory at address including its 10 neighbors (words, not bytes) 
        // ADDRESS SHOULD BE EVEN, OTHERWISE WEIRD SHIT WILL HAPPEN
        // I AM WARNING YOU 
        private void ViewMemoryAt(short address) {
            tbMemViewer.Text = "";
            for (short i = -12; i <= 12; i += 2) {
                if (address + i >= 0 && address + i < memory.Length) {
                    tbMemViewer.Text += ToHexString((short)(address + i)) + " | " + ToHexFromMemory(address + i) + Environment.NewLine;
                }
            }
        }

        private short GetOperandValByAddressingMode(Instruction inst) {
            short val = 0;
            if (inst.addressingMode == AddressingMode.Immediate)
                val = inst.op1;
            else if (inst.addressingMode == AddressingMode.MemLoc)
                val = memory[inst.op1];
            else if (inst.addressingMode == AddressingMode.SecondRegister)
                val = HexToInt(registers[inst.r2].Text);
            return val;
        }

        private string ToHexString(short val) {
            return "0x" + val.ToString("X4");
        }

        private string ToHexFromMemory(int address) {
            if (address < 0 || address >= memory.Length)
                return "n/a";
            // last byte, can't form word 
            if (address == memory.Length - 1) 
                return $"0x{memory[address]:X2}";

            return $"0x{memory[address]:X2}{memory[address + 1]:X2}";
        }

        private bool NFlagSet() {
            int flag = HexToInt(registers[FlagsReg].Text) >> 3;
            return flag % 2 == 1;
        }

        private bool ZFlagSet() {
            int flag = HexToInt(registers[FlagsReg].Text) >> 2;
            return flag % 2 == 1;
        }

        private short HexToInt(string hex) {
            return Convert.ToInt16(hex, 16);
        }

        private void btnRun_Click(object sender, EventArgs e) {
            lbOutput.SelectedIndex = 0;
            lbOutput.Enabled = false;
            btnStep.Enabled = true;
            btnRun.Enabled = false;
            btnRunToEnd.Enabled = true;
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
                registers[PCreg].Text = "0x0000"; // reset PC 
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

        private void btnViewMem_Click(object sender, EventArgs e) {
            try {
                short address = HexToInt(tbMemSelection.Text);
                ViewMemoryAt(address);
            }
            catch (Exception) {
                MessageBox.Show("Invalid memory address. Input should be four hex characters.", "Invalid Input", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRunToEnd_Click(object sender, EventArgs e) {
            while (!done) {
                Step();
            }
            Reset();
        }

        private void Reset() {
            lbOutput.SelectedIndex = 0;
            lbOutput.Enabled = true;
            btnStep.Enabled = false;
            btnRun.Enabled = true;
            btnDecode.Enabled = true;
            registers[PCreg].Text = "0x0000"; // reset PC 
            btnRunToEnd.Enabled = false;
        }

        //GNN - Added file directory to open files.
        private void button1_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;

                    //Read the contents of the file into a stream
                    var fileStream = openFileDialog.OpenFile();

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                }
            }

            MessageBox.Show(fileContent, "File Content at path: " + filePath, MessageBoxButtons.OK);
            tbInput.Text = fileContent;
        }
    }
}
