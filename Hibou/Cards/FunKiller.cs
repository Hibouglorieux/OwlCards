
namespace OwlCards.Cards
{
	internal class FunKiller : AOwlCard
	{
		public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			conditions[GetTitle()] = (float soul) => { return soul >= 2; };
			//Edits values on card itself, which are then applied to the player in `ApplyCardStats`
		}
		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			Extensions.CharacterStatModifiersExtension.GetAdditionalData(characterStats).Soul -= 2;
			foreach (Player otherPlayer in PlayerManager.instance.players.ToArray())
			{
				if (otherPlayer.playerID != player.playerID)
				{
					Extensions.CharacterStatModifiersExtension.GetAdditionalData(otherPlayer.data.stats).Soul -= 0.5f;
				}
			}
			RerollButton.instance.RerollCurrentCards(player.playerID, 0);
			//Edits values on player when card is selected
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			//Run when the card is removed from the player
		}

		protected override string GetTitle()
		{
			return "Fun Killer";
		}
		protected override string GetDescription()
		{
			return "Reroll your cards and drain everyone's soul.";
		}
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
				new CardInfoStat()
				{
					positive = false,
					stat = "Soul",
					amount = "-2",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				},
				new CardInfoStat()
				{
					positive = true,
					stat = "Others Souls",
					amount = "-0.5",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
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
			return CardThemeColor.CardThemeColorType.TechWhite;
		}
	}
}
