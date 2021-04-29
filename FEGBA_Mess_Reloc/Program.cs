using System;
using System.IO;
using System.Linq;
using System.Text;

namespace FEGBA_Mess_Reloc
{

    public interface ROMINFO
    {
        String GameID();
        uint MessagePointer(); // Position of String table
        uint MessageNo(); // Number of Huffman coded strings
        uint MessageStringLoc(); // Position of String #0
        uint MessageStringSpaceSize(); // Usually pos of HuffmanLoc 0 MessageStringLoc
        uint HuffmanLoc(); // Beginning position of Huffman Table
        uint HuffmanRoot(); // Pointer to the root node of Huffman Tree structure
        uint AntiHuffmanFuncLoc(); // Position where anti-huffman patch is applied
        uint AntiHuffmanCheck(); // Four-byte check data if anti-huffman is applied

    }
    sealed class FE6J : ROMINFO
    {
        public string GameID() { return "AFEJ"; }
        public uint MessagePointer() { return 0xf635c; }
        public uint MessageNo() { return 0xd0d; }
        public uint MessageStringLoc() { return 0x9fac0; }
        public uint MessageStringSpaceSize() { return 0xf300c - 0x9fac0; }
        public uint HuffmanLoc() { return 0xf300c; }
        public uint HuffmanRoot() { return 0xf6358; }
        public uint AntiHuffmanFuncLoc() { return 0x384c; }
        public uint AntiHuffmanCheck() { return 0xb002b503; }
    }
    sealed class FE7J : ROMINFO
    {
        public string GameID() { return "AE7J"; }
        public uint MessagePointer() { return 0xbbb370; }
        public uint MessageNo() { return 0x1234; }
        public uint MessageStringLoc() { return 0xb36950; }
        public uint MessageStringSpaceSize() { return 0xbb72e0 - 0xb36950; }
        public uint HuffmanLoc() { return 0xbb72e0; }
        public uint HuffmanRoot() { return 0xbbb36c; }
        public uint AntiHuffmanFuncLoc() { return 0x13324; }
        public uint AntiHuffmanCheck() { return 0x1c284902; }
    }
    sealed class FE7U : ROMINFO
    {
        public string GameID() { return "AE7E"; }
        public uint MessagePointer() { return 0xb808ac; }
        public uint MessageNo() { return 0x133d; }
        public uint MessageStringLoc() { return 0xaeae8c; }
        public uint MessageStringSpaceSize() { return 0xb7d71c - 0xaeae8c; }
        public uint HuffmanLoc() { return 0xb7d71c; }
        public uint HuffmanRoot() { return 0xb808a8; }
        public uint AntiHuffmanFuncLoc() { return 0x12c6c; }
        public uint AntiHuffmanCheck() { return 0x1c284902; }
    }
    sealed class FE8J : ROMINFO
    {
        public string GameID() { return "BE8J"; }
        public uint MessagePointer() { return 0x14d08c; }
        public uint MessageNo() { return 0xd0a; }
        public uint MessageStringLoc() { return 0xed7f4; }
        public uint MessageStringSpaceSize() { return 0x14929c - 0xed7f4; }
        public uint HuffmanLoc() { return 0x14929c; }
        public uint HuffmanRoot() { return 0x14d088; }
        public uint AntiHuffmanFuncLoc() { return 0x2af4; }
        public uint AntiHuffmanCheck() { return 0x0fc2b500; }
    }
    sealed class FE8U : ROMINFO
    {
        public string GameID() { return "BE8E"; }
        public uint MessagePointer() { return 0x15d48c; }
        public uint MessageNo() { return 0xd4b; }
        public uint MessageStringLoc() { return 0xe8414; }
        public uint MessageStringSpaceSize() { return 0x15a72c - 0xe8414; }
        public uint HuffmanLoc() { return 0x15a72c; }
        public uint HuffmanRoot() { return 0x15d488; }
        public uint AntiHuffmanFuncLoc() { return 0x2ba4; }
        public uint AntiHuffmanCheck() { return 0x0fc2b500; }
    }

    class Node
    {
        public Node left, right;
        public ushort value;
    }

