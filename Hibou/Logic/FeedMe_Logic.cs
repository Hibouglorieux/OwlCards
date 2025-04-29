using OwlCards.Cards;
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
		float rerollLeftToGainThisPoint = FeedMe.rerollPointsToGainPerPoint;
		void Start()
		{
			player = GetComponent<Player>();
			GameModeManager.AddHook(GameModeHooks.HookPointStart, ResetLimitGainedPerPoint);
		}

		private IEnumerator ResetLimitGainedPerPoint(IGameModeHandler gm)
		{
			OwlCards.Log("FeedMe gave " + (rerollLeftToGainThisPoint - FeedMe.rerollPointsToGainPerPoint) + " rerolls this round point");
			rerollLeftToGainThisPoint = FeedMe.rerollPointsToGainPerPoint;
			yield break;
		}

		public override void WasDealtDamage(Vector2 damage, bool selfDamage)
		{
			if (!selfDamage && rerollLeftToGainThisPoint > 0)
			{
				float rerollEarned = damage.magnitude / 1000.0f;
				rerollEarned = Mathf.Min(rerollLeftToGainThisPoint, rerollEarned);

				Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).Soul += rerollEarned;
				rerollLeftToGainThisPoint -= rerollEarned;
			}
		}
		void OnDestroy()
		{
			GameModeManager.RemoveHook(GameModeHooks.HookPointStart, ResetLimitGainedPerPoint);
		}
	}
}
