using OwlCards.Cards;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnboundLib.GameModes;
using UnityEngine;

namespace OwlCards.Logic
{
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
				float rerollToSteal = damage.magnitude / damagedPlayer.data.maxHealth / 5.0f;
				rerollToSteal = Mathf.Min(rerollToSteal, maxAmountToSteal);

				Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).soul += rerollToSteal;
				Extensions.CharacterStatModifiersExtension.GetAdditionalData(damagedPlayer.data.stats).soul -= rerollToSteal;
				rerollsLeftToStealThisPoint[damagedPlayer.playerID] -= rerollToSteal;

				OwlCards.Log("Stole: " + rerollToSteal + " rerolls");
            }
        }

		void OnDestroy()
		{
			GameModeManager.RemoveHook(GameModeHooks.HookPointStart, ResetStats);
		}
	}
}
