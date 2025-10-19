using System;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;

namespace InstallerStudio.Providers
{
    public static partial class SetupProvider
    {
        private static readonly byte[] _signature1 = [0xDE, 0xAD, 0xBE, 0xEF];
        private static readonly byte[] _signature2 = [0xDE, 0xAD, 0xBE, 0xED];

        public static bool IsNullsoft(string filePath)
        {
            // https://github.com/microsoft/winget-pkgs/blob/master/Tools/Modules/YamlCreate/YamlCreate.InstallerDetection/YamlCreate.InstallerDetection.psm1#L273
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var reader = new PEReader(fileStream);

            if (!reader.PEHeaders.IsExe)
            {
                return false;
            }

            var section = reader.PEHeaders.SectionHeaders.First(x => x.Name == ".rsrc");
            var offset = section.PointerToRawData + section.SizeOfRawData;

            using var binaryReader = new BinaryReader(fileStream);

            // Read 8 bytes after the offset.
            fileStream.Seek(offset, SeekOrigin.Begin);
            var bytes = binaryReader.ReadBytes(8);

            if (bytes.Length > 0)
            {
                // Read the nullsoft header bytes.
                // https://sourceforge.net/p/nsis/code/HEAD/tree/NSIS/branches/WIN64/Source/exehead/fileform.h#l222
                var presumedBytes = bytes.Skip(4).Take(4).Reverse().ToArray();
                return presumedBytes.SequenceEqual(_signature1) || presumedBytes.SequenceEqual(_signature2);
            }

            return false;
        }
    }
}
