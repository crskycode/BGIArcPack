using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGIArcPacker
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: path/to/folder path/to/arc");
                Console.ReadKey();
                return;
            }

            ArcPacker packer = new ArcPacker();
            packer.Pack(args[0], args[1]);
        }

        class ArcPacker
        {
            struct Entry
            {
                public string path;
                public string name;
                public uint size;
            }

            public void Pack(string rootFolder, string arcFileName)
            {
                if (rootFolder.EndsWith("\\"))
                    rootFolder += "\\";

                FileStream arcFile = File.OpenWrite(arcFileName);
                BinaryWriter writer = new BinaryWriter(arcFile);

                writer.Write(Encoding.ASCII.GetBytes("BURIKO ARC20"));

                List<Entry> entries = new List<Entry>();

                foreach (string fileName in Directory.EnumerateFiles(rootFolder))
                {
                    string name = Path.GetFileName(fileName);

                    if (name.Length >= 0x60)
                    {
                        Console.WriteLine("File name \"{0}\" too long, ignored", name);
                        continue;
                    }

                    FileInfo fileInfo = new FileInfo(fileName);

                    if (fileInfo.Length == 0)
                    {
                        Console.WriteLine("File \"{0}\" length is zero, ignored", name);
                        continue;
                    }

                    Entry entry = new Entry();
                    entry.path = fileName;
                    entry.name = name;
                    entry.size = (uint)fileInfo.Length;

                    entries.Add(entry);
                }

                writer.Write((uint)entries.Count);

                uint curOffset = 0;

                foreach (Entry entry in entries)
                {
                    byte[] nameBytes = Encoding.GetEncoding(936).GetBytes(entry.name);
                    writer.Write(nameBytes);

                    uint nameBytesPadding = 0x60 - (uint)nameBytes.Length;
                    if (nameBytesPadding > 0)
                    {
                        byte[] paddingBytes = new byte[nameBytesPadding];
                        writer.Write(paddingBytes);
                    }

                    writer.Write(curOffset);
                    writer.Write(entry.size);
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(0);

                    curOffset += entry.size;
                }

                foreach (Entry entry in entries)
                {
                    Console.WriteLine("Adding {0} ...", entry.name);

                    byte[] fileData = File.ReadAllBytes(entry.path);
                    writer.Write(fileData);
                }

                writer.Close();
            }
        }
    }
}
