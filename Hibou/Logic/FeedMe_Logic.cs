using OwlCards.Cards;
using OwlCards.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnboundLib.GameModes;
using UnityEngine;

namespace OwlCards.Logic
{
	internal class FeedMe_Logic : WasDealtDamageEffect
	{
		Player player;
		float soulLeftToGainThisPoint = FeedMe.soulPointsToGainPerPoint;
		void Start()
		{
			player = GetComponent<Player>();
			GameModeManager.AddHook(GameModeHooks.HookPointStart, ResetLimitGainedPerPoint);
		}

		private IEnumerator ResetLimitGainedPerPoint(IGameModeHandler gm)
		{
			OwlCards.Log("FeedMe gave " + (FeedMe.soulPointsToGainPerPoint - soulLeftToGainThisPoint) + " rerolls this round point");
			soulLeftToGainThisPoint = FeedMe.soulPointsToGainPerPoint;
			yield break;
		}

		public override void WasDealtDamage(Vector2 damage, bool selfDamage)
		{
			if (!selfDamage && soulLeftToGainThisPoint > 0)
			{
				float soulEarned = damage.magnitude / 1000.0f;
				soulEarned = Mathf.Min(soulLeftToGainThisPoint, soulEarned);

				float newSoul = CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).Soul + soulEarned;
				OwlCardsData.UpdateSoul(new int[] { player.playerID }, new float[] { newSoul});
				soulLeftToGainThisPoint -= soulEarned;
			}
		}
		void OnDestroy()
		{
			GameModeManager.RemoveHook(GameModeHooks.HookPointStart, ResetLimitGainedPerPoint);
		}
	}
}
