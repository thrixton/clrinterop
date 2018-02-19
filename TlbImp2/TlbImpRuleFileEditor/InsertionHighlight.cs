using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace TlbImpRuleFileEditor
{
    public partial class InsertionHighlight : Panel
    {
        Pen m_pen;
        Point m_startPoint;
        Point m_endPoint;

        public InsertionHighlight()
        {
            InitializeComponent();
            Init();
        }

        public InsertionHighlight(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
            Init();
        }

        private void Init()
        {
            m_pen = new Pen(Color.Blue);
            m_pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            m_pen.Width = 2;
            m_startPoint = new Point(0, 1);
            m_endPoint = new Point(40, 1);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawLine(m_pen, m_startPoint, m_endPoint);
            base.OnPaint(e);
        }
    }
}
