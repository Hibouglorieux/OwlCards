using OwlCards.Logic;
using Photon.Pun;
using RarityBundle;
using UnboundLib;
using UnityEngine;

namespace OwlCards.Cards
{
	internal class SoulLeech : AOwlCard
	{
		static public float rerollLeeched => 0.05f;
		static public float maxLeechPerRoundPerPlayer => 0.4f;
		public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
		{
			//Edits values on card itself, which are then applied to the player in `ApplyCardStats`
			statModifiers.lifeSteal = 0.2f;
			gun.damage = 0.85f;
			gun.projectileColor = new Color(0.65f, 0.35f, 0.77f, 1f);
			cardInfo.allowMultiple = false;
		}
		public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
		{
			//Edits values on player when card is selected
			if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
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
			return "Your bullets steal Soul";
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
					amount = "-15%",
					simepleAmount = CardInfoStat.SimpleAmount.slightlyLower
				}
			};
		}

		protected override GameObject GetCardArt()
		{
			return GetCardArt("C_SoulLeech");
		}
		protected override CardInfo.Rarity GetRarity()
		{
			return Rarities.Uncommon;
		}

		protected override CardThemeColor.CardThemeColorType GetTheme()
		{
			return CardThemeColor.CardThemeColorType.NatureBrown;
		}
	}
}
