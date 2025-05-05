using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;
using UnboundLib.GameModes;
using System.Linq;
using InControl;
using HarmonyLib;
using Photon.Pun;
using UnboundLib.Networking;
using OwlCards.Extensions;
using UnboundLib;
using System.ComponentModel.Design;
using OwlCards.Dependencies;

namespace OwlCards
{
    internal class Reroll : MonoBehaviour
    {
        static public Reroll instance = null;

		public Dictionary<int, List<(int drawsUntilTriggered, CardCategory[] blacklist)>> customDraws = new Dictionary<int, List<(int, CardCategory[])>>();

		public void AddCustomDraw(int playerID, int drawsUntilTriggered, CardCategory[] blacklist)
		{
			if (customDraws.ContainsKey(playerID))
				customDraws[playerID].Add((drawsUntilTriggered, blacklist));
			else
			{
				List<(int, CardCategory[])> list = new List<(int, CardCategory[])>();
				customDraws.Add(playerID, list);
				customDraws[playerID].Add((drawsUntilTriggered, blacklist));
			}
		}

        void Start()
        {
            instance = this;
            GameModeManager.AddHook(GameModeHooks.HookPlayerPickEnd, CheckRerolls, GameModeHooks.Priority.First);
            GameModeManager.AddHook(GameModeHooks.HookGameStart, OnGameStart);
            GameModeManager.AddHook(GameModeHooks.HookGameEnd, OnGameEnd);
        }

		private IEnumerator OnGameStart(IGameModeHandler gm)
		{
			customDraws.Clear();
			yield break;
		}

		private IEnumerator OnGameEnd(IGameModeHandler gm)
		{
			customDraws.Clear();
			yield break;
		}

		void OnDestroy()
        {
            GameModeManager.RemoveHook(GameModeHooks.HookPlayerPickEnd, CheckRerolls);
        }

        // Watch for input whenever a player is picking
        void Update()
        {
            int pickrID = CardChoice.instance.pickrID;
            if (pickrID == -1)
                return;

			if (OwlCards.instance.bCurseActivated)
			{
				// If we're picking a curse disable reroll
				if (CurseHandler.IsPickingCurse())
				{
					OwlCards.Log("IS PICKING CURSE");
					return;
				}
			}
            var isPlayingField = AccessTools.Field(typeof(CardChoice), "isPlaying");
            bool isPlaying = (bool)isPlayingField.GetValue(CardChoice.instance);

            float currentSoul = CharacterStatModifiersExtension.GetAdditionalData(Utils.GetPlayerWithID(pickrID).data.stats).Soul;

            if (!isPlaying && currentSoul >= OwlCards.instance.rerollSoulCost.Value)
            {
                PlayerActions[] watchedActions = PlayerManager.instance.GetActionsFromPlayer(CardChoice.instance.pickrID);
                if (watchedActions != null)
                {
                    for (int i = 0; i < watchedActions.Length; i++)
                    {
                        if (watchedActions[i] == null)
                            continue;

                        PlayerAction watchedSpecificInput = watchedActions[i].Block;
                        if (watchedSpecificInput.WasPressed)
                        {
                            RerollCards(pickrID, OwlCards.instance.rerollSoulCost.Value);
                            break;
                        }
                        if (OwlCards.instance.bExtraPickActive.Value && watchedActions[i].Fire.WasPressed && currentSoul >= OwlCards.instance.extraPickSoulCost.Value)
                        {
                            var indexField = AccessTools.Field(typeof(CardChoice), "currentlySelectedCard");
                            int selectedCardIndex = (int)indexField.GetValue(CardChoice.instance);

                            var listRefField = AccessTools.FieldRefAccess<CardChoice, List<GameObject>>("spawnedCards");
                            List<GameObject> spawnedCards = listRefField(CardChoice.instance);

                            RerollCards(pickrID, OwlCards.instance.extraPickSoulCost.Value, spawnedCards[selectedCardIndex]);
                            break;
                        }
                        /* the method is private and i can't deselect it for some reason
						if (((OneAxisInputControl)watchedActions[i].Down).Value > 0.7f)
						{
							CardChoiceVisuals.instance.currentCardSelected = -1;
						}
						*/
                    }
                }
            }
        }

        [UnboundRPC]
        private static void UpdateRerollValue_RPC(int[] playerIDs, int[] newValues)
        {
            for (int i = 0; i < playerIDs.Length; i++)
            {
                int playerID = playerIDs[i];
                int newValue = newValues[i];

                OwlCardsData additionalData = CharacterStatModifiersExtension.GetAdditionalData(Utils.GetPlayerWithID(playerID).data.stats);
                var rerollsField = AccessTools.Field(typeof(OwlCardsData), "_rerolls");
                rerollsField.SetValue(additionalData, newValue);
            }
        }


