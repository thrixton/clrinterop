using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TlbImpRegressionTestTool
{
    public partial class SettingsForm : Form
    {
        private TestCaseSettings m_settings;

        private string m_testedCommandText;

        private string m_assemPrinterPathText;

        private string m_windiffPathText;

        public SettingsForm(TestCaseSettings settings)
        {
            InitializeComponent();
            m_settings = settings;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            if (m_settings != null)
            {
                if (m_settings.TestedCommand != null)
                {
                    textBoxTestedCommand.Text = m_settings.TestedCommand;
                }
                if (m_settings.WindiffPath != null)
                {
                    textBoxWindiffPath.Text = m_settings.WindiffPath;
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            m_testedCommandText = textBoxTestedCommand.Text;
            m_windiffPathText = textBoxWindiffPath.Text;

            DialogResult = DialogResult.OK;
            Dispose();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Dispose();
        }

        public string TestedCommandText
        {
            get
            {
                return m_testedCommandText;
            }
        }

        public string AssemPrinterPathText
        {
            get {
                return m_assemPrinterPathText;
            }
        }

        public string WindiffPathText
        {
            get {
                return m_windiffPathText;
            }
        }
    }
}