    class Program
    {
        public static Node BuildBinTree(byte[] data, int loc, int val)
        {
            Node node = new();
            UInt16 leftVal = BitConverter.ToUInt16(data.Skip(loc + val * 4).Take(2).ToArray());
            UInt16 rightVal = BitConverter.ToUInt16(data.Skip(loc + val * 4 + 2).Take(2).ToArray());
            if (rightVal != 0xffff)
            {
                node.left = BuildBinTree(data, loc, leftVal);
                node.right = BuildBinTree(data, loc, rightVal);
            }
            else
            {
                node.left = null;
                node.right = null;
                node.value = leftVal;
            }
            return node;
        }
        public static Node RipHuffman(byte[] data, int loc, int root)
        {
            uint headLoc = BitConverter.ToUInt32(data.Skip(root).Take(4).ToArray()) - 0x8000000;
            Node head = new();
            UInt16 leftVal = BitConverter.ToUInt16(data.Skip((int)headLoc).Take(2).ToArray());
            UInt16 rightVal = BitConverter.ToUInt16(data.Skip((int)headLoc + 2).Take(2).ToArray());
            if (rightVal != 0xffff)
            {
                Console.WriteLine("Taking left...");
                head.left = BuildBinTree(data, loc, leftVal);
                Console.WriteLine("Taking right...");
                head.right = BuildBinTree(data, loc, rightVal);
            }
            else
            {
                head.value = leftVal;
            }
            return head;
        }
        public static byte[] GetMessageBytes(byte[] data, int loc, Node huff)
        {
            bool terminal = false;
            bool[] huffChar = Array.Empty<bool>();
            byte[] output = Array.Empty<byte>();
            uint i = 0;
            Node seeking = huff;
            while (!terminal)
            {
                Array.Resize(ref output, output.Length + 1);
                output[^1] = data[loc + (int)i];
                byte d = data[loc + (int)i];
                for (int b = 0; b < 8; b++)
                {
                    Array.Resize(ref huffChar, huffChar.Length + 1);
                    if (d % 2 == 0)
                    {
                        seeking = seeking.left;
                        huffChar[^1] = false;
                    }
                    else
                    {
                        seeking = seeking.right;
                        huffChar[^1] = true;
                    }
                    if (seeking.right == null)
                    {
                        if (seeking.value == 0)
                        {
                            terminal = true;
                            break;
                        }
                        huffChar = Array.Empty<bool>();
                        seeking = huff;
                    }
                    d = (byte)(d / 2);
                }
                i++;
            }
            return output;
        }
        static void Main(string[] args)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            string path;
            try
            {
                path = args[0].Trim('"');
            }
            catch (Exception e)
            {
                Console.WriteLine("Write the path of file you want to modify: ");
                path = Console.ReadLine().Trim('"');
            }
            try
            {
                if (File.Exists(path))
                {
                    int typeoffset = 0xac;
                    string ext = Path.GetExtension(path);
                    string filename = Path.GetFileNameWithoutExtension(path);
                    byte[] filedata = File.ReadAllBytes(path);
                    string romtype = Encoding.Default.GetString(filedata.Skip(typeoffset).Take(4).ToArray());

                    ROMINFO rominfo = new FE6J();
                    if (romtype.Equals("AFEJ"))
                    {
                        rominfo = new FE6J();
                    }
                    else if (romtype.Equals("AE7J"))
                    {
                        rominfo = new FE7J();
                    }
                    else if (romtype.Equals("AE7E"))
                    {
                        rominfo = new FE7U();
                    }
                    else if (romtype.Equals("BE8J"))
                    {
                        rominfo = new FE8J();
                    }
                    else if (romtype.Equals("BE8U"))
                    {
                        rominfo = new FE8U();
                    }
                    else
                    {
                        Console.WriteLine($"The file {Path.GetFileName(path)} is currently unsupported, or not a Game Boy Advance Fire Emblem ROM.");
                        return;
                    }
                    uint antiHuff = BitConverter.ToUInt32(filedata.Skip((int)rominfo.AntiHuffmanFuncLoc()).Take(4).ToArray());
                    if (antiHuff == rominfo.AntiHuffmanCheck())
                    {
                        Console.WriteLine("ROMs that anti-huffman patch is applied are not supported.");
                        return;
                    }
                    uint[] oldPointer = new uint[rominfo.MessageNo() + 1];
                    byte[][] messages = new byte[rominfo.MessageNo() + 1][];
                    for (int i = 0; i <= rominfo.MessageNo(); i++)
                    {
                        oldPointer[i] = BitConverter.ToUInt32(filedata.Skip((int)rominfo.MessagePointer() + 4 * i).Take(4).ToArray());
                    }
                    Console.WriteLine("Constructing Huffman Table...");
                    Node huff = RipHuffman(filedata, (int)rominfo.HuffmanLoc(), (int)rominfo.HuffmanRoot());
                    Console.WriteLine("Extracting messages...");
                    int len = 0;
                    for (int i = 0; i <= rominfo.MessageNo(); i++)
                    {
                        Console.Write($"Reading message {i} of {rominfo.MessageNo()}: ");
                        messages[i] = GetMessageBytes(filedata, (int)oldPointer[i] - 0x8000000, huff);
                        Console.WriteLine($"Size: {messages[i].Length}");
                        len += messages[i].Length;
                    }
                    Console.Write("Cleaning space...");
                    for (int i = 0; i <= rominfo.MessageNo(); i++)
                    {
                        for (int ii = 0; ii < messages[i].Length; ii++)
                        {
                            filedata[oldPointer[i] + ii - 0x8000000] = 0;
                        }
                    }
                    if (len > (int)rominfo.MessageStringSpaceSize())
                    {
                        Console.WriteLine($"Free space is not enough: {len} is bigger than {rominfo.MessageStringSpaceSize()}. Aborting.");
                        return;
                    }
                    Console.WriteLine("Relocating messages...");
                    uint[] newPointer = new uint[rominfo.MessageNo() + 1];
                    newPointer[0] = rominfo.MessageStringLoc() + 0x8000000;
                    for (int i = 0; i <= rominfo.MessageNo(); i++)
                    {
                        if (i > 0)
                        {
                            newPointer[i] = newPointer[i - 1] + (uint)messages[i - 1].Length;
                        }
                        for (int ii = 0; ii < messages[i].Length; ii++)
                        {
                            filedata[newPointer[i] + ii - 0x8000000] = messages[i][ii];
                        }
                        Array.Copy(BitConverter.GetBytes(newPointer[i]), 0, filedata, rominfo.MessagePointer() + i * 4, 4);
                        Console.WriteLine($"Wrote message {i} of {rominfo.MessageNo()} at {newPointer[i]:X}");
                    }
                    try
                    {
                        BinaryWriter bw = new(new FileStream($"{Path.GetDirectoryName(path)}/{Path.GetFileNameWithoutExtension(path)}_Rewrite{Path.GetExtension(path)}", FileMode.Create));
                        bw.Write(filedata);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("There is an output error.");
                    }
                }
                else
                {
                    Console.WriteLine("File does not exist.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("This is not a file.");
            }
        }
    }
}
