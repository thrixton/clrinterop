using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Windows.Forms;

namespace TlbImpRegressionTestTool
{
    public class TestCaseSettings
    {
        public static readonly string KeyTestedCommand = "TestedCommand";
        public static readonly string KeyWindiffPath = "WindiffPath";

        private string m_testedCommand;
        private string m_windiffPath;

        public TestCaseSettings()
        {
            m_testedCommand = ConfigurationManager.AppSettings[KeyTestedCommand];
            m_windiffPath = ConfigurationManager.AppSettings[KeyWindiffPath];
        }

        public string TestedCommand
        {
            get
            {
                return m_testedCommand;
            }
            set
            {
                m_testedCommand = value;
            }
        }

        public string WindiffPath
        {
            get
            {
                return m_windiffPath;
            }
            set
            {
                m_windiffPath = value;
            }
        }

        internal void SaveToConfigFile()
        {
            SetValue(KeyTestedCommand, m_testedCommand);
            SetValue(KeyWindiffPath, m_windiffPath);
        }

        public void SetValue(string AppKey, string AppValue)
        {
            XmlDocument xDoc = new XmlDocument();

            xDoc.Load(Application.StartupPath + "\\TlbImpRegressionTestTool.exe.config");
            XmlNode xNode;
            XmlElement xElem1;
            XmlElement xElem2;
            xNode = xDoc.SelectSingleNode("//appSettings");
            xElem1 = (XmlElement)xNode.SelectSingleNode("//add[@key='" + AppKey + "']");
            if (xElem1 != null)
            {
                xElem1.SetAttribute("value", AppValue);
            }
            else
            {
                xElem2 = xDoc.CreateElement("add");
                xElem2.SetAttribute("key", AppKey);
                xElem2.SetAttribute("value", AppValue);
                xNode.AppendChild(xElem2);
            }
            xDoc.Save(Application.StartupPath + "\\TlbImpRegressionTestTool.exe.config");
        }
    }
}
