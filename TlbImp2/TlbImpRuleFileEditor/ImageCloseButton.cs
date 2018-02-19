using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace TlbImpRuleFileEditor
{
    public class ImageCloseButton : PictureBox
    {
        private Image imageNormal;
        private Image imageHover;
        private Image imageDown;

        public Image ImageNormal
        {
            get
            {
                return imageNormal;
            }
            set
            {
                imageNormal = value;
                if (this.Image == null)
                    this.Image = imageNormal;
            }
        }

        public Image ImageHover
        {
            get
            {
                return imageHover;
            }
            set
            {
                imageHover = value;
            }
        }

        public Image ImageDown
        {
            get
            {
                return imageDown;
            }
            set
            {
                imageDown = value;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.Image = this.imageDown;
            base.OnMouseDown(e);
        }

        protected override void OnMouseHover(EventArgs e)
        {
            this.Image = this.imageHover;
            base.OnMouseHover(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            this.Image = this.imageNormal;
            base.OnMouseLeave(e);
        }

    }
}
