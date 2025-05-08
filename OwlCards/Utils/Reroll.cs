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
            GameModeManager.AddHook(GameModeHooks.HookPlayerPickEnd, CheckRerolls, GameModeHooks.Priority.High);
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
			// don't look for input if in esc menu
			if (EscapeMenuHandler.isEscMenu)
				return;

			// not currently picking
            int pickrID = CardChoice.instance.pickrID;
            if (pickrID == -1)
                return;

			// look if card loading animation is still playing beforehand
            var isPlayingField = AccessTools.Field(typeof(CardChoice), "isPlaying");
            bool isPlaying = (bool)isPlayingField.GetValue(CardChoice.instance);

            float currentSoul = CharacterStatModifiersExtension.GetAdditionalData(Utils.GetPlayerWithID(pickrID).data.stats).Soul;

			float soulConsumptionFactor = OwlCardsData.GetData(pickrID).soulConsumptionFactor;

            if (!isPlaying && currentSoul >= OwlCards.instance.rerollSoulCost.Value * soulConsumptionFactor)
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
                            RerollCards(pickrID, OwlCards.instance.rerollSoulCost.Value * soulConsumptionFactor);
                            break;
                        }
						var listRefField = AccessTools.FieldRefAccess<CardChoice, List<GameObject>>("spawnedCards");
						List<GameObject> spawnedCards = listRefField(CardChoice.instance);
                        if (OwlCards.instance.bExtraPickActive.Value && watchedActions[i].Fire.WasPressed &&
							currentSoul >= OwlCards.instance.extraPickSoulCost.Value * soulConsumptionFactor
							&& spawnedCards.Count > 1)
                        {
							// If we're picking a curse disable extrapick
							if (OwlCards.instance.bCurseActivated)
								if (CurseHandler.IsPickingCurse)
									continue;

							OwlCardsData.RequestUpdateSoul(new int[] { pickrID }, new float[] { -OwlCards.instance.extraPickSoulCost.Value * soulConsumptionFactor });

							var indexField = AccessTools.Field(typeof(CardChoice), "currentlySelectedCard");
                            int selectedCardIndex = (int)indexField.GetValue(CardChoice.instance);


							DoExtraPickRequest(pickrID, spawnedCards[selectedCardIndex]);
                            break;
                        }

						// Later: rework UI for selection of more options than 2 choices ?
                        /*
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

		// Copied from  CardChoice.ReplaceCards (line 208)
        private IEnumerator ReplaceCards()
        {
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

			var spawnUniqueCardMethod = AccessTools.Method(typeof(CardChoice), "SpawnUniqueCard");
			var childrenRef = AccessTools.FieldRefAccess<Transform[]>(typeof(CardChoice), "children");
			Transform[] children = childrenRef(CardChoice.instance);
			for (int j = 0; j < children.Length; j++)
			{
				GameObject newSpawnedCard = (GameObject)spawnUniqueCardMethod.Invoke(CardChoice.instance, new object[]{
					children[j].transform.position, children[j].transform.rotation
				});
				spawnedCards.Add(newSpawnedCard);
				spawnedCards[j].AddComponent<PublicInt>().theInt = j;
				yield return new WaitForSecondsRealtime(0.1f);
			}
            //CardChoice.instance.GetComponent<PhotonView>().RPC("RPCA_DonePicking", RpcTarget.All);
            isPlayingField.SetValue(CardChoice.instance, false);
        }

		// Copied from CardChoice.DoEndPick (line 121)
		private IEnumerator DoExtraPickClients(int[] cardIDs, int targetCardID, int theInt, int pickId)
		{
			//prevent another input and set isPlaying to true
            var isPlayingField = AccessTools.Field(typeof(CardChoice), "isPlaying");
			isPlayingField.SetValue(CardChoice.instance, true);

			GameObject pickedCard = PhotonNetwork.GetPhotonView(targetCardID).gameObject;

            var spawnedCardsRef = AccessTools.FieldRefAccess<List<GameObject>>(typeof(CardChoice), "spawnedCards");
            List<GameObject> spawnedCards = spawnedCardsRef(CardChoice.instance);

			var cardFromIDSMethod = AccessTools.Method(typeof(CardChoice), "CardFromIDs");

			// Had to make this local only otherwise there is a bug sound for some reason i couldn't find
			if (Utils.GetPlayerWithID(pickId).data.view.IsMine)
			{
				spawnedCards.Clear();
				spawnedCards.AddRange((List<GameObject>)cardFromIDSMethod.Invoke(CardChoice.instance, new object[] { cardIDs }));
			}

			//bring card to player
			Vector3 startPos = pickedCard.transform.position;
			Vector3 endPos = CardChoiceVisuals.instance.transform.position;
			float c2 = 0f;
			while (c2 < 1f)
			{
				CardChoiceVisuals.instance.framesToSnap = 1;
				Vector3 position = Vector3.LerpUnclamped(startPos, endPos, c2);
				pickedCard.transform.position = position;
				CardChoice.instance.transform.GetChild(theInt).position = position;
				c2 += Time.deltaTime * 4f;
				yield return null;
			}

			//update array of cards by removing picked card
			pickedCard.GetComponentInChildren<CardVisuals>().Leave();

			//spawnedCards update locally only
			if (Utils.GetPlayerWithID(pickId).data.view.IsMine)
				spawnedCards.Remove(pickedCard);

			// bring arm back to where it was
			AnimationCurve softCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
			Vector3 startPos2 = CardChoice.instance.transform.GetChild(theInt).transform.position;
			Vector3 endPos2 = startPos;
			c2 = 0f;
			while (c2 < 1f)
			{
				Vector3 position2 = Vector3.LerpUnclamped(startPos2, endPos2, softCurve.Evaluate(c2));
				CardChoice.instance.transform.GetChild(theInt).position = position2;
				c2 += Time.deltaTime * 4.0f * 1.5f;
				yield return null;
			}

			isPlayingField.SetValue(CardChoice.instance, false);
			yield break;
		}

		[UnboundRPC]
		private static void ExtraPick_RPC(int[] cardIDs, int targetCardID, int theInt, int pickId)
		{
			IEnumerator e = instance.DoExtraPickClients(cardIDs, targetCardID, theInt, pickId);
			instance.StartCoroutine(e);
		}

		private void DoExtraPickRequest(int pickrID, GameObject pickedCard)
		{
			var pickerTypeField = AccessTools.Field(typeof(CardChoice), "pickerType");
			PickerType pickerType = (PickerType)pickerTypeField.GetValue(CardChoice.instance);

			pickedCard.GetComponentInChildren<ApplyCardStats>().Pick(pickrID, forcePick: false, pickerType);

			int targetCardID = pickedCard.GetComponent<PhotonView>().ViewID;
			int theInt = pickedCard.GetComponent<PublicInt>().theInt;
			var cardFromIDSMethod = AccessTools.Method(typeof(CardChoice), "CardIDs");
			int[] cardIDs = (int[])cardFromIDSMethod.Invoke(CardChoice.instance, new object[] { });

			NetworkingManager.RPC(typeof(Reroll), nameof(ExtraPick_RPC), new object[] {
				cardIDs,
				pickedCard.GetComponent<PhotonView>().ViewID,
				pickedCard.GetComponent<PublicInt>().theInt,
				pickrID
			});
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
			UpdateRerollValue_RPC(playersIDs, newRerollsValue);
			if (!PhotonNetwork.OfflineMode)
				NetworkingManager.RPC_Others(typeof(Reroll), nameof(UpdateRerollValue_RPC), data);
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
			OwlCardsData.RequestUpdateSoul(new int[] { pickrID }, new float[] { -soulUsed });
			if (cardToPick)
				PickCard(cardToPick);
			else
				StartCoroutine(ReplaceCards());
        }

        private IEnumerator CheckRerolls(IGameModeHandler gm)
        {
			OwlCards.Log("CheckRerolls called !");
            int pickrID = -1;
			List<CardCategory> blacklist = new List<CardCategory>();
			Player chosenPlayer = null;
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


			int newRerollID = -1;
			foreach (Player player in PlayerManager.instance.players.ToArray())
			{
				CharacterStatModifiers stats = player.data.stats;
				if (stats.GetAdditionalData().Rerolls > 0)
				{
					newRerollID = player.playerID;
					break;
				}
			}

            yield return new WaitForSecondsRealtime(0.1f);
            yield return GameModeManager.TriggerHook(GameModeHooks.HookPlayerPickEnd);
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
}
