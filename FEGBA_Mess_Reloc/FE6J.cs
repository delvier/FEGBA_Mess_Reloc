namespace FEGBA_Mess_Reloc
{
    sealed class FE6J : IROMINFO
    {
        public string GameID() { return "AFEJ"; }
        public uint MessagePointer2() { return 0x13B10; }
        public uint MessagePointer() { return 0xf635c; }
        public uint MessageNo() { return 0xd0d; }
        public uint MessageStringLoc() { return 0x9fac0; }
        public uint MessageStringSpaceSize() { return HuffmanLoc() - MessageStringLoc(); }
        public uint HuffmanLocPointer() { return 0x6E0; }
        public uint HuffmanLoc() { return 0xf300c; }
        public uint HuffmanRootPointer() { return 0x6DC; }
        public uint HuffmanRoot() { return 0xf6358; }
        public uint AntiHuffmanFuncLoc() { return 0x384c; }
        public uint AntiHuffmanCheck() { return 0xb002b503; }
    }
}
