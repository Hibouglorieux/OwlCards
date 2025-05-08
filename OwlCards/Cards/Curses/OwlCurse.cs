using System.Collections;
using OwlCards.Dependencies;

namespace OwlCards.Cards.Curses
{
	internal class OwlCurse
	{
		static public void GiveCurse(Player player, int amount = 1)
		{
			OwlCards.instance.StartCoroutine(GiveCurseCoroutine(player, amount));
		}

		static private IEnumerator GiveCurseCoroutine(Player player, int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				CurseHandler.CursePlayer(player, (curse) => { ModdingUtils.Utils.CardBarUtils.instance.ShowAtEndOfPhase(player, curse); });
				for (int j = 0; j < 20; j++)
					yield return null;
			}
			yield break;
		}
	}
}
