using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IPT_Lab5_233985
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Invalid args count.");
                Console.ReadKey();
                return;
            }
            //CompressFile(args[0]);
            DecompressFile(args[0]);
            Console.WriteLine("Done. Press any key to continue...");
            Console.ReadKey();
        }

        private static void CompressFile(string path)
        {
            string text = File.ReadAllText(path);
            string dict_path = System.IO.Path.ChangeExtension(path, "dct");
            string output = System.IO.Path.ChangeExtension(path, "dc");
            GenerateDict(text, path);
            string[] words = Regex.Split(text, @"(?= |\r\n)");
            Dictionary<string, byte[]> dict = GenerateCodes(Regex.Split(File.ReadAllText(dict_path/*, System.Text.Encoding.GetEncoding("ISO-8859-1")*/), @"@@@"));
            using (BinaryWriter binaryWriter = new BinaryWriter(File.OpenWrite(output)/*, System.Text.Encoding.GetEncoding("ISO-8859-1")*/))
            {
                foreach (string word in words)
                {
                    binaryWriter.Write(dict[word]);
                }
            }
        }

        private static void DecompressFile(string path)
        {
            //string text = File.ReadAllText(path);
            string dict_path = System.IO.Path.ChangeExtension(path, "dct");
            string[] dict_text = Regex.Split(File.ReadAllText(dict_path/*, System.Text.Encoding.GetEncoding("ISO-8859-1")*/), @"@@@");
            string output = System.IO.Path.ChangeExtension(path, null) + "_decompressed";
            Dictionary<string, byte[]> codes = GenerateCodes(dict_text);
            List<byte> arrBuffer = new List<byte>();
            byte buffer;
            using (StreamWriter streamWriter = new StreamWriter(File.OpenWrite(output)/*, System.Text.Encoding.GetEncoding("ISO-8859-1")*/))
            {
                using (BinaryReader binaryReader = new BinaryReader(File.OpenRead(path)))
                {
                    while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
                    {
                        buffer = binaryReader.ReadByte();
                        //Console.WriteLine(Convert.ToString(buffer, 2).PadLeft(8, '0'));
                        arrBuffer.Append(buffer);
                        if ((buffer & (128)) != 128)
                        {
                            arrBuffer.Add(buffer);
                        }
                        else
                        {
                            arrBuffer.Add(buffer);
                            byte[] byteArr;
                            byteArr = arrBuffer.ToArray();
                            arrBuffer.Clear();
                            string key = codes.SingleOrDefault(x => x.Value.SequenceEqual(byteArr)).Key;
                            streamWriter.Write(key);
                            streamWriter.Flush();
                        }
                    }
                    binaryReader.Close();
                }
                streamWriter.Close();
            }
        }

        private static void GenerateDict(string text, string path)
        {
            //string text = File.ReadAllText(path);
            Dictionary<string, int> occurs = new Dictionary<string, int>();
            string[] words = Regex.Split(text, @"(?= |\r\n)");//text.Split();
            foreach (string word in words)
            {
                occurs.TryGetValue(word, out var count);
                occurs[word] = count + 1;
            }

            string path_out = System.IO.Path.ChangeExtension(path, "dct");
            using (FileStream fs = File.OpenWrite(path_out))
            {
                using (StreamWriter sw = new StreamWriter(fs/*, System.Text.Encoding.GetEncoding("ISO-8859-1")*/))
                {
                    bool initial = true;
                    foreach (KeyValuePair<string, int> keyValuePair in occurs.OrderByDescending(x => x.Value))
                    {
                        if (initial)
                            initial = false;
                        else
                            sw.Write("@@@");
                        sw.Write(keyValuePair.Key);
                    }
                    sw.Close();
                }
                fs.Close();
            }
        }


        private static Dictionary<string, byte[]> GenerateCodes(string[] text)
        {
            Dictionary<string, byte[]> codes = new Dictionary<string, byte[]>();
            int counter = 0;

            foreach (string word in text)
            {
                if (counter < 128)
                {
                    byte[] arr = new byte[1];
                    arr[0] = 1 << 7;
                    arr[0] |= (byte)counter;
                    codes[word] = arr;
                }
                else
                {
                    int exponent = 2;
                    int targetArrSize = 1;
                    bool carryOverFlag = true;
                    while (carryOverFlag)
                    {
                        int range = (int)Math.Pow(2, 7 * exponent) + 128;
                        if (counter < range)
                            carryOverFlag = false;
                        targetArrSize++;
                        exponent++;
                    }
                    //Console.WriteLine(targetArrSize);
                    byte[] arr = new byte[targetArrSize];
                    int remainder = counter;
                    arr[targetArrSize - 1] = (byte)((remainder % 128) + 128);
                    remainder /= 128;
                    int i = targetArrSize - 2;
                    while (remainder > 0)
                    {
                        remainder--;
                        arr[i] = (byte)(remainder % 128);
                        remainder /= 128;
                        i--;
                    }
                    codes[word] = arr;
                }
                counter++;
            }
            return codes;
        }

    }
}
