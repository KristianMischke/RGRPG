using System.Collections;
using System.Collections.Generic;

namespace RGRPG.Core.NetworkCore
{
    public interface IGameServerManager
    {
        // Initial connection and client syncing
        void BroadcastClientConnect(int id, int playerNumber, bool observer);
        void SyncClientInfo(object[] data);

        void BroadcastBeginGame(string[] chosenCharacters);
        void BroadcastPlayerUpdate(object[] data);
        void BroadcastEnemyUpdate(object[] data);
        void BroadcastSceneUpdate(string sceneID);
        void BroadcastBeginCombat(int[] enemyIDs);
        void BroadcastCombatState(int combatState);
    }
}