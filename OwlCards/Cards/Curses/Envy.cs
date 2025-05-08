using ModdingUtils.Extensions;
using RarityBundle;
using RarityLib.Utils;
using UnityEngine;
using System.Linq;
using UnboundLib;
using System.Collections;
using OwlCards.Extensions;
using OwlCards.Dependencies;
using Photon.Pun;

namespace OwlCards.Cards.Curses
{
    internal class Envy : AOwlCard
    {
        public override void SetupCard_child(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
			conditions[GetTitle()] = (float soul) => { return soul < 1.5; };//Mismatch is intended, to put player in negative
			if (!cardInfo.categories.Contains(CurseHandler.CurseCategory))
				cardInfo.categories = cardInfo.categories.Append(CurseHandler.CurseSpawnerCategory).ToArray();
			if (!cardInfo.categories.Contains(OwlCardCategory.soulCondition))
				cardInfo.categories = cardInfo.categories.Append(OwlCardCategory.soulCondition).ToArray();
			cardInfo.GetAdditionalData().canBeReassigned = false;
            //Edits values on card itself, which are then applied to the player in `ApplyCardStats`
        }
        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
			if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient)
				OwlCardsData.UpdateSoul(player.playerID, OwlCardsData.GetData(player.playerID).Soul - 2);
			if (CurseHandler.bCurseAvailable)
			{
				OwlCards.instance.ExecuteAfterFrames(20, () =>
				{
					foreach (int otherPlayerID in Utils.GetOpponentsPlayersIDs(player.playerID))
					{
						float otherPlayerSoul = OwlCardsData.GetData(otherPlayerID).Soul;
						int cursesToGive = 1;
						if (otherPlayerSoul >= 2)
							cursesToGive++;
						if (otherPlayerSoul >= 3)
							cursesToGive++;
						OwlCurse.GiveCurse(Utils.GetPlayerWithID(otherPlayerID), cursesToGive);
					}
				});
			}
            //Edits values on player when card is selected
        }

        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            //Run when the card is removed from the player
        }

        protected override string GetTitle()
        {
            return "Envy";
        }
        protected override string GetDescription()
        {
            return "All foes get curses the more soul they have";
        }
        protected override CardInfoStat[] GetStats()
        {
            return new CardInfoStat[]
            {
                new CardInfoStat()
                {
                    positive = true,
                    stat = "to Foes",
                    amount = "+1 Curse",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = true,
                    stat = "to Foes with 2 Soul",
                    amount = "+1 Curse",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = true,
                    stat = "to Foes with 3 Soul",
                    amount = "+1 Curse",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                },
                new CardInfoStat()
                {
                    positive = false,
                    stat = "Soul",
                    amount = "-2",
                    simepleAmount = CardInfoStat.SimpleAmount.notAssigned
                }
            };
        }

        protected override GameObject GetCardArt()
        {
            return GetCardArt("C_Envy");
        }
        protected override CardInfo.Rarity GetRarity()
        {
            return Rarities.Scarce;
        }

        protected override CardThemeColor.CardThemeColorType GetTheme()
        {
            return CardThemeColor.CardThemeColorType.EvilPurple;
        }
    }
}
