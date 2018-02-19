using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using TlbImpRuleEngine;

namespace TlbImpRuleFileEditor
{
    public partial class ParameterIndexNumericUpDown : UserControl
    {
        public ParameterIndexNumericUpDown()
        {
            InitializeComponent();
        }

        public int Value
        {
            get
            {
                return (int) this.numericUpDownParameterIndex.Value;
            }
            set
            {
                int oldValue = (int) this.numericUpDownParameterIndex.Value;
                this.numericUpDownParameterIndex.Value = value;
                if (oldValue == value)
                    numericUpDownParameterIndex_ValueChanged(null, null);
            }
        }

        private void numericUpDownParameterIndex_ValueChanged(object sender, EventArgs e)
        {
            this.textBoxParameterIndex.Text =
                NativeParameterIndexCondition.GetParameterIndexText(
                    (int) numericUpDownParameterIndex.Value);
        }
    }
}
