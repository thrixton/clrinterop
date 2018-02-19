using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting;
using System.Windows.Forms;

namespace TlbImpRegressionTestTool
{
    class TestCaseRunner
    {
        private TestCase m_testCase;
        private TestCaseSettings m_settings;
        private string m_outputDirectory;
        private int m_id;

        public TestCaseRunner(TestCase testCase, TestCaseSettings settings, int id)
        {
            m_testCase = testCase;
            m_settings = settings;
            m_id = id;

            FileInfo file = new FileInfo(testCase.TestedTarget);
            m_outputDirectory = file.DirectoryName;
        }

        internal void Run(bool baseline)
        {
            m_testCase.TestCaseStatus = TestCaseStatus.Running;

            string targetFileName = Path.GetFileName(m_testCase.TestedTarget);

            string outputFileName = m_id + targetFileName + ".dll.bsl"; 
            bool success = CreateResult(m_settings.TestedCommand, m_testCase.TestedTarget,
                m_testCase.WorkingDirectory, m_testCase.Arguments, outputFileName);

            if (!success)
            {
                m_testCase.TestCaseStatus = TestCaseStatus.Error;
                return;
            }

            string outputFilePathName = Path.Combine(m_outputDirectory, outputFileName);
            if (FileCompareIdentical(m_testCase.Baseline, outputFilePathName))
            {
                m_testCase.TestCaseStatus = TestCaseStatus.Succeed;
            }
            else
            {
                if (baseline)
                {
                    File.Copy(outputFilePathName, m_testCase.Baseline, overwrite:true);
                    m_testCase.TestCaseStatus = TestCaseStatus.Baselined;
                }
                else
                { 
                    m_testCase.TestCaseStatus = TestCaseStatus.Failed;
                }
            }
        }

        private void DeleteOutputFiles(string fileName)
        {
            try
            {
                FileInfo file = new FileInfo(fileName);
                if (file.Exists)
                    file.Delete();
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Can not delete file {0}.", fileName));
                throw new Exception("Can not delete file");
            }
        }

        private bool FileCompareIdentical(string fileFullPath1, string fileFullPath2)
        {
            try
            {
                FileStream file1 = new FileStream(fileFullPath1, FileMode.Open, FileAccess.Read);
                FileStream file2 = new FileStream(fileFullPath2, FileMode.Open, FileAccess.Read);
                if (file1.Length != file2.Length)
                {
                    file1.Close();
                    file2.Close();
                    return false;
                }
                int byte1;
                int byte2;
                do
                {
                    byte1 = file1.ReadByte();
                    byte2 = file2.ReadByte();
                } while (byte1 == byte2 && byte1 != -1);
                file1.Close();
                file2.Close();
                return (byte1 == byte2);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool CreateResult(string command, string target, string workingDirectory,
            string arguments, string outFileName)
        {
            try
            {
                string targetFileName = Path.GetFileName(target);
                DeleteOutputFiles(Path.Combine(m_outputDirectory ,targetFileName + ".dll"));
                Process p = new Process();
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.WorkingDirectory = workingDirectory;
                p.StartInfo.FileName = command;
                p.StartInfo.Arguments = "\"" + target + "\" " + arguments + " /out:\"" +
                    Path.Combine(m_outputDirectory, targetFileName + ".dll") + "\"";
                p.Start();
                p.WaitForExit();
                if (!FileExist(Path.Combine(m_outputDirectory, targetFileName + ".dll")))
                    return false;

                DeleteOutputFiles(Path.Combine(m_outputDirectory, targetFileName + ".dll.txt"));

                PrinterAssemblyInPlace(Path.Combine(m_outputDirectory, targetFileName + ".dll"), workingDirectory);
                

                FileInfo assemPrinterOutFile = new FileInfo(
                    Path.Combine(m_outputDirectory, targetFileName + ".dll" + ".txt"));
                if (!assemPrinterOutFile.Exists)
                {
                    return false;
                }

                string outputFileFullPath = Path.Combine(m_outputDirectory, outFileName);
                assemPrinterOutFile.CopyTo(outputFileFullPath, true);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void PrinterAssemblyInPlace(string assemblyFileFullPath, string workingDirectory)
        {
            // Create an AppDomain to load the assembly printer.
            AppDomainSetup options = new AppDomainSetup();
            options.ApplicationBase = workingDirectory;
            AppDomain domain = AppDomain.CreateDomain("AssemPrinter", null, options);
            if (domain == null)
            {
                MessageBox.Show("Cannot Create AppDomain.");
                throw new Exception("Cannot Create AppDomain.");
            }

            // Create the remote component that will printer the assembly.
            ObjectHandle h = domain.CreateInstanceFrom(typeof(TlbImpRegressionTestTool.RemoteAssemPrinter).Assembly.CodeBase,
                                                       "TlbImpRegressionTestTool.RemoteAssemPrinter");
            if (h == null)
            {
                MessageBox.Show("Failed in Calling Assembly Printer.");
                throw new Exception("Failed in Calling Assembly Printer.");
            }

            RemoteAssemPrinter code = (RemoteAssemPrinter)h.Unwrap();

            if (code != null)
                code.Run(assemblyFileFullPath);

            // Unload the app domain. Now the assembly is unloaded, too.
            AppDomain.Unload(domain);
        }

        private bool FileExist(string path)
        {
            FileInfo file = new FileInfo(path);
            return file.Exists;
        }
    }
}
