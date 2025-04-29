using OwlCards.Logic;
using UnboundLib;
using UnityEngine;

namespace OwlCards.Cards
{
	internal class SoulLeech : AOwlCard
	{
		public const float rerollLeeched = 0.1f;
		public const float maxLeechPerRoundPerPlayer = 0.15f;
		public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			//Edits values on card itself, which are then applied to the player in `ApplyCardStats`
			statModifiers.lifeSteal = 0.2f;
			gun.damage = 0.8f;
			cardInfo.allowMultiple = false;
		}
		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			//Edits values on player when card is selected
			player.gameObject.GetOrAddComponent<SoulLeech_Logic>();
		}

		private void MyCustomDamageDealt(Vector2 whatIsThisIDontEvenKnow, bool bThisIsABoolean)
		{
			// Handle my stuff
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			//Run when the card is removed from the player
			SoulLeech_Logic soulLeech_Logic = player.GetComponent<SoulLeech_Logic>();
			if (soulLeech_Logic)
				Destroy(soulLeech_Logic);
		}

		protected override string GetTitle()
		{
			return "Soul Leech";
		}
		protected override string GetDescription()
		{
			return "Your gun now drains your opponent soul";
		}
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
				new CardInfoStat()
				{
					positive = true,
					stat = "Lifesteal",
					amount = "+20%",
					simepleAmount = CardInfoStat.SimpleAmount.Some
				},
				new CardInfoStat()
				{
					positive = false,
					stat = "Damage",
					amount = "-20%",
					simepleAmount = CardInfoStat.SimpleAmount.slightlyLower
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
			return CardThemeColor.CardThemeColorType.NatureBrown;
		}
	}
}
