using System.ComponentModel;
using UnityEngine;

namespace OwlCards.Cards
{
	[Description("SoulExhaustion")]
	internal class SoulExhaustion : AOwlCard
	{
		public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			gun.projectileSpeed = 1.3f;
			gun.attackSpeed = 1.25f;

			//TODO tmp

			gun.slow = 0.25f;
			//Mathf.Lerp(0.25f, 1.0f, insertratio);
			//Edits values on card itself, which are then applied to the player in `ApplyCardStats`
		}
		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			//TODO
			//Edits values on player when card is selected
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			//Run when the card is removed from the player
		}

		protected override string GetTitle()
		{
			return "Soul Exhaustion";
		}
		protected override string GetDescription()
		{
			return "Your bullets exhaust your target, slowing them down the less reroll they have.";
		}
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
				new CardInfoStat()
				{
					positive = true,
					stat = "Projectile Speed",
					amount = "+30%",
					simepleAmount = CardInfoStat.SimpleAmount.aLittleBitOf
				},
				new CardInfoStat()
				{
					positive = true,
					stat = "Attack Speed",
					amount = "+25%",
					simepleAmount = CardInfoStat.SimpleAmount.aLittleBitOf
				}
			};
		}
		/*
			protected override GameObject GetCardArt()
			{
				return GetCardArt("C_CARD_NAME");
			}
		*/
		protected override CardInfo.Rarity GetRarity()
		{
			return CardInfo.Rarity.Common;
		}

		protected override CardThemeColor.CardThemeColorType GetTheme()
		{
			return CardThemeColor.CardThemeColorType.ColdBlue;
		}
	}
}
