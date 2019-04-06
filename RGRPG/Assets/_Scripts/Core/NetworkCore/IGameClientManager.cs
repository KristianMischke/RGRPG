using System.Collections;
using System.Collections.Generic;

namespace RGRPG.Core.NetworkCore
{
    public interface IGameClientManager
    {
        // Inital connection
        void RequestClientID(bool isObserver);

        // Character Selection Screen
        void ChooseCharacterToPlay(string zType, int slot);
        void SubmitCharacterSelection();

        // In Game
        void MoveCharacter(int xDirection, int yDirection);

        // Combat States
        void CombatFinishPlayerTurnInput();
        void CombatWaitingForNextRound();
    }
}