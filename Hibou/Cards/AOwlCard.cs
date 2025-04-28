using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using UnboundLib.Cards;
using UnityEngine;


namespace OwlCards.Cards
{
	internal abstract class AOwlCard : CustomCard
	{
		protected GameObject GetCardArt(string name)
		{
			return OwlCards.instance.Bundle.LoadAsset<GameObject>(name);
		}

		public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			//Edits values on card itself, which are then applied to the player in `ApplyCardStats`
		}
		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			//Edits values on player when card is selected
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			//Run when the card is removed from the player
		}
		protected override GameObject GetCardArt()
		{
			return null;
		}
		/*
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
				new CardInfoStat()
				{
					positive = true,
					stat = "Effect",
					amount = "No",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				}
			};
		}
		*/
		public override string GetModName()
		{
			return OwlCards.ModInitials;
		}
	}
}
