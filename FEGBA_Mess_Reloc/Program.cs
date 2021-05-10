using System;
using System.IO;
using System.Linq;
using System.Text;

namespace FEGBA_Mess_Reloc
{
    class Program
    {
        public static Node BuildBinTree(byte[] data, int loc, int val)
        {
            Node node = new();
            ushort leftVal = BitConverter.ToUInt16(data.Skip(loc + val * 4).Take(2).ToArray());
            ushort rightVal = BitConverter.ToUInt16(data.Skip(loc + val * 4 + 2).Take(2).ToArray());
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
            ushort leftVal = BitConverter.ToUInt16(data.Skip((int)headLoc).Take(2).ToArray());
            ushort rightVal = BitConverter.ToUInt16(data.Skip((int)headLoc + 2).Take(2).ToArray());
            if (rightVal != 0xffff)
            {
                head.left = BuildBinTree(data, loc, leftVal);
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
        public static void Reloc(string path)
        {
            if (File.Exists(path))
            {
                int typeoffset = 0xac;
                string ext = Path.GetExtension(path);
                string filename = Path.GetFileNameWithoutExtension(path);
                byte[] filedata = File.ReadAllBytes(path);
                string romtype = Encoding.Default.GetString(filedata.Skip(typeoffset).Take(4).ToArray());

                IROMINFO rominfo = new FE6J();
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
                    BinaryWriter bw = new(new FileStream($"{Path.GetDirectoryName(path)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(path)}_Rewrite{Path.GetExtension(path)}", FileMode.Create));
                    bw.Write(filedata);
                    bw.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e}");
                    Console.WriteLine("There is an output error.");
                }
            }
            else
            {
                Console.WriteLine("File does not exist.");
            }
        }
        static void Main(string[] args)
        {
            string path;
            try
            {
                path = args[0].Trim('"');
            }
            catch (Exception)
            {
                Console.WriteLine("Write the path of file you want to modify: ");
                path = Console.ReadLine().Trim('"');
            }
            try
            {
                Reloc(path);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
            }
        }
    }
}
