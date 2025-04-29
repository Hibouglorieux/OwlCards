using ModdingUtils.RoundsEffects;
using OwlCards.Cards;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnboundLib.GameModes;
using UnityEngine;

namespace OwlCards.Logic
{
	// perhaps use a  instead of a DealtDamageEffect HitEffect
	[DisallowMultipleComponent]
	internal class SoulLeech_Logic : DealtDamageEffect
	{
		Player player;
		Dictionary<int, float> rerollsLeftToStealThisPoint = new Dictionary<int, float>();
		void Start()
		{
			player = GetComponent<Player>();
			GameModeManager.AddHook(GameModeHooks.HookPointStart, ResetStats);
		}

		private IEnumerator ResetStats(IGameModeHandler gm)
		{
			rerollsLeftToStealThisPoint.Clear();
			yield break;
		}

		public override void DealtDamage(Vector2 damage, bool selfDamage, Player damagedPlayer)
		{
            if (!selfDamage && damagedPlayer)
			{
				if (!rerollsLeftToStealThisPoint.ContainsKey(damagedPlayer.playerID))
					rerollsLeftToStealThisPoint.Add(damagedPlayer.playerID, SoulLeech.maxLeechPerRoundPerPlayer);

				float maxAmountToSteal = rerollsLeftToStealThisPoint[damagedPlayer.playerID];
				if (maxAmountToSteal <= 0)
					return;

				// this might be bad, why not make it per bullet ?
				// steal some points based on damage / target maxHealth
				float soulToSteal = damage.magnitude / damagedPlayer.data.maxHealth / 5.0f;
				soulToSteal = Mathf.Min(soulToSteal, maxAmountToSteal);

				Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).Soul += soulToSteal;
				Extensions.CharacterStatModifiersExtension.GetAdditionalData(damagedPlayer.data.stats).Soul -= soulToSteal;
				rerollsLeftToStealThisPoint[damagedPlayer.playerID] -= soulToSteal;

				OwlCards.Log("Stole: " + soulToSteal + " steal");
            }
        }

		void OnDestroy()
		{
			GameModeManager.RemoveHook(GameModeHooks.HookPointStart, ResetStats);
		}
	}
}
