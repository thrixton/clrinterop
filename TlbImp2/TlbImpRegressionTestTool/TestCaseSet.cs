using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml;
using System.IO;

namespace TlbImpRegressionTestTool
{
    class TestCaseSet
    {
        private string m_testCaseFilePath;

        private List<TestCase> m_list = new List<TestCase>();

        public readonly string TestCaseElementName = "TestCase";

        public readonly string TargetAttributeName = "Target";

        public readonly string ArgumentsAttributeName = "Arguments";

        public readonly string WorkingDirectoryAttributeName = "WorkingDirectory";

        public readonly string BaselineAttributeName = "Baseline";
        
        public List<TestCase> TestCaseList
        {
            get
            {
                return m_list;
            }
        }

        public string TestCaseFilePath
        {
            get
            {
                return m_testCaseFilePath;
            }
        }

        public TestCaseSet(string testCaseFile)
        {
            FileInfo file = new FileInfo(testCaseFile);
            m_testCaseFilePath = file.FullName;
            XmlDocument doc = new XmlDocument();
            doc.Load(testCaseFile);
            XmlNodeList testCaseNodeList = doc.GetElementsByTagName(TestCaseElementName);
            foreach (XmlNode testCaseNode in testCaseNodeList)
            {
                TestCase testCase = ProcessTestCaseNode(testCaseNode);
                m_list.Add(testCase);
            }
        }

        private TestCase ProcessTestCaseNode(XmlNode testCaseNode)
        {
            FileInfo file = new FileInfo(m_testCaseFilePath);
            string testCaseFileDir = file.DirectoryName;
            // Target Attribute
            string target = null;
            XmlNode targetAttributeNode = testCaseNode.Attributes.GetNamedItem(
                    TargetAttributeName);
            if (targetAttributeNode != null)
            {
                if (Path.IsPathRooted(targetAttributeNode.InnerText))
                {
                    target = targetAttributeNode.InnerText;
                }
                else
                {
                    target = (new FileInfo(Path.Combine(testCaseFileDir,
                                                        targetAttributeNode.InnerText))).FullName;
                }
            }

            // WorkingDirectory Attribute
            string workingDirectory = null;
            XmlNode workingDirectoryAttributeNode = testCaseNode.Attributes.GetNamedItem(
                   WorkingDirectoryAttributeName);
            if (workingDirectoryAttributeNode != null)
            {
                if (Path.IsPathRooted(workingDirectoryAttributeNode.InnerText))
                {
                    workingDirectory = workingDirectoryAttributeNode.InnerText;
                }
                else
                {
                    workingDirectory =
                        (new FileInfo(Path.Combine(testCaseFileDir,
                                                   workingDirectoryAttributeNode.InnerText))).FullName;
                }
            }
            else
            {
                workingDirectory = testCaseFileDir;
            }

            // Arguments Attribute
            string arguments = null;
            XmlNode argumentsAttributeNode = testCaseNode.Attributes.GetNamedItem(
                   ArgumentsAttributeName);
            if (argumentsAttributeNode != null)
                arguments = argumentsAttributeNode.InnerText;

            // Baseline Attribute
            string baseline = null;
            XmlNode baselineAttributeNode = testCaseNode.Attributes.GetNamedItem(
                   BaselineAttributeName);
            if (baselineAttributeNode != null)
            {
                if (Path.IsPathRooted(baselineAttributeNode.InnerText))
                {
                    baseline = baselineAttributeNode.InnerText;
                }
                else
                {
                    baseline = (new FileInfo(Path.Combine(testCaseFileDir,
                                                          baselineAttributeNode.InnerText))).FullName;
                }
            }

            if (target == null || workingDirectory == null || arguments == null || baseline == null)
            {
                throw new BadXmlFormatException();
            }
            else
            {
                return new TestCase(target, workingDirectory, arguments, baseline);
            }
        }
    }
}
