using System;
using System.Collections.Generic;
using System.Text;

namespace TlbImpRegressionTestTool
{
    public class RemoteAssemPrinter : MarshalByRefObject
    {
        public int Run(String assemblyFileFullPath)
        {
            return AssemPrinter.PrinterAssemblyInPlace(assemblyFileFullPath);
        }
    }

}
