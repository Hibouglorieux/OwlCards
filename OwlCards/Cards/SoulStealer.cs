using ModdingUtils.Extensions;
using RarityBundle;
using RarityLib.Utils;
using UnboundLib;
using UnityEngine;
using OwlCards.Logic;
using Photon.Pun;

namespace OwlCards.Cards
{
	internal class SoulStealer : AOwlCard
	{
		static public float baseRadius => 1.5f / 1.24f;// 1.24 is the base scale of a player
		static public float amountOfStealToSteal => 0.15f;
		static public float baseSlow => 0.4f;
		static public float slowDuration => 2.0f;
		static public float delayBetweenSteal => 5.0f;
		public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			statModifiers.movementSpeed = 1.3f;
			statModifiers.jump = 1.25f;
			cardInfo.allowMultiple = false;
			//Edits values on card itself, which are then applied to the player in `ApplyCardStats`
		}
		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			if (PhotonNetwork.OfflineMode || player.GetComponent<PhotonView>().IsMine)
				player.gameObject.GetOrAddComponent<SoulStealer_Logic>();
			//Edits values on player when card is selected
		}
		public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			SoulStealer_Logic soulStealer_Logic = player.GetComponent<SoulStealer_Logic>();
			if (soulStealer_Logic)
				Destroy(soulStealer_Logic);
			//Run when the card is removed from the player
		}

		protected override string GetTitle()
		{
			return "Soul Stealer";
		}
		protected override string GetDescription()
		{
			return "Touching a foe will steal some of his soul and restrain his movement";
		}
		protected override CardInfoStat[] GetStats()
		{
			return new CardInfoStat[]
			{
				new CardInfoStat()
				{
					positive = true,
					stat = "Move speed",
					amount = "+30%",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				},
				new CardInfoStat()
				{
					positive = true,
					stat = "Jump height",
					amount = "+25%",
					simepleAmount = CardInfoStat.SimpleAmount.notAssigned
				}
			};
		}

		protected override GameObject GetCardArt()
		{
			return GetCardArt("C_SoulStealer");
		}
		protected override CardInfo.Rarity GetRarity()
		{
			return Rarities.Exotic;
		}

		protected override CardThemeColor.CardThemeColorType GetTheme()
		{
			return CardThemeColor.CardThemeColorType.NatureBrown;
		}
	}
}