        private IEnumerator ReplaceCards()
        {
            // Copied from  CardChoice.ReplaceCards (line 208)
            var isPlayingField = AccessTools.Field(typeof(CardChoice), "isPlaying");
            isPlayingField.SetValue(CardChoice.instance, true);

            var spawnedCardsRef = AccessTools.FieldRefAccess<List<GameObject>>(typeof(CardChoice), "spawnedCards");
            List<GameObject> spawnedCards = spawnedCardsRef(CardChoice.instance);
            for (int i = 0; i < spawnedCards.Count; i++)
            {
                // changed to Pick to make the card disappear with photon/RPC
                // unlike default method which is local with Leave()
                spawnedCards[i].GetComponentInChildren<CardVisuals>().Pick();
                yield return new WaitForSecondsRealtime(0.1f);
            }
            spawnedCards.Clear();
            yield return new WaitForSecondsRealtime(0.2f);

            CardChoice.instance.GetComponent<PhotonView>().RPC("RPCA_DonePicking", RpcTarget.All);
            isPlayingField.SetValue(CardChoice.instance, false);
        }
        private void PickCard(GameObject cardToPick)
        {
            CardChoice.instance.Pick(cardToPick, cardToPick == null);
            // this is done in CardChoice:435 in normal pick behaviour, pickr ID should be set to -1 after call
            CardChoice.instance.pickrID = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playersIDs"></param>
        /// <param name="newRerollsValue">If null then will be current value + 1</param>
        public void AddReroll(int[] playersIDs, int[] newRerollsValue = null)
        {
            if (newRerollsValue == null)
            {
                newRerollsValue = new int[playersIDs.Length];
                for (int i = 0; i < newRerollsValue.Length; i++)
                {
                    CharacterStatModifiers stats = Utils.GetPlayerWithID(playersIDs[i]).data.stats;
                    newRerollsValue[i] = stats.GetAdditionalData().Rerolls + 1;
                }
            }
            object[] data = { playersIDs, newRerollsValue };
            NetworkingManager.RPC(typeof(Reroll), nameof(UpdateRerollValue_RPC), data);
        }

        public void Add1Reroll(int playerID)
        {
            AddReroll(new int[] { playerID });
        }

        /// <summary>
        /// This method is called to reroll cards displayed without picking a card
        /// </summary>
        /// <param name="pickrID"></param>
        /// <param name="soulUsed"></param>
        /// <param name="cardToPick"></param>
        private void RerollCards(int pickrID, float soulUsed, GameObject cardToPick = null)
        {
            OwlCardsData.UpdateSoul(pickrID,
            CharacterStatModifiersExtension.GetAdditionalData(Utils.GetPlayerWithID(pickrID).data.stats).Soul - soulUsed);
            Add1Reroll(pickrID);
            if (PhotonNetwork.OfflineMode)
            {
                PickCard(cardToPick);
            }
            else
            {
                if (cardToPick)
                {
                    PickCard(cardToPick);
                }
                else
                {
                    StartCoroutine(ReplaceCards());
                }
            }
        }

        private IEnumerator CheckRerolls(IGameModeHandler gm)
        {
            int pickrID = -1;
			List<CardCategory> blacklist = new List<CardCategory>();
			Player chosenPlayer = null;
			//TODO issue: custom draw wait is affected by rerolls
            foreach (Player player in PlayerManager.instance.players.ToArray())
            {
                CharacterStatModifiers stats = player.data.stats;

                if (stats.GetAdditionalData().Rerolls > 0)
                {
					// get data of player rerolling
                    pickrID = player.playerID;
					chosenPlayer = player;

					// get blacklist if needed
					if (customDraws.ContainsKey(pickrID))
					{
						List<(int drawsUntilTriggered, CardCategory[] blacklist)> customDraw = customDraws[pickrID];
						int priority = 1;
						int indexChosen = -1;
						// look into all custom draws for only one (DON'T MIX THEM)
						for (int i = 0; i < customDraw.Count; i++)
						{
							
							// if there's multiple customDraws available pick only the most recent
							if (customDraw[i].drawsUntilTriggered <= 0 && customDraw[i].drawsUntilTriggered < priority)
							{
								indexChosen = i;
								priority = customDraw[i].drawsUntilTriggered;
							}
							// keep iterating as we want to the delay for all
							customDraw[i] = (customDraw[i].drawsUntilTriggered - 1, customDraw[i].blacklist);
						}
						if (indexChosen != -1)
						{
							blacklist = customDraw[indexChosen].blacklist.ToList();
							customDraw.RemoveAt(indexChosen);
						}
					}

					break;
                }
            }

            if (pickrID == -1 || chosenPlayer == null)
                yield break;

			OwlCardsData additionalData = OwlCardsData.GetData(pickrID);
			var rerollsField = AccessTools.Field(typeof(OwlCardsData), "_rerolls");
			rerollsField.SetValue(additionalData, additionalData.Rerolls - 1);

			if (blacklist.Count > 0)
				ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(chosenPlayer.data.stats).blacklistedCategories.AddRange(blacklist);

            yield return GameModeManager.TriggerHook(GameModeHooks.HookPlayerPickStart);
            CardChoiceVisuals.instance.Show(Enumerable.Range(0, PlayerManager.instance.players.Count).Where(i => PlayerManager.instance.players[i].playerID == pickrID).First(), true);
            yield return CardChoice.instance.DoPick(1, pickrID, PickerType.Player);

			foreach (CardCategory cardCategory in blacklist)
				ModdingUtils.Extensions.CharacterStatModifiersExtension.GetAdditionalData(chosenPlayer.data.stats).blacklistedCategories.Remove(cardCategory);

            yield return new WaitForSecondsRealtime(0.1f);
            yield return GameModeManager.TriggerHook(GameModeHooks.HookPlayerPickEnd);
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
}
