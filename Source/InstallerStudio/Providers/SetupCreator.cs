using System;
using InstallerStudio.Data.Models;
using InstallerStudio.Models;

namespace InstallerStudio.Providers
{
    public static partial class SetupCreator
    {
        public static string CreateScript(Project project, CompilerType compilerType) => compilerType switch
        {
            CompilerType.InnoSetup => CreateInnoSetupScript(project),
            CompilerType.Nullsoft => CreateNullsoftScript(project),
            _ => throw new NotImplementedException(),
        };
    }
}
