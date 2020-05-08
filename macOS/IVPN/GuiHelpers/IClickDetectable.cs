//
//  IVPN Client Desktop
//  https://github.com/ivpn/desktop-app-ui
//
//  Created by Stelnykovych Alexandr.
//  Copyright (c) 2020 Privatus Limited.
//
//  This file is part of the IVPN Client Desktop.
//
//  The IVPN Client Desktop is free software: you can redistribute it and/or
//  modify it under the terms of the GNU General Public License as published by the Free
//  Software Foundation, either version 3 of the License, or (at your option) any later version.
//
//  The IVPN Client Desktop is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
//  or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more
//  details.
//
//  You should have received a copy of the GNU General Public License
//  along with the IVPN Client Desktop. If not, see <https://www.gnu.org/licenses/>.
//

﻿using System;
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
