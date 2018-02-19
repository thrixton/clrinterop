using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace TlbImpRegressionTestTool
{
    public partial class MainForm : Form
    {
        private TestCaseSettings m_settings;

        private TestCaseSet m_testCaseSet;

        private Thread m_runTestCaseThread;

        public static readonly string OutputDirectory = "out";

        public static readonly string TargetColumnName = "Target";

        public static readonly string IdColumnName = "Id";

        public static readonly string WorkingDirectoryName = "WorkingDirectory";

        public static readonly string ArgumentsColumnName = "Arguments";

        public static readonly string BaselineColumnName = "Baseline";

        public static readonly string StatusColumnName = "Status";

        delegate void EnableRunMenuItemDelegate(bool enabled);
        private EnableRunMenuItemDelegate m_enableRunMenuItemDelegate;

        public MainForm()
        {
            InitializeComponent();
            m_settings = new TestCaseSettings();
            m_enableRunMenuItemDelegate =
                new EnableRunMenuItemDelegate(this.EnableRunMenuItem);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openTestCaseFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    m_testCaseSet = new TestCaseSet(openTestCaseFileDialog.FileName);
                    List<TestCase> testCaseList = m_testCaseSet.TestCaseList;
                    UpdateDataGridViewTestCases(testCaseList);
                }
                catch (Exception)
                {
                    MessageBox.Show("Bad test case file format.");
                }
            }
        }

        private void UpdateDataGridViewTestCases(List<TestCase> testCaseList)
        {
            dataGridViewTestCases.Rows.Clear();
            dataGridViewTestCases.Rows.Add(testCaseList.Count);
            for (int i = 0; i < testCaseList.Count; i++)
            {
                testCaseList[i].StatusChange +=
                    new TestCase.StatusChangeHandler(this.TestCaseStatusChangeHandler);
                dataGridViewTestCases[IdColumnName, i].Value = "" + (i + 1);
                dataGridViewTestCases[TargetColumnName, i].Value = testCaseList[i].TestedTarget;
                dataGridViewTestCases[WorkingDirectoryName, i].Value = testCaseList[i].WorkingDirectory;
                dataGridViewTestCases[ArgumentsColumnName, i].Value = testCaseList[i].Arguments;
                dataGridViewTestCases[BaselineColumnName, i].Value = testCaseList[i].Baseline;
                dataGridViewTestCases[StatusColumnName, i].Value = testCaseList[i].TestCaseStatus;
                dataGridViewTestCases.Rows[i].DefaultCellStyle.BackColor = GetDataGridRowColor(testCaseList[i].TestCaseStatus);
            }
        }

        private Color GetDataGridRowColor(TestCaseStatus testCaseStatus)
        {
            switch (testCaseStatus)
            {
                case TestCaseStatus.Untested:
                    return Color.LightGray;
                case TestCaseStatus.Running:
                    return Color.Yellow;
                case TestCaseStatus.Error:
                    return Color.Orange;
                case TestCaseStatus.Failed:
                    return Color.Red;
                case TestCaseStatus.Baselined:
                    return Color.Aqua;
                 case TestCaseStatus.Succeed:
                    return Color.LightGreen;
                default:
                    return Color.LightGray;
            }
        }

        private void TestCaseStatusChangeHandler(object sender, TestCaseStatusChangeEventArgs e)
        {
            TestCase testCase = e.Source;
            List<TestCase> testCaseList = m_testCaseSet.TestCaseList;
            int index = testCaseList.IndexOf(testCase);
            if (index != -1)
            {
                dataGridViewTestCases[StatusColumnName, index].Value = testCase.TestCaseStatus;
                dataGridViewTestCases.Rows[index].DefaultCellStyle.BackColor =
                    GetDataGridRowColor(testCase.TestCaseStatus);
            }
        }

        private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsForm form = new SettingsForm(m_settings);
            if (form.ShowDialog() == DialogResult.OK)
            {
                m_settings.TestedCommand = form.TestedCommandText;
                m_settings.WindiffPath = form.WindiffPathText;

                m_settings.SaveToConfigFile();
            }
        }

        private void toolStripMenuItemRunAllTestCases_Click(object sender, EventArgs e)
        {
            if (m_testCaseSet != null)
            {
                RunAllTestCases(m_testCaseSet, onlyRunSelected:false, baseline:false);
            }
        }

        delegate void ExecuteTestCaseDelegate(TestCase tc, int index);

        private void RunAllTestCases(TestCaseSet testCaseSet, bool onlyRunSelected, bool baseline) 
        {
            if (!CheckAllCommandFileExist())
                return;

            List<TestCase> testCaseList = testCaseSet.TestCaseList;
            //FileInfo file = new FileInfo(testCaseSet.TestCaseFilePath);
            //DirectoryInfo outputDir = new DirectoryInfo(
            //    Path.Combine(file.DirectoryName, OutputDirectory));
            //if (!outputDir.Exists)
            //    outputDir.Create();
            if (m_runTestCaseThread != null && m_runTestCaseThread.IsAlive)
            {
                MessageBox.Show("Stop running test case first.");
                return;
            }
            m_runTestCaseThread = new Thread(delegate() {
                this.Invoke(m_enableRunMenuItemDelegate, new object[] { false });
                try
                {
                    for (int i = 0; i < testCaseList.Count; i++)
                    {
                        if (!onlyRunSelected ||
                            dataGridViewTestCases.Rows[i].Cells[0].EditedFormattedValue.ToString() == "True")
                        {
                            TestCaseRunner runner = new TestCaseRunner(testCaseList[i], m_settings, i + 1);
                            runner.Run(baseline);
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                }
                finally
                {
                    ChangeRunningTestCases2Untested(testCaseList);
                    this.Invoke(m_enableRunMenuItemDelegate, new object[] { true });
                }
            });
            m_runTestCaseThread.IsBackground = true;
            m_runTestCaseThread.Start();
        }

        private bool CheckAllCommandFileExist()
        {
            try
            {
                FileInfo file;

                file = new FileInfo(m_settings.TestedCommand);
                if (!file.Exists)
                {
                    MessageBox.Show("Tested TlbImp does not exist.");
                    return false;
                }

                file = new FileInfo(m_settings.WindiffPath);
                if (!file.Exists)
                {
                    FileInfo windiffFile =
                        new FileInfo(Path.Combine(Application.StartupPath, "windiff.exe"));
                    if (windiffFile.Exists)
                    {
                        m_settings.WindiffPath = windiffFile.FullName;
                        m_settings.SaveToConfigFile();
                    }
                    else
                    {
                        MessageBox.Show("Windiff Tool does not exist.");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bad file path. Check the settings please.");
                return false;
            }
        }

        private void ChangeRunningTestCases2Untested(List<TestCase> testCaseList)
        {
            for (int i = 0; i < testCaseList.Count; i++)
            {
                if (testCaseList[i].TestCaseStatus == TestCaseStatus.Running)
                {
                    testCaseList[i].TestCaseStatus = TestCaseStatus.Untested;
                }
            }
        }

        private void toolStripMenuItemRunSelectedTestCases_Click(object sender, EventArgs e)
        {
            if (m_testCaseSet != null)
            {
                RunSelectedTestCases(m_testCaseSet);
            }
        }

        private void RunSelectedTestCases(TestCaseSet testCaseSet)
        {
            RunAllTestCases(testCaseSet, onlyRunSelected:true, baseline:false);
        }

        private void EnableRunMenuItem(bool enabled)
        {
            toolStripMenuItemRunSelectedTestCases.Enabled = enabled;
            toolStripMenuItemRunAllTestCases.Enabled = enabled;
            toolStripMenuItemOpenTestCaseFile.Enabled = enabled;
            toolStripMenuItemRunAllTestCases.Enabled = enabled;
            toolStripMenuItemBaselineSelectedTestcases.Enabled = enabled;
            toolStripMenuItemStop.Enabled = !enabled;
        }

        private void toolStripMenuItemStop_Click(object sender, EventArgs e)
        {
            if (m_runTestCaseThread != null && m_runTestCaseThread.IsAlive)
            {
                m_runTestCaseThread.Abort();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void dataGridViewTestCases_DoubleClick(object sender, EventArgs e)
        {
            if (dataGridViewTestCases.SelectedRows.Count == 1)
            {
                int index = dataGridViewTestCases.SelectedRows[0].Index;
                TestCase testCase = m_testCaseSet.TestCaseList[index];
                if (testCase.TestCaseStatus == TestCaseStatus.Failed)
                {
                    string target = testCase.TestedTarget;
                    string targetFileName = Path.GetFileName(target);
                    FileInfo file = new FileInfo(testCase.TestedTarget);
                    string outputDirString = file.DirectoryName;
                    ShowDiff(m_settings.WindiffPath, testCase.Baseline,
                        Path.Combine(outputDirString, (index + 1) + targetFileName + ".dll.bsl"));
                }
            }
        }

        private void ShowDiff(string windiffCommand, string fileFullPath1, string fileFullPath2)
        {
            // windiff fileFullPath1 fileFullPath2
            try
            {
                Process p = new Process();
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.FileName = windiffCommand;
                p.StartInfo.Arguments = "\"" + fileFullPath1 + "\" \"" + fileFullPath2 + "\"";
                p.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dataGridViewTestCases_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
        }

        private void toolStripMenuItemBaselineSelectedTestcases_Click(object sender, EventArgs e)
        {
            RunAllTestCases(m_testCaseSet, onlyRunSelected: true, baseline: true);
        }
    }
}
