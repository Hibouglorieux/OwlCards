using OwlCards.Logic;
using System.ComponentModel;
using UnboundLib;
using UnityEngine;
using Photon.Pun;

namespace OwlCards.Cards
{
    [Description("SoulExhaustion")]
	internal class SoulExhaustion : AOwlCard
	{
		public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			gun.projectileSpeed = 1.15f;
			gun.attackSpeed = 1.25f;
			cardInfo.allowMultiple = false;

			//Edits values on card itself, which are then applied to the player in `ApplyCardStats`
		}
		public static float GetSlowValue(float soulValue)
		{
			return Mathf.Lerp(1.0f, 0.20f, (soulValue - 1.5f) / 5f);
		}

		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
				player.gameObject.GetOrAddComponent<SoulExhaustion_Logic>();
			//Edits values on player when card is selected
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			SoulExhaustion_Logic logic = player.gameObject.GetComponent<SoulExhaustion_Logic>();
			if (logic)
				Destroy(logic);
			//Run when the card is removed from the player
		}

		protected override string GetTitle()
		{
			return "Soul Exhaustion";
		}
		protected override string GetDescription()
		{
			return "Your bullets exhaust your target, slowing them down the less Soul they have.";
		}
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
				new CardInfoStat()
				{
					positive = true,
					stat = "Projectile Speed",
					amount = "+15%",
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
			return CardInfo.Rarity.Uncommon;
		}

		protected override CardThemeColor.CardThemeColorType GetTheme()
		{
			return CardThemeColor.CardThemeColorType.ColdBlue;
		}
	}
}
