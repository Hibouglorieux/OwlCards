using OwlCards.Cards;
using System;
using System.Collections;
using UnityEngine;
using OwlCards.Extensions;

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
				// if damage is suppose to kill (here is shield and damage reduction not taken into account..)
				// then verify next frame (because played isn't dead yet)
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
				float newSoul = CharacterStatModifiersExtension.GetAdditionalData(owner.data.stats).Soul + LastHitter.soulGainedPerKill;
				OwlCardsData.UpdateSoul(new int[] { owner.playerID }, new float[] { newSoul});
			}
			yield break;
		}

		void Start()
		{
			owner = GetComponent<Player>();
		}
	}
}
