using System.ComponentModel;

namespace OwlCards.Cards
{
	[Description("LastHitter")]
	internal class LastHitter : AOwlCard
	{
		public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			//Edits values on card itself, which are then applied to the player in `ApplyCardStats`
			gun.damage = 1.25f;
			gun.reloadTime = 1.25f;
			gun.ammo = -1;
		}
		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			// TODO add effect on onkill
			//Edits values on player when card is selected
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			//Run when the card is removed from the player
		}

		protected override string GetTitle()
		{
			return "Last Hitter";
		}
		protected override string GetDescription()
		{
			return "Your ennemies will give you some rerolls when you finish them.";
		}
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
				new CardInfoStat()
				{
					positive = true,
					stat = "Damage",
					amount = "+25%",
					simepleAmount = CardInfoStat.SimpleAmount.Some
				},
				new CardInfoStat()
				{
					positive = false,
					stat = "Ammo",
					amount = "-1",
					simepleAmount = CardInfoStat.SimpleAmount.lower
				},
				new CardInfoStat()
				{
					positive = false,
					stat = "Projectile Size",
					amount = "Smaller",
					simepleAmount = CardInfoStat.SimpleAmount.smaller
				},
				new CardInfoStat()
				{
					positive = false,
					stat = "Reload",
					amount = "+25%",
					simepleAmount = CardInfoStat.SimpleAmount.lower
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
			return CardInfo.Rarity.Uncommon;
		}

		protected override CardThemeColor.CardThemeColorType GetTheme()
		{
			return CardThemeColor.CardThemeColorType.DestructiveRed;
		}
	}
}
