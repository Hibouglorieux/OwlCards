using System;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace OwlCards.Extensions
{
	[Serializable]
	public class CharacterStatModifiersOwlCardsData
	{
		private float _soul;
		private int _rerolls;
		public int Rerolls { get => _rerolls; }
		public float Soul { get { return _soul; } set
			{
				if (_soul != value)
				{
					_soul = value;
					soulChanged?.Invoke(value);
				}
			}
		}

		public event Action<float> soulChanged;

		public CharacterStatModifiersOwlCardsData()
		{
			Soul = OwlCards.instance.soulOnGameStart.Value;
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

				additionalData.Soul = OwlCards.instance.soulOnGameStart.Value;

				var rerollsField = AccessTools.Field(typeof(CharacterStatModifiersOwlCardsData), "_rerolls");
				rerollsField.SetValue(additionalData, 0);
			}
		}
}
