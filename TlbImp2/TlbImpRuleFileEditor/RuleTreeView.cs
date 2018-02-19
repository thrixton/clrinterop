using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using CoreRuleEngine;

namespace TlbImpRuleFileEditor
{
    public class RuleTreeView : TreeView
    {
        #region Scroll Event

        // Event declaration
        public delegate void ScrollEventHandler(object sender, ScrollEventArgs e);
        
        public event ScrollEventHandler Scroll;
        
        // WM_HSCROLL, WM_VSCROLL message constants
        private const int WM_HSCROLL = 0x114;
        private const int WM_VSCROLL = 0x115;
        private const int WM_MOUSEWHEEL = 0x20A;

        private const int SB_THUMBTRACK = 5;
        private const int SB_ENDSCROLL = 8;

        protected override void WndProc(ref Message m)
        {
            // Trap the WM_VSCROLL message to generate the Scroll event
            base.WndProc(ref m);
            if (Scroll != null &&
                (m.Msg == WM_VSCROLL || m.Msg == WM_HSCROLL || m.Msg == WM_MOUSEWHEEL))
            {
                Scroll.Invoke(this, new ScrollEventArgs());
            }
        }

        public class ScrollEventArgs
        {
        }
        
        #endregion
    }
}
