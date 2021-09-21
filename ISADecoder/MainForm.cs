﻿using System;
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
        public MainForm() {
            InitializeComponent();
            // build register array
            int rCount = 0;
            foreach (TextBox t in this.Controls.OfType<TextBox>()) {
                if (t.Name.StartsWith("tbR")) {
                    registers[rCount] = t;
                    rCount++;
                } 
            }
            
            
            
        }

        private void btnDecode_Click(object sender, EventArgs e) {
            lbOutput.Items.Clear();
            try {
                Decoder d = new Decoder(tbInput.Text);

                foreach (Instruction i in d.instructions) {
                    lbOutput.Items.Add(i.ToString());
                }

                CalculateStats(d.instructions);
            }
            catch (Exception ex) {
                tbInput.Text = ex.Message;
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
    }
}
