
namespace OwlCards.Cards
{
	internal class $safeitemrootname$: AOwlCard
	{
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

		protected override string GetTitle()
		{
			return "$safeitemrootname$";
		}
		protected override string GetDescription()
		{
			return "";
		}
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
				/*
				new CardInfoStat()
				{
					positive = true,
					stat = "Effect",
					amount = "No",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				}
				*/
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
			return CardThemeColor.CardThemeColorType.TechWhite;
		}
	}
}
