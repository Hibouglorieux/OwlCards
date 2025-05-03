using ModdingUtils.RoundsEffects;
using OwlCards.Cards;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OwlCards.Logic
{
	[DisallowMultipleComponent]
	internal class SoulExhaustion_Logic : HitEffect
	{
		public override void DealtDamage(Vector2 damage, bool selfDamage, Player damagedPlayer = null)
		{
			if (!selfDamage && damagedPlayer)
			{
				float soulValue = Extensions.CharacterStatModifiersExtension.GetAdditionalData(damagedPlayer.data.stats).Soul;
				float slowValue = SoulExhaustion.GetSlowValue(soulValue);
				OwlCards.Log("Applying slow value: " + slowValue + " with soulValue: " + soulValue);
				damagedPlayer.data.stats.RPCA_AddSlow(slowValue);
			}
		}
	}
}
