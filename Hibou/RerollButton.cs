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
			//UnityEngine.Debug.Log(LogPrefix + gm.GameMode.Name);
			GameObject UI_Game = GameObject.Find("UI_Game");
			GameObject canvas = UI_Game.transform.GetChild(0).gameObject;
			GameObject cardChoice = GameObject.Find("Card Choice");

			//GameObject buttonAdded = MenuHandler.CreateButton("Reroll", Canvas, ButtonClicked);
			GameObject customButton = Bundle.LoadAsset<GameObject>("C_RerollButton");
			GameObject buttonAdded = Instantiate(customButton, canvas.transform);
			buttonAdded.transform.position = new Vector3(0, -18, buttonAdded.transform.position.z);
			*/


			bNeedToAddUI = true;
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
			Destroy(addedUI);
			addedUI = null;

			yield break;
		}

		private void AddUI(int pickrID)
		{
			GameObject customUI = OwlCards.instance.Bundle.LoadAsset<GameObject>("UI_RerollFill");
			GameObject UI_Game = GameObject.Find("UI_Game");

			addedUI = Instantiate(customUI, UI_Game.transform.parent);
			//added UI is a canvas which has a Border Child, which itself has a Background child, which has two childs Image and text
			Transform background = addedUI.transform.GetChild(0).GetChild(0);
			Image fillImage = background.GetChild(0).GetComponent<Image>();
			Text text = background.GetChild(1).GetComponent<Text>();

			text.text = String.Format(OwlCards.instance.rerollPerPlayer[pickrID].ToString("F2") + " Rerolls");
			float rerolls = OwlCards.instance.rerollPerPlayer[pickrID];

			fillImage.fillAmount = (rerolls > 1 ? rerolls - (Mathf.Floor(rerolls)) : rerolls);
			Image backgroundImage = background.GetComponent<Image>();
			if (rerolls < 0)
			{
				fillImage.fillOrigin = 1; // should be right
				fillImage.color = Color.red;
				fillImage.fillAmount = Mathf.Clamp(Mathf.Abs(rerolls), 0, 1);
			}
			if (rerolls > 1)
			{
				backgroundImage.color = fillImage.color;
				//fillImage.color = new Color(0f, 1f, 0.38f, 1f);
				fillImage.color = new Color(0f, 1f, 0.82f, 1f);
			}
			if (rerolls > 2)
			{
				backgroundImage.color = fillImage.color;
				fillImage.color = new Color(0f, 0.27f, 1f, 1f);
			}
			if (rerolls > 3)
			{
				backgroundImage.color = fillImage.color;
				fillImage.color = new Color(0.63f, 0f, 1f, 1f);
			}
			if (rerolls > 4)
			{
				backgroundImage.color = fillImage.color;
				fillImage.color = new Color(1f, 0.58f, 0f, 1f);
			}
		}

		void Update()
		{
			int pickrID = CardChoice.instance.pickrID;
			if (pickrID == -1)
				return;
			if (bNeedToAddUI)
			{
				AddUI(CardChoice.instance.pickrID);
				bNeedToAddUI = false;
			}
			if (bIsActive && OwlCards.instance.rerollPerPlayer[pickrID] >= 1.0f)
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
							OwlCards.instance.rerollPerPlayer[pickrID] -= 1.0f;
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
