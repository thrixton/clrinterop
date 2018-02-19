using System;
using System.Collections.Generic;
using System.Text;

namespace TlbImpRegressionTestTool
{
    enum TestCaseStatus {
        Untested,
        Running,
        Error,
        Failed,
        Baselined,
        Succeed,
    }

    class TestCase
    {
        private string m_testedTarget;
        private string m_workingDirectory;
        private string m_arguments;
        private string m_baseline;
        private TestCaseStatus m_testCaseStatus = TestCaseStatus.Untested;

        public delegate void StatusChangeHandler(object sender, TestCaseStatusChangeEventArgs e);

        public event StatusChangeHandler StatusChange; 

        public TestCase(string testedTarget, string workingDirectory, string arguments, string baseline)
        {
            m_testedTarget = testedTarget;
            m_workingDirectory = workingDirectory;
            m_arguments = arguments;
            m_baseline = baseline;
        }

        public string TestedTarget
        {
            get
            {
                return m_testedTarget;
            }
            set
            {
                m_testedTarget = value;
            }
        }

        public string WorkingDirectory
        {
            get
            {
                return m_workingDirectory;
            }
            set
            {
                m_workingDirectory = value;
            }
        }

        public string Arguments
        {
            get
            {
                return m_arguments;
            }
            set
            {
                m_arguments = value;
            }
        }

        public TestCaseStatus TestCaseStatus
        {
            get
            {
                return m_testCaseStatus;
            }
            set
            {
                m_testCaseStatus = value;
                TestCaseStatusChangeEventArgs e = new TestCaseStatusChangeEventArgs(this);
                StatusChange(this, e);
            }
        }

        public string Baseline
        {
            get
            {
                return m_baseline;
            }
            set
            {
                m_baseline = value;
            }
        }
    }
}
