namespace FEGBA_Mess_Reloc
{
    sealed class FE8J : IROMINFO
    {
        public string GameID() { return "BE8J"; }
        public uint MessagePointer() { return 0x14d08c; }
        public uint MessageNo() { return 0xd0a; }
        public uint MessageStringLoc() { return 0xed7f4; }
        public uint MessageStringSpaceSize() { return HuffmanLoc() - MessageStringLoc(); }
        public uint HuffmanLoc() { return 0x14929c; }
        public uint HuffmanRoot() { return 0x14d088; }
        public uint AntiHuffmanFuncLoc() { return 0x2af4; }
        public uint AntiHuffmanCheck() { return 0x0fc2b500; }
    }
}
