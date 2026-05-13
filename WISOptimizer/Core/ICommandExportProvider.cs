using System.Collections.Generic;

namespace WISOptimizer.Core
{
    public interface ICommandExportProvider
    {
        List<string> GetSelectedPowerShellCommands();
    }
}
