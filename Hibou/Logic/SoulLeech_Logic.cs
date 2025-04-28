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

				// steal some points based on damage / target maxHealF/h
				float rerollToSteal = damage.magnitude / damagedPlayer.data.maxHealth / 3.0f;
				rerollToSteal = Mathf.Clamp(rerollToSteal, 0.05f, maxAmountToSteal);

				OwlCards.instance.rerollPerPlayer[player.playerID] += rerollToSteal;
				OwlCards.instance.rerollPerPlayer[damagedPlayer.playerID] -= rerollToSteal;
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
