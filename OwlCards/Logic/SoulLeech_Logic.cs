using ModdingUtils.RoundsEffects;
using OwlCards.Cards;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnboundLib.GameModes;
using OwlCards.Extensions;
using UnityEngine;

namespace OwlCards.Logic
{
	// perhaps use a HitEffect instead of a DealtDamageEffect
	[DisallowMultipleComponent]
	internal class SoulLeech_Logic : DealtDamageEffect
	{
		Player player;
		Dictionary<int, float> soulLeftToStealThisPoint = new Dictionary<int, float>();
		void Start()
		{
			player = GetComponent<Player>();
			GameModeManager.AddHook(GameModeHooks.HookPointStart, ResetStats);
		}

		private IEnumerator ResetStats(IGameModeHandler gm)
		{
			soulLeftToStealThisPoint.Clear();
			yield break;
		}

		public override void DealtDamage(Vector2 damage, bool selfDamage, Player damagedPlayer)
		{
            if (!selfDamage && damagedPlayer)
			{
				if (!soulLeftToStealThisPoint.ContainsKey(damagedPlayer.playerID))
					soulLeftToStealThisPoint.Add(damagedPlayer.playerID, SoulLeech.maxLeechPerRoundPerPlayer);

				float maxAmountToSteal = soulLeftToStealThisPoint[damagedPlayer.playerID];
				if (maxAmountToSteal <= 0)
					return;

				// this might be bad, why not make it per bullet ?
				// steal some points based on damage / target maxHealth
				float soulToSteal = damage.magnitude / damagedPlayer.data.maxHealth / 5.0f;
				soulToSteal = Mathf.Min(soulToSteal, maxAmountToSteal);


				int[] playerIDs = new int[2] { player.playerID, damagedPlayer.playerID };
				float[] newSouls = new float[2] {
					CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).Soul + soulToSteal,
					CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).Soul - soulToSteal
				};
				OwlCardsData.UpdateSoul(playerIDs, newSouls);

				soulLeftToStealThisPoint[damagedPlayer.playerID] -= soulToSteal;

				OwlCards.Log("Stole: " + soulToSteal + " steal" + " from playerID: " + damagedPlayer.playerID + " to me: " + player.playerID);
            }
        }

		void OnDestroy()
		{
			GameModeManager.RemoveHook(GameModeHooks.HookPointStart, ResetStats);
		}
	}
}
