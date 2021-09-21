using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISADecoder {
    enum Mnemonic {
        LD,
        ST,
        MOV,
        COM,
        B,
        BL,
        BLE,
        BG,
        BGE,
        BE,
        BNE,
        STOP,
        ADD,
        SUB,
        ASL,
        LSR,
        ASR,
        LSL,
        MULT
    }
}
