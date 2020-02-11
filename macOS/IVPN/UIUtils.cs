using System;
using CoreGraphics;

using AppKit;

namespace IVPN
{
    public class UIUtils
    {
        public static CGRect UpdateHeight(CGRect originalFrame, nfloat newHeight)
        {
            CGRect newRect = originalFrame;
            newRect.Y += originalFrame.Height;
            newRect.Y -= newHeight;
            newRect.Height = newHeight;

            return newRect;
        }

        public static void DisableButtonHightlighting(NSButton button)
        {            
            button.Cell.HighlightsBy = (int)NSCellStyleMask.NoCell;
            button.Cell.ShowsStateBy = (int)NSCellStateValue.Off;
        }

        public static NSTextField NewLabel(string text)
        {
            NSTextField textField = new NSTextField();
            textField.Editable = false;
            textField.DrawsBackground = false;
            textField.StringValue = text;
            textField.Bordered = false;
            textField.SizeToFit();

            return textField;
        }

        public static NSFont GetSystemFontOfSize(nfloat fontSize)
        {
            return NSFont.SystemFontOfSize(fontSize);
        }

        public static NSFont GetSystemFontOfSize(nfloat fontSize, nfloat weight)
        {
            // not possible to use  NSFont.SystemFontOfSize because it supported only since macOS 10.11+

            NSFont f = NSFont.SystemFontOfSize(fontSize);//, weight);

            NSFontManager manger = NSFontManager.SharedFontManager;

            // Full details here – https://developer.apple.com/library/mac/documentation/Cocoa/Reference/ApplicationKit/Classes/NSFontManager_Class/#//apple_ref/occ/instm/NSFontManager/convertWeight:ofFont:   
            //
            // 1 – ultralight
            // 2 – thin
            // 3 – light, extralight
            // 4 – book
            // 5 – regular, display
            // 6 – medium
            // 7 – demi, demibold
            // 8 – semi, semibold
            // 9 – bold
            // 10 – extra, extrabold
            // 11 – heavy
            // 12 – black
            // 13 – ultrablack
            // 14 – extrablack
            int w = 5;
            if (weight == NSFontWeight.UltraLight) 
                w = 1;
            else if (weight == NSFontWeight.Thin)
                w = 2;
            else if (weight == NSFontWeight.Light)
                w = 3;
            else if (weight == NSFontWeight.Regular)
                w = 5;
            else if (weight == NSFontWeight.Medium)
                w = 6;
            else if (weight == NSFontWeight.Semibold)
                w = 8;
            else if (weight == NSFontWeight.Bold)
                w = 9;
            else if (weight == NSFontWeight.Heavy)
                w = 11;
            else if (weight == NSFontWeight.Black)
                w = 12;

            NSFont ret = manger.FontWithFamily(f.FamilyName, manger.TraitsOfFont(f), w, fontSize);

            return ret;
        }
    }
}

