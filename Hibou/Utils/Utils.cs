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
	}
}
