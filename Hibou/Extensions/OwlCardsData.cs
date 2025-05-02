using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnboundLib.Networking;
using UnboundLib;

namespace OwlCards.Extensions
{
	[Serializable]
	public class CharacterStatModifiersOwlCardsData
	{
		private float _soul;
		public float Soul
		{
			get { return _soul; }
			private set
			{
				if (_soul != value)
				{
					_soul = value;
					soulChanged?.Invoke(value);
				}
			}
		}
		public event Action<float> soulChanged;

		private int _rerolls;
		public int Rerolls { get => _rerolls; }

		[UnboundRPC]
		private static void UpdateSoul_RPC(int[] playersIDs, float[] newSoulValues)
		{
			for (int i = 0; i < playersIDs.Length; i++)
			{
				int playerID = playersIDs[i];
				float newSoulValue = newSoulValues[i];

				CharacterStatModifiersOwlCardsData data = CharacterStatModifiersExtension.GetAdditionalData(Utils.GetPlayerWithID(playerID).data.stats);
				data.Soul = newSoulValue;
			}
		}

		public static void UpdateSoul(int[] playersIDs, float[] newSoulValues)
		{
			object[] obj = { playersIDs, newSoulValues };
			NetworkingManager.RPC(typeof(CharacterStatModifiersOwlCardsData), nameof(UpdateSoul_RPC), obj);
		}
		public static void UpdateSoul(int playerID, float newSoulValue)
		{
			UpdateSoul(new int[] { playerID }, new float[] { newSoulValue });
		}


		public CharacterStatModifiersOwlCardsData()
		{
			_soul = OwlCards.instance.soulOnGameStart.Value;
			_rerolls = 0;
		}
	}

	public static class CharacterStatModifiersExtension
	{
		public static readonly ConditionalWeakTable<CharacterStatModifiers, CharacterStatModifiersOwlCardsData> data =
			new ConditionalWeakTable<CharacterStatModifiers, CharacterStatModifiersOwlCardsData>();

		public static CharacterStatModifiersOwlCardsData GetAdditionalData(this CharacterStatModifiers statModifiers)
		{
			return data.GetOrCreateValue(statModifiers);
		}

		public static void AddData(this CharacterStatModifiers statModifiers, CharacterStatModifiersOwlCardsData value)
		{
			try
			{
				data.Add(statModifiers, value);
			}
			catch (Exception) { }
		}
	}

	[HarmonyPatch(typeof(CharacterStatModifiers), "ResetStats")]
	class CharacterStatModifiersPatchResetStats
	{
		private static void Prefix(CharacterStatModifiers __instance)
		{
			CharacterStatModifiersOwlCardsData additionalData = __instance.GetAdditionalData();

			if (additionalData.Soul != OwlCards.instance.soulOnGameStart.Value)
			{
				var soulPropertySetter = AccessTools.PropertySetter(typeof(CharacterStatModifiersOwlCardsData), "Soul");
				soulPropertySetter.Invoke(additionalData, new object[] {OwlCards.instance.soulOnGameStart.Value});
			}

			var rerollsField = AccessTools.Field(typeof(CharacterStatModifiersOwlCardsData), "_rerolls");
			rerollsField.SetValue(additionalData, 0);
		}
	}
}
