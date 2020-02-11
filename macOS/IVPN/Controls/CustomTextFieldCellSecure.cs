using System;
using AppKit;
using CoreGraphics;
using Foundation;

namespace IVPN
{
    /// <summary>
    /// Custom secure text field cell.
    /// </summary>
    [Register ("CustomTextFieldCellSecure")]
    public class CustomTextFieldCellSecure : NSSecureTextFieldCell
    {
        #region Constructors
        public CustomTextFieldCellSecure ()
        {
            // Init
            Initialize ();
        }

        public CustomTextFieldCellSecure (IntPtr handle) : base (handle)
        {
            // Init
            Initialize ();
        }

        private void Initialize ()
        {
            Title = "";
            CellPaddingHorisontally = 15;
            CellPaddingVertically = 7;

            this.Font = UIUtils.GetSystemFontOfSize (14);
            this.UsesSingleLineMode = true;
        }
        #endregion

        public override CGRect DrawingRectForBounds (CGRect theRect)
        {
            if (CellPaddingLeft != 0 || CellPaddingRight != 0 || CellPaddingTop != 0 || CellPaddingButtom != 0) {
                var rect = new CGRect (
                    theRect.X + CellPaddingLeft,
                    theRect.Y + CellPaddingTop,
                    theRect.Width - CellPaddingRight,
                    theRect.Height - CellPaddingButtom);

                return base.DrawingRectForBounds (rect);
            }

            return base.DrawingRectForBounds (theRect);
        }

        public nfloat CellPaddingLeft { get; set; }
        public nfloat CellPaddingRight { get; set; }
        public nfloat CellPaddingTop { get; set; }
        public nfloat CellPaddingButtom { get; set; }

        public nfloat CellPaddingVertically {
            set {
                CellPaddingTop = value;
                CellPaddingButtom = value;
            }
        }

        public nfloat CellPaddingHorisontally {
            set {
                CellPaddingLeft = value;
                CellPaddingRight = value;
            }
        }
    }
}
