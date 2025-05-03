using ModdingUtils.RoundsEffects;
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
	internal class FeedMe_Logic : WasHitEffect
	{
		Player player;
		void Start()
		{
			player = GetComponent<Player>();
		}

		public override void WasDealtDamage(Vector2 damage, bool selfDamage)
		{
			if (!selfDamage)
			{
				float soulEarned = Mathf.Min(damage.magnitude, player.data.health) / 1000.0f;

				float newSoul = CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).Soul + soulEarned;
				OwlCardsData.UpdateSoul(player.playerID, newSoul);
			}
		}
	}
}
