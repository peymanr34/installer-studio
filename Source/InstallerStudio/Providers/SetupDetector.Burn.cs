using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;

namespace InstallerStudio.Providers
{
    public static partial class SetupDetector
    {
        public static bool IsBurn(string filePath)
        {
            // https://github.com/microsoft/winget-pkgs/blob/master/Tools/Modules/YamlCreate/YamlCreate.InstallerDetection/YamlCreate.InstallerDetection.psm1#L332
            // https://github.com/wixtoolset/wix/blob/main/src/burn/engine/inc/engine.h#L8
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var reader = new PEReader(fileStream);

            return reader.PEHeaders.IsExe && reader.PEHeaders.SectionHeaders.Any(x => x.Name == ".wixburn");
        }
    }
}
