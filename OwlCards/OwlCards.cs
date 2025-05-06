using HarmonyLib;
using BepInEx;
using BepInEx.Configuration; // load and save data
using UnityEngine;
using UnboundLib;
using UnboundLib.Cards;
using UnboundLib.GameModes;
using UnboundLib.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using OwlCards.Cards;
using OwlCards.Extensions;
using Photon.Pun;
using UnboundLib.Utils;

namespace OwlCards
{
	// These are the mods required for our mod to work
	[BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("root.rarity.lib", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("com.CrazyCoders.Rounds.RarityBundle", BepInDependency.DependencyFlags.HardDependency)]

	[BepInDependency("com.willuwontu.rounds.managers", BepInDependency.DependencyFlags.SoftDependency)]
	//TODO Add pickncards dependency for dynamic/temp handsize increase/decrease
	//[BepInDependency("pykess.rounds.plugins.pickncards", BepInDependency.DependencyFlags.SoftDependency)]

	// Declares our mod to Bepin
	[BepInPlugin(ModId, ModName, Version)]
	// The game our mod is associated with
	[BepInProcess("Rounds.exe")]
	public class OwlCards : BaseUnityPlugin
	{
		private const string ModId = "com.HibouGlorieux.Rounds.OwlCards";
		public const string ModName = "OwlCards";
		public const string Version = "0.1.0"; // What version are we on (major.minor.patch)?

		public const string ModInitials = "OWL";
		private const string LogPrefix = ModName + ": ";

		public static OwlCards instance { get; private set; }

		public readonly AssetBundle Bundle = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("owlcards", typeof(OwlCards).Assembly);

		public ConfigEntry<float> soulOnGameStart;
		public ConfigEntry<float> soulGainedPerRound;
		public ConfigEntry<float> soulGainedPerPointWon;

		public ConfigEntry<float> rerollSoulCost;
		public ConfigEntry<float> extraPickSoulCost;
		public ConfigEntry<bool> bExtraPickActive;

		public bool bCurseActivated { get; private set; }
		//public bool bPickNCards { get; private set;}

		private List<int[]> pointWinnersID = new List<int[]>();

		void Awake()
		{
			instance = this;

			bCurseActivated = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.willuwontu.rounds.managers");
			//bPickNCards = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("pykess.rounds.plugins.pickncards");

			soulGainedPerRound = Config.Bind(ModName, nameof(soulGainedPerRound), 0.5f, "How much soul resource is earned passively each round");
			soulGainedPerPointWon = Config.Bind(ModName, nameof(soulGainedPerPointWon), 0.25f, "How much soul resource is earned passively each round");
			soulOnGameStart = Config.Bind(ModName, nameof(soulOnGameStart), 1.0f, "How much soul you have when a game starts");

			rerollSoulCost = Config.Bind(ModName, nameof(rerollSoulCost), 1.0f, "how much soul does it cost to reroll");
			extraPickSoulCost = Config.Bind(ModName, nameof(extraPickSoulCost), 4.0f, "how much soul does it cost to do an extra pick");

			bExtraPickActive = Config.Bind(ModName, nameof(bExtraPickActive), true, "Enable extra roll");

			// Use this to call any harmony patch files your mod may have
			var harmony = new Harmony(ModId);
			harmony.PatchAll();

		}
		void Start()
		{
			BuildCards();

			ModdingUtils.Utils.Cards.instance.AddCardValidationFunction(OwlCardValidation);
			// Spawns Menu
			OptionMenu.CreateMenu();

			// spawn specialized component
			gameObject.AddComponent<Reroll>();
			gameObject.AddComponent<UI.Manager>();

			Unbound.RegisterHandshake(ModId, OnHandshakeCompleted);

			GameModeManager.AddHook(GameModeHooks.HookRoundEnd, UpdatePlayerResourcesRoundEnd);
			GameModeManager.AddHook(GameModeHooks.HookPointEnd, TrackPointWinners);
			GameModeManager.AddHook(GameModeHooks.HookGameStart, OnGameStart, GameModeHooks.Priority.High);
		}

		private IEnumerator OnGameStart(IGameModeHandler gm)
		{
			OwlCardCategory.InitializeRarityCategories();
			Card[] cards = CardManager.cards.Values.ToArray();

			//Add rarityCategories 
            foreach (var card in cards)
            {
				if (!card.cardInfo.categories.Contains(OwlCardCategory.rarityCategories[card.cardInfo.rarity]))
				{
					card.cardInfo.categories = card.cardInfo.categories.Append(OwlCardCategory.rarityCategories[card.cardInfo.rarity]).ToArray();
				}
            }
            yield break;
		}

		private void OnHandshakeCompleted()
		{
			if (PhotonNetwork.IsMasterClient)
				NetworkingManager.RPC_Others(typeof(OwlCards), nameof(SyncSettings),
					new object[] {
						new float[]
						{
							soulGainedPerRound.Value,
							soulGainedPerPointWon.Value,
							soulOnGameStart.Value,
							rerollSoulCost.Value,
							extraPickSoulCost.Value
						}
					});
		}

		[UnboundRPC]
		private static void SyncSettings(float[] settings)
		{
			instance.soulGainedPerRound.Value = settings[0];
			instance.soulGainedPerPointWon.Value = settings[1];
			instance.soulOnGameStart.Value = settings[2];
			instance.rerollSoulCost.Value = settings[3];
			instance.extraPickSoulCost.Value = settings[4];
		}

		private IEnumerator TrackPointWinners(IGameModeHandler gm)
		{
			if (!(PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient))
				yield break;

			pointWinnersID.Add((int[])gm.GetPointWinners().Clone());
		}

		private bool OwlCardValidation(Player player, CardInfo cardInfo)
		{
			if (cardInfo.categories.Contains(OwlCardCategory.modCategory))
				if (AOwlCard.conditions.ContainsKey(cardInfo.cardName))
				{
					return AOwlCard.conditions[cardInfo.cardName]
						(Extensions.CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).Soul);
				}
			return true;
		}
		private void BuildCards()
		{
			// TODO what to do actually...?
			//CustomCard.BuildCard<Cards.Blahaj>();
			//CustomCard.BuildCard<Cards.LetheRapide>();
			//CustomCard.BuildCard<Cards.Lethe>();

			// Debug/Test

#if DEBUG
			//CustomCard.BuildCard<Cards.Soul>();
			//CustomCard.BuildCard<Cards.Soulless>();
#endif

			// Should be ok
			CustomCard.BuildCard<Cards.SoulLeech>();
			CustomCard.BuildCard<Cards.FeedMe>();
			CustomCard.BuildCard<Cards.LastHitter>();
			CustomCard.BuildCard<Cards.Resolution>();

			// TODO need to be tested
			//CustomCard.BuildCard<Cards.SoulExhaustion>();

			//Random/give other cards
			CustomCard.BuildCard<Cards.FunKiller>();
			//gives a legendary card but gives a reroll to everyone else
			CustomCard.BuildCard<Cards.CorruptedPower>();
			//gives a random low card but you earn soul
			CustomCard.BuildCard<Cards.Dedication>();
			//trade soul for random strong card
			CustomCard.BuildCard<Cards.Bargain>();
			//gives a random strong card but gives soul to others
			CustomCard.BuildCard<Cards.OpenBar>();
			//remove X soul and instead have a good draw
			CustomCard.BuildCard<Cards.Pious>();

			// trade random curse for some soul
			// todo later with softdependancy of curses
			//CustomCard.BuildCard<Cards.Soul>();

			// base + move speed, touching a player slows him + steal soul
			//CustomCard.BuildCard<Cards.Soul>();


		}

		private IEnumerator UpdatePlayerResourcesRoundEnd(IGameModeHandler gm)
		{
			if (!(PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient))
				yield break;

			int[] winningPlayersID = gm.GetRoundWinners();

			Dictionary<int, float> newSoulValues = new Dictionary<int, float>();
			// passive gain
			foreach (Player player in PlayerManager.instance.players.ToArray())
			{
				if (!winningPlayersID.Contains(player.playerID))
				{
					newSoulValues.Add(player.playerID,
						CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).Soul + soulGainedPerRound.Value
						);
				}
			}

			// the last added value is BAD as gamemode doesn't update the data when round is won
			pointWinnersID.RemoveAt(pointWinnersID.Count - 1);
			// gain per point won
			float soulEarnedWithPoints = 0;
			foreach (int[] pointWinners in pointWinnersID)
			{
				foreach (int pointWinner in pointWinners)
				{
					if (!winningPlayersID.Contains(pointWinner))
					{
						Player player = Utils.GetPlayerWithID(pointWinner);
						if (newSoulValues.ContainsKey(player.playerID))
						{
							newSoulValues[player.playerID] += soulGainedPerPointWon.Value;
						}
						else
						{
							newSoulValues.Add(player.playerID,
								CharacterStatModifiersExtension.GetAdditionalData(player.data.stats).Soul + soulGainedPerPointWon.Value
								);
						}
						soulEarnedWithPoints += soulGainedPerPointWon.Value;
					}
				}
			}
			var pairs = newSoulValues.ToArray();
			int[] playerIDs = pairs.Select(p => p.Key).ToArray();
			float[] souls = pairs.Select(p => p.Value).ToArray();
			OwlCardsData.UpdateSoul(playerIDs, souls);

			pointWinnersID.Clear();
			Log("End of round total points earned with Points won: " + soulEarnedWithPoints);
		}

		void OnDestroy()
		{
			GameModeManager.RemoveHook(GameModeHooks.HookGameStart, OnGameStart);
			GameModeManager.RemoveHook(GameModeHooks.HookRoundEnd, UpdatePlayerResourcesRoundEnd);
			GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, TrackPointWinners);
		}

		static public void Log(string msg)
		{
#if DEBUG
			UnityEngine.Debug.Log(LogPrefix + msg);
#endif
		}
	}
	internal class OwlCardsException : Exception
	{
		public OwlCardsException(string msg) : base(msg){}
	}
}
