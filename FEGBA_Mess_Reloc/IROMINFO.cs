using System;

namespace FEGBA_Mess_Reloc
{
    public interface IROMINFO
    {
        String GameID();
        uint MessagePointer(); // Position of String table
        uint MessageNo(); // Number of Huffman coded strings
        uint MessageStringLoc(); // Position of String #0
        uint MessageStringSpaceSize(); // Usually pos of HuffmanLoc - MessageStringLoc
        uint HuffmanLoc(); // Beginning position of Huffman Table
        uint HuffmanRoot(); // Pointer to the root node of Huffman Tree structure
        uint AntiHuffmanFuncLoc(); // Position where anti-huffman patch is applied
        uint AntiHuffmanCheck(); // Four-byte check data if anti-huffman is applied

    }
}
