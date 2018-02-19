using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace AssemblyTreeViewer
{
    public partial class AssemblyTVForm : Form
    {
        public AssemblyTVForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.Load("mscorlib");
            this.treeViewAssembly.SetAssembly(assembly);
        }
    }
}
