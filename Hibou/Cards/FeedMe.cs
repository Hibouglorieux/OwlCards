using OwlCards.Logic;
using UnboundLib;
using UnityEngine;

namespace OwlCards.Cards
{
	internal class FeedMe : AOwlCard
	{
		public const float rerollPointsToGainPerPoint = 0.3f;
		public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			statModifiers.health = 1.5f;
			cardInfo.allowMultiple = false;
			//Edits values on card itself, which are then applied to the player in `ApplyCardStats`
		}
		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			player.gameObject.GetOrAddComponent<FeedMe_Logic>();
			//Edits values on player when card is selected
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			FeedMe_Logic component = player.gameObject.GetComponent<FeedMe_Logic>();
			if (component)
				Destroy(component);
			//Run when the card is removed from the player
		}

		protected override string GetTitle()
		{
			return "Feed Me";
		}
		protected override string GetDescription()
		{
			return "You learned how to assimilate bullets that hit you to strengthen your soul";
		}
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
				new CardInfoStat()
				{
					positive = true,
					stat = "Health",
					amount = "+50%",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				}
			};
		}

		protected override GameObject GetCardArt()
		{
			return GetCardArt("C_FeedMe");
		}
		protected override CardInfo.Rarity GetRarity()
		{
			return CardInfo.Rarity.Common;
		}

		protected override CardThemeColor.CardThemeColorType GetTheme()
		{
			return CardThemeColor.CardThemeColorType.MagicPink;
		}
	}
}
