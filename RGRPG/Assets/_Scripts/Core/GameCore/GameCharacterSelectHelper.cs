using System.Collections;
using System.Collections.Generic;

namespace RGRPG.Core
{
    public class GameCharacterSelectHelper
    {

        public static bool IsValidSlot(int playerNumber, int numPlayers, int slotNumber)
        {
            if (playerNumber == 0)
            {
                if (slotNumber == 0) return true;
                if (slotNumber == 1) return numPlayers < 4;
                if (slotNumber == 2 || slotNumber == 3) return numPlayers == 1;
            }
            else if (playerNumber == 1)
            {
                if (slotNumber == 2) return true;
                if (slotNumber == 3) return numPlayers == 2;
            }
            else if (playerNumber == 2)
            {
                if (slotNumber == 3) return true;
            }
            else if (playerNumber == 3)
            {
                if (slotNumber == 1) return true;
            }
            
            return false;
        }
    }
}