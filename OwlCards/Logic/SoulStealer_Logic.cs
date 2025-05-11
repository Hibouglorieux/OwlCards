using ModdingUtils.MonoBehaviours;
using OwlCards.Cards;
using OwlCards.Extensions;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnboundLib;
using UnboundLib.GameModes;
using UnboundLib.Networking;
using UnityEngine;

namespace OwlCards.Logic
{
	// only runs on SELF view
	internal class SoulStealer_Logic : MonoBehaviour
	{
		Player owner;
		List<Player> playerTouched = new List<Player>();
		bool bActive = false;

		void Start()
		{
			owner = GetComponent<Player>();
			GameModeManager.AddHook(GameModeHooks.HookPointStart, OnPointStart);
			GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);
		}

		private IEnumerator OnPointStart(IGameModeHandler gm)
		{
			// reset tmp data
			playerTouched = new List<Player>();
			bActive = true;
			yield break;
		}
		private IEnumerator OnPointEnd(IGameModeHandler gm)
		{
			bActive = false;
			yield break;
		}

		void OnDestroy()
		{
			GameModeManager.RemoveHook(GameModeHooks.HookPointStart, OnPointStart);
			GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, OnPointEnd);
		}

		bool CanStealPlayer(Player otherPlayer)
		{
			return !playerTouched.Contains(otherPlayer);
		}

		private IEnumerator ClearPlayer(Player player)
		{
			yield return new WaitForSeconds(SoulStealer.delayBetweenSteal);
			playerTouched.Remove(player);
			yield break;
		}
		void Update()
		{
			if (!bActive || EscapeMenuHandler.isEscMenu)
				return;
			foreach (Player player in PlayerManager.instance.players.ToArray())
			{
				if (player.teamID != owner.teamID)
				{
					//TODO keep track and steal 0.15 then 0.10, then cap at 0.05 for balance
					if ((player.transform.position - owner.transform.position).magnitude <= (SoulStealer.baseRadius * owner.transform.localScale.x))
						if (CanStealPlayer(player))
						{
							OwlCards.Log("touched player !" + player.playerID);
							playerTouched.Add(player);
							StartCoroutine(ClearPlayer(player));
							int[] playersIds = new int[2] { owner.playerID, player.playerID };
							float[] diff = new float[2] { SoulStealer.amountOfStealToSteal, -SoulStealer.amountOfStealToSteal };
							// update his speed and request steal
							OwlCardsData.RequestUpdateSoul(playersIds, diff);
							NetworkingManager.RPC(typeof(SoulStealer_Logic), nameof(AddSoulStoleEffect_RPC), new object[] { player.playerID });
						}
				}
			}
		}

		[UnboundRPC]
		private static void AddSoulStoleEffect_RPC(int playerID)
		{
			Player player = Utils.GetPlayerWithID(playerID);
			player.gameObject.AddComponent<SoulStoleEffect>();
		}
	}

	internal class SoulStoleEffect : ReversibleEffect
	{
		private ReversibleColorEffect colorEffect = null;
		private float currentSlow = SoulStealer.baseSlow;
		private float slowDurationLeft = SoulStealer.slowDuration;
		private float effectDurationLeft = SoulStealer.delayBetweenSteal;

		public override void OnOnEnable()
		{
		}

		public override void OnStart()
		{
			characterStatModifiersModifier.movementSpeed_mult = (1.0f - currentSlow);
			characterStatModifiersModifier.jump_mult = (1.0f - currentSlow);

			colorEffect = player.gameObject.AddComponent<ReversibleColorEffect>();
			colorEffect.SetColor(Color.Lerp(new Color(1, 1, 1, 1), colorEffect.GetOriginalColorMax(), 0.0f));
			GameModeManager.AddHook(GameModeHooks.HookPointEnd, OnPointEnd);
		}

		private IEnumerator OnPointEnd(IGameModeHandler gm)
		{
			effectDurationLeft = -1;
			ClearModifiers();
			Destroy(this);
			yield break;
		}

		public override void OnUpdate()
		{
			if (EscapeMenuHandler.isEscMenu)
				return;
			slowDurationLeft -= Time.deltaTime;
			effectDurationLeft -= Time.deltaTime;

			if (effectDurationLeft <= 0)
				Destroy(this);
			if (slowDurationLeft > 0)
			{
				ClearModifiers();
				float ratio = slowDurationLeft / SoulStealer.slowDuration;
				characterStatModifiersModifier.movementSpeed_mult = (1.0f - (currentSlow * ratio));
				characterStatModifiersModifier.jump_mult = (1.0f - (currentSlow * ratio));
				ApplyModifiers();

				colorEffect.SetColor(Color.Lerp(colorEffect.GetOriginalColorMax(), Color.white, Mathf.Max(ratio, 0.3f)));
				colorEffect.ApplyColor();
			}
			else
			{
				ClearModifiers();
			}
		}

		public override void OnOnDestroy()
		{
			//TODO would be nice to add a sound effect
			colorEffect.Destroy();
		}
	}
}


