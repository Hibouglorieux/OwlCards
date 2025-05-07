using ModdingUtils.AIMinion.Patches;
using System;
using System.Collections.Generic;
using System.Text;

namespace OwlCards
{
	internal class Utils
	{
		public static Player GetPlayerWithID(int playerID)
		{
			List<Player> players = PlayerManager.instance.players;
			for (int i = 0; i < players.Count; i++)
			{
				if (players[i].playerID == playerID)
				{
					return players[i];
				}
			}

			return null;
		}

		public static int[] GetOtherPlayersIDs(int myPlayerID)
		{
			int[] othersIDs = new int[PlayerManager.instance.players.Count - 1];

			int i = 0;
			foreach (Player otherPlayer in PlayerManager.instance.players.ToArray())
			{
				if (otherPlayer.playerID != myPlayerID)
				{
					othersIDs[i++] = otherPlayer.playerID;
				}
			}
			return othersIDs;
		}

		public static int[] GetOpponentsPlayersIDs(int playerID)
		{
			List<int> opponentsIDs = new List<int>();
			Player player = GetPlayerWithID(playerID);

			foreach (Player otherPlayer in PlayerManager.instance.players)
			{
				if (otherPlayer.teamID != player.teamID)
					opponentsIDs.Add(otherPlayer.teamID);
			}
			return opponentsIDs.ToArray();
		}
	}
}
