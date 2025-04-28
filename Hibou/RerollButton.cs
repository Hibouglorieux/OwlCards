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

namespace OwlCards
{
	internal class RerollButton : MonoBehaviour
	{
		AssetBundle Bundle { get { return OwlCards.Bundle; } }

		bool bIsActive = false;
		int lastPickrID = -1;
		private GameObject addedUI;

		void Start()
		{
			OwlCards.Log("RerollButton created and gameStartHook registered");
			//GameModeManager.AddHook(GameModeHooks.HookGameStart, OnGameStart);
			//GameModeManager.AddHook(GameModeHooks.HookGameEnd, OnGameEnd);

			GameModeManager.AddHook(GameModeHooks.HookPlayerPickStart, OnPlayerPickStart, GameModeHooks.Priority.VeryLow);
			GameModeManager.AddHook(GameModeHooks.HookPlayerPickEnd, OnPlayerPickEnd, GameModeHooks.Priority.VeryHigh);
			//GameModeManager.AddHook(GameModeHooks.HookPlayerPickEnd, Reroll, GameModeHooks.Priority.Last);

        }


		private IEnumerator OnPickStart(IGameModeHandler gm)
		{
			OwlCards.Log("PICK start called !");
			yield break;
		}
		IEnumerator OnPlayerPickStart(IGameModeHandler gm)
		{
			OwlCards.Log("PlayerPickStart called !");
			/*
			/*
			//UnityEngine.Debug.Log(LogPrefix + gm.GameMode.Name);
			GameObject UI_Game = GameObject.Find("UI_Game");
			GameObject canvas = UI_Game.transform.GetChild(0).gameObject;
			GameObject cardChoice = GameObject.Find("Card Choice");

			//GameObject buttonAdded = MenuHandler.CreateButton("Reroll", Canvas, ButtonClicked);
			GameObject customButton = Bundle.LoadAsset<GameObject>("C_RerollButton");
			GameObject buttonAdded = Instantiate(customButton, canvas.transform);
			buttonAdded.transform.position = new Vector3(0, -18, buttonAdded.transform.position.z);
			*/

			GameObject customUI = Bundle.LoadAsset<GameObject>("UI_RerollFill");
			GameObject UI_Game = GameObject.Find("UI_Game");
			addedUI = Instantiate(customUI, UI_Game.transform.parent);
			bIsActive = true;

			yield break;
		}

		IEnumerator OnPlayerPickEnd(IGameModeHandler gm)
		{
			OwlCards.Log("PlayerPickEnd called !");

			bIsActive = false;
			Destroy(addedUI);
			addedUI = null;

			yield break;
		}

		void Update()
		{
			if (bIsActive && CardChoice.instance.pickrID != -1)
			{
				PlayerActions[] watchedActions = null;
				watchedActions = PlayerManager.instance.GetActionsFromPlayer(CardChoice.instance.pickrID);
				if (watchedActions != null)
				{
					for (int i = 0; i < watchedActions.Length; i++)
					{
						if (watchedActions[i] == null)
							continue;

						if (((OneAxisInputControl)watchedActions[i].Block).WasPressed)
						{
							lastPickrID = CardChoice.instance.pickrID;
							OnButtonClicked();
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

		private void OnButtonClicked()
		{
			OwlCards.Log("Button has been clicked");
			GameModeManager.AddHook(GameModeHooks.HookPlayerPickEnd, Reroll, GameModeHooks.Priority.Last);
			CardChoice.instance.Pick(null, true);
			//CardChoice.instance.DoPick(1, pickrID, PickerType.Player);
			//Extensions.CharacterStatModifiersExtension;
		}
		private IEnumerator OnGameStart(IGameModeHandler gm)
		{
			//GameModeManager.AddHook(GameModeHooks.HookPlayerPickStart, OnPlayerPickStart, GameModeHooks.Priority.Last);
			//GameModeManager.AddHook(GameModeHooks.HookPickStart, OnPickStart);
			//GameModeManager.AddHook(GameModeHooks.HookPlayerPickEnd, OnPlayerPickEnd);
			yield break;
		}

		private IEnumerator OnGameEnd(IGameModeHandler gm)
		{
			//GameModeManager.RemoveHook(GameModeHooks.HookPlayerPickStart, OnPlayerPickStart);
			//GameModeManager.RemoveHook(GameModeHooks.HookPlayerPickEnd, OnPlayerPickEnd);
			yield break;
		}

		void OnDestroy()
		{
			GameModeManager.RemoveHook(GameModeHooks.HookGameStart, OnGameStart);
			GameModeManager.RemoveHook(GameModeHooks.HookGameStart, OnGameEnd);
		}
	}
}
