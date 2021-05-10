namespace FEGBA_Mess_Reloc
{
    sealed class FE7U : IROMINFO
    {
        public string GameID() { return "AE7E"; }
        public uint MessagePointer2() { return 0x12CB8; }
        public uint MessagePointer() { return 0xb808ac; }
        public uint MessageNo() { return 0x133d; }
        public uint MessageStringLoc() { return 0xaeae8c; }
        public uint MessageStringSpaceSize() { return HuffmanLoc() - MessageStringLoc(); }
        public uint HuffmanLocPointer() { return 0x6BC; }
        public uint HuffmanLoc() { return 0xb7d71c; }
        public uint HuffmanRootPointer() { return 0x6B8; }
        public uint HuffmanRoot() { return 0xb808a8; }
        public uint AntiHuffmanFuncLoc() { return 0x12c6c; }
        public uint AntiHuffmanCheck() { return 0x1c284902; }
    }
}
