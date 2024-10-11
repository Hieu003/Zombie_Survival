using System;

namespace HQFPSWeapons
{
	[Flags]
	public enum ItemContainerFlags
	{
		Storage = 1,
		Equipment = 2,
		Hotbar = 4,
		External = 8,
		AmmoPouch = 16
	}
}