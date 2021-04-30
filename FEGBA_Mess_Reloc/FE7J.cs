namespace FEGBA_Mess_Reloc
{
    sealed class FE7J : IROMINFO
    {
        public string GameID() { return "AE7J"; }
        public uint MessagePointer() { return 0xbbb370; }
        public uint MessageNo() { return 0x1234; }
        public uint MessageStringLoc() { return 0xb36950; }
        public uint MessageStringSpaceSize() { return HuffmanLoc() - MessageStringLoc(); }
        public uint HuffmanLoc() { return 0xbb72e0; }
        public uint HuffmanRoot() { return 0xbbb36c; }
        public uint AntiHuffmanFuncLoc() { return 0x13324; }
        public uint AntiHuffmanCheck() { return 0x1c284902; }
    }
}
