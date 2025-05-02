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

namespace OwlCards
{
	internal class RerollButton : MonoBehaviour
	{
		bool bNeedToAddUI = false;
		static public RerollButton instance = null;


		void Start()
		{
			instance = this;
			GameModeManager.AddHook(GameModeHooks.HookPlayerPickStart, OnPlayerPickStart, GameModeHooks.Priority.VeryLow);
			GameModeManager.AddHook(GameModeHooks.HookPlayerPickEnd, OnPlayerPickEnd, GameModeHooks.Priority.VeryHigh);
			GameModeManager.AddHook(GameModeHooks.HookPlayerPickEnd, CheckRerolls, GameModeHooks.Priority.First);
		}
		void OnDestroy()
		{
			GameModeManager.RemoveHook(GameModeHooks.HookPlayerPickStart, OnPlayerPickStart);
			GameModeManager.RemoveHook(GameModeHooks.HookPlayerPickEnd, OnPlayerPickEnd);
			GameModeManager.RemoveHook(GameModeHooks.HookPlayerPickEnd, CheckRerolls);
		}

		IEnumerator OnPlayerPickStart(IGameModeHandler gm)
		{
			OwlCards.Log("PlayerPickStart called !");

			// delay UI creation as we don't know who is the pickrID yet
			bNeedToAddUI = true;

			yield break;
		}

		IEnumerator OnPlayerPickEnd(IGameModeHandler gm)
		{
			OwlCards.Log("PlayerPickEnd called !");

			UI.Manager.instance.RemoveFill();
			yield break;
		}

		void Update()
		{
			int pickrID = CardChoice.instance.pickrID;
			if (pickrID == -1)
				return;
			var isPlayingField = AccessTools.Field(typeof(CardChoice), "isPlaying");
			bool isPlaying = (bool)isPlayingField.GetValue(CardChoice.instance);
			if (bNeedToAddUI)
			{
				UI.Manager.instance.BuildFillUI(Utils.GetPlayerWithID(CardChoice.instance.pickrID));
				bNeedToAddUI = false;
			}
			float currentSoul = CharacterStatModifiersExtension.GetAdditionalData(Utils.GetPlayerWithID(pickrID).data.stats).Soul;
			if (!isPlaying && currentSoul >= OwlCards.instance.rerollSoulCost.Value)
			{
				PlayerActions[] watchedActions = null;
				watchedActions = PlayerManager.instance.GetActionsFromPlayer(CardChoice.instance.pickrID);
				if (watchedActions != null)
				{
					for (int i = 0; i < watchedActions.Length; i++)
					{
						if (watchedActions[i] == null)
							continue;

						PlayerAction watchedSpecificInput = watchedActions[i].Block;
						if (((OneAxisInputControl)watchedSpecificInput).WasPressed)
						{
							Reroll(pickrID, OwlCards.instance.rerollSoulCost.Value);
							break;
						}
						if (((OneAxisInputControl)(watchedActions[i].Fire)).WasPressed && currentSoul >= OwlCards.instance.extraPickSoulCost.Value)
						{
							var indexField = AccessTools.Field(typeof(CardChoice), "currentlySelectedCard");
							int selectedCardIndex = (int)indexField.GetValue(CardChoice.instance);

							var listRefField = AccessTools.FieldRefAccess<CardChoice, List<GameObject>>("spawnedCards");
							List<GameObject> spawnedCards = listRefField(CardChoice.instance);

							Reroll(pickrID, OwlCards.instance.extraPickSoulCost.Value, spawnedCards[selectedCardIndex]);
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
					newRerollsValue[i] = CharacterStatModifiersExtension.GetAdditionalData(stats).Rerolls + 1;
				}
			}
			object[] data = { playersIDs, newRerollsValue };
			NetworkingManager.RPC(typeof(RerollButton), nameof(UpdateRerollValue_RPC), data);
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
		private void Reroll(int pickrID, float soulUsed, GameObject cardToPick = null)
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
			foreach (Player player in PlayerManager.instance.players.ToArray())
			{
				CharacterStatModifiers stats = player.data.stats;

				if (CharacterStatModifiersExtension.GetAdditionalData(stats).Rerolls > 0)
				{
					pickrID = player.playerID;
					OwlCardsData additionalData = CharacterStatModifiersExtension.GetAdditionalData(stats);
					var rerollsField = AccessTools.Field(typeof(OwlCardsData), "_rerolls");
					rerollsField.SetValue(additionalData, additionalData.Rerolls - 1);
				}
			}
			if (pickrID == -1)
				yield break;

			yield return GameModeManager.TriggerHook(GameModeHooks.HookPlayerPickStart);
			CardChoiceVisuals.instance.Show(Enumerable.Range(0, PlayerManager.instance.players.Count).Where(i => PlayerManager.instance.players[i].playerID == pickrID).First(), true);
			yield return CardChoice.instance.DoPick(1, pickrID, PickerType.Player);
			yield return new WaitForSecondsRealtime(0.1f);
			yield return GameModeManager.TriggerHook(GameModeHooks.HookPlayerPickEnd);
			yield return new WaitForSecondsRealtime(0.1f);
		}
	}
}
