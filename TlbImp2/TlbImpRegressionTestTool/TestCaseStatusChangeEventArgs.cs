using System;
using System.Collections.Generic;
using System.Text;

namespace TlbImpRegressionTestTool
{
    class TestCaseStatusChangeEventArgs : EventArgs
    {
        private TestCase m_testCase;

        internal TestCaseStatusChangeEventArgs(TestCase testCase)
        {
            m_testCase = testCase;
        }

        public TestCase Source
        {
            get
            {
                return m_testCase;
            }
        }
    }
}
