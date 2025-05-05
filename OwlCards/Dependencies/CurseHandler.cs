using System;
using System.Collections.Generic;
using System.Text;

namespace OwlCards.Dependencies
{
	internal class CurseHandler
	{
		public static bool IsPickingCurse()
		{
			return WillsWackyManagers.Utils.CurseManager.instance.CursePick;
		}
	}
}
