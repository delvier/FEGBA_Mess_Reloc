namespace FEGBA_Mess_Reloc
{
    sealed class FE8U : IROMINFO
    {
        public string GameID() { return "BE8E"; }
        public uint MessagePointer2() { return 0xA2A0; }
        public uint MessagePointer() { return 0x15d48c; }
        public uint MessageNo() { return 0xd4b; }
        public uint MessageStringLoc() { return 0xe8414; }
        public uint MessageStringSpaceSize() { return HuffmanLoc() - MessageStringLoc(); }
        public uint HuffmanLocPointer() { return 0x6E0; }
        public uint HuffmanLoc() { return 0x15a72c; }
        public uint HuffmanRootPointer() { return 0x6DC; }
        public uint HuffmanRoot() { return 0x15d488; }
        public uint AntiHuffmanFuncLoc() { return 0x2ba4; }
        public uint AntiHuffmanCheck() { return 0x0fc2b500; }
    }
}
