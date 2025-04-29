using ModdingUtils;
using ModdingUtils.RoundsEffects;
using OwlCards.Cards;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace OwlCards.Logic
{
	[DisallowMultipleComponent]
	internal class LastHitter_Logic : DealtDamageEffect
	{
		Player owner;
		public override void DealtDamage(Vector2 damage, bool selfDamage, Player damagedPlayer = null)
		{
			if (!selfDamage && damagedPlayer)
			{
				if (damage.magnitude > damagedPlayer.data.health)
				{
					StartCoroutine(nameof(CheckIfPlayerDied), damagedPlayer);
				}
			}
		}

		private IEnumerator CheckIfPlayerDied(Player damagedPlayer)
		{
			yield return new WaitForEndOfFrame();
			if (damagedPlayer.data.dead)
			{
				Extensions.CharacterStatModifiersExtension.GetAdditionalData(owner.data.stats).Soul += LastHitter.soulGainedPerKill;
			}
			yield break;
		}

		void Start()
		{
			owner = GetComponent<Player>();
		}
	}
}
