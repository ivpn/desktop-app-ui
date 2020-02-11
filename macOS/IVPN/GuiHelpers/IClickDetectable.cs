using System;
namespace IVPN.GuiHelpers
{
	public delegate void OnClickDelegate ();
	public delegate void OnDoubleClickDelegate ();

	/// <summary>
	/// Interface for window objects: clicks detectable
	/// 
	/// USAGE EXAMPLE:
	///  Variable definitions:
	///     private readonly GuiHelpers.ClickDetection _clickDetector = new GuiHelpers.ClickDetection ();
	///     
	///     public event GuiHelpers.OnClickDelegate OnClick;
	///     public event GuiHelpers.OnDoubleClickDelegate OnDoubleClick;
	/// 
	///  Window initialization:
	///     _clickDetector.OnClick += () => {OnDoubleClick?.Invoke ();};
	///     _clickDetector.OnDoubleClick += () => {OnClick?.Invoke ();};
	/// 
	///  Implementation:
	///     public override void MouseUp (NSEvent theEvent)
	///     {
	///         base.MouseUp (theEvent);
	///         _clickDetector.MouseUp();
	///     }
	///     public override void MouseDown (NSEvent theEvent)
	///     {
	///         base.MouseDown (theEvent);
	///         _clickDetector.MouseDown ();
	///     }
	/// </summary>
	public interface IClickDetectable
    {
		event OnClickDelegate OnClick;
		event OnDoubleClickDelegate OnDoubleClick;
    }
}
