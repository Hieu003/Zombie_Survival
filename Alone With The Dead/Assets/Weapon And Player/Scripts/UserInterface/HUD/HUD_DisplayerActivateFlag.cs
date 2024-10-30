using System;

namespace HQFPSWeapons.UserInterface
{
    [Flags]
    public enum HUD_DisplayerActivateFlag
    {
        None = 0,
        Player_TakeDamage = 1,
        Player_UseItem = 2,
        Player_ChangeItem = 4,
        Player_Reload = 8
    }
}
