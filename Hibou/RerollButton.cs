using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;
using UnboundLib.GameModes;
using UnityEngine.SceneManagement;
using ModdingUtils.Extensions;
using UnityEngine.Events;
using System.Linq;
using InControl;
using UnityEngine.UI;

namespace OwlCards
{
	internal class RerollButton : MonoBehaviour
	{
		bool bIsActive = false;
		bool bNeedToAddUI = false;
		int lastPickrID = -1;


		void Start()
		{
			GameModeManager.AddHook(GameModeHooks.HookPlayerPickStart, OnPlayerPickStart, GameModeHooks.Priority.VeryLow);
			GameModeManager.AddHook(GameModeHooks.HookPlayerPickEnd, OnPlayerPickEnd, GameModeHooks.Priority.VeryHigh);
        }
		void OnDestroy()
		{
			GameModeManager.RemoveHook(GameModeHooks.HookPlayerPickStart, OnPlayerPickStart);
			GameModeManager.RemoveHook(GameModeHooks.HookPlayerPickEnd, OnPlayerPickEnd);
		}

		IEnumerator OnPlayerPickStart(IGameModeHandler gm)
		{
			OwlCards.Log("PlayerPickStart called !");

			// delay UI creation as we don't know who is the pickrID yet
			bNeedToAddUI = true;

			// Bugs if PickCards() is called before full instantiation, delay as a cheap fix
			StartCoroutine(SetInputAsActive(1.0f)); // TODO adapt this to the amount of cards

			yield break;
		}

		IEnumerator SetInputAsActive(float delay)
		{
			yield return new WaitForSeconds(delay);
			bIsActive = true;
			yield break;
		}

		IEnumerator OnPlayerPickEnd(IGameModeHandler gm)
		{
			OwlCards.Log("PlayerPickEnd called !");

			bIsActive = false;
			UI.Manager.instance.RemoveFill();
			yield break;
		}

		void Update()
		{
			int pickrID = CardChoice.instance.pickrID;
			if (pickrID == -1)
				return;
			if (bNeedToAddUI)
			{
				UI.Manager.instance.BuildFillUI(Utils.GetPlayerWithID(CardChoice.instance.pickrID));
				bNeedToAddUI = false;
			}
			if (bIsActive && Extensions.CharacterStatModifiersExtension.GetAdditionalData(Utils.GetPlayerWithID(pickrID).data.stats).Soul >= 1.0f)
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
							bIsActive = false;
							Extensions.CharacterStatModifiersExtension.GetAdditionalData(Utils.GetPlayerWithID(pickrID).data.stats).Soul -= 1.0f;
							lastPickrID = pickrID;
							GameModeManager.AddHook(GameModeHooks.HookPlayerPickEnd, Reroll, GameModeHooks.Priority.Last);
							CardChoice.instance.Pick(null, true);
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
		private IEnumerator Reroll(IGameModeHandler gm)
		{

			GameModeManager.RemoveHook(GameModeHooks.HookPlayerPickEnd, Reroll);
			yield return GameModeManager.TriggerHook(GameModeHooks.HookPlayerPickStart);
			CardChoiceVisuals.instance.Show(Enumerable.Range(0, PlayerManager.instance.players.Count).Where(i => PlayerManager.instance.players[i].playerID == lastPickrID).First(), true);
			yield return CardChoice.instance.DoPick(1, lastPickrID, PickerType.Player);
			yield return new WaitForSecondsRealtime(0.1f);
			yield return GameModeManager.TriggerHook(GameModeHooks.HookPlayerPickEnd);
			yield return new WaitForSecondsRealtime(0.1f);
		}
	}
}
