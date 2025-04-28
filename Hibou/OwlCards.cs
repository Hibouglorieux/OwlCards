using UnboundLib;
using TMPro;
using UnboundLib.Cards;
using HarmonyLib;
using BepInEx;
using BepInEx.Configuration; // load and save data
using UnityEngine;
using UnityEngine.Events;
using UnboundLib.Utils.UI;
using UnboundLib.GameModes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Photon.Compression;
using System;
using System.Linq;

namespace OwlCards
{
	// These are the mods required for our mod to work
	[BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]
	// Declares our mod to Bepin
	[BepInPlugin(ModId, ModName, Version)]
	// The game our mod is associated with
	[BepInProcess("Rounds.exe")]
	public class OwlCards : BaseUnityPlugin
	{
		private const string ModId = "com.Hibou.Glorieux.OwlCards";
		public const string ModName = "OwlCards";
		public const string Version = "0.1.0"; // What version are we on (major.minor.patch)?

		public const string ModInitials = "OWL";
		private const string LogPrefix = ModName + ": ";

		public static OwlCards instance {get; private set;}

		public readonly AssetBundle Bundle = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("firstmodtest", typeof(OwlCards).Assembly);

		public ConfigEntry<float> startingRerolls;
		public ConfigEntry<float> rerollPointsPerRound;
		public ConfigEntry<float> rerollPointsPerPointWon;

		public Dictionary<int, float> rerollPerPlayer = new Dictionary<int, float>();

		void Awake()
        {
			instance = this;

			rerollPointsPerRound = Config.Bind(ModName, nameof(rerollPointsPerRound), 0.5f, "How much reroll resource is earned passively each round");
			rerollPointsPerPointWon = Config.Bind(ModName, nameof(rerollPointsPerPointWon), 0.25f, "How much reroll resource is earned passively each round");
			startingRerolls = Config.Bind(ModName, nameof(startingRerolls), 1.0f, "How many rerolls you have when a game starts");

            // Use this to call any harmony patch files your mod may have
            var harmony = new Harmony(ModId);
            harmony.PatchAll();

        }
        void Start()
        {
			//GameModeManager;
            CustomCard.BuildCard<Cards.Blahaj>();
            CustomCard.BuildCard<Cards.LetheRapide>();
            CustomCard.BuildCard<Cards.Lethe>();
            CustomCard.BuildCard<Cards.SoulLeech>();
            CustomCard.BuildCard<Cards.FeedMe>();
			UnityEngine.Debug.Log(LogPrefix + "Started");

			OptionMenu.CreateMenu();
			// instantiate logic for reroll UI
			gameObject.AddComponent<RerollButton>();
			GameModeManager.AddHook(GameModeHooks.HookGameStart, SetupPlayerResources);
			GameModeManager.AddHook(GameModeHooks.HookRoundEnd, UpdatePlayerResourcesRoundEnd);
			//GameModeManager.AddHook(GameModeHooks.HookPlayerPickStart, this.OnRoundStart);
			//ButtonClicked = ButtonClickedFunction;
        }

		private IEnumerator SetupPlayerResources(IGameModeHandler gm)
		{
			rerollPerPlayer.Clear();
			foreach (Player player in PlayerManager.instance.players.ToArray())
			{
				rerollPerPlayer.Add(player.playerID, startingRerolls.Value);
			}
			yield break;
		}
		private IEnumerator UpdatePlayerResourcesRoundEnd(IGameModeHandler gm)
		{
			int[] winningPlayersID = gm.GetRoundWinners();
			// passive gain
			foreach (int playerID in rerollPerPlayer.Keys.ToArray())
			{
				if (!winningPlayersID.Contains(playerID))
					rerollPerPlayer[playerID] += rerollPointsPerRound.Value;
			}

			// gain per point
			foreach (int pointWinner in gm.GetPointWinners())
			{
				if (!winningPlayersID.Contains(pointWinner))
					rerollPerPlayer[pointWinner] += rerollPointsPerPointWon.Value;
			}
			yield break;
		}

		void OnDestroy()
		{
			GameModeManager.RemoveHook(GameModeHooks.HookGameStart, SetupPlayerResources);
			GameModeManager.RemoveHook(GameModeHooks.HookRoundEnd, UpdatePlayerResourcesRoundEnd);
		}

		static public void Log(string msg)
		{
			/*
			UnityEngine.Debug.Log(LogPrefix + msg);
			*/
		}
    }
}
