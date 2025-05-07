using System;
using System.Linq;
using BepInEx;
using System.Reflection;
using HarmonyLib;

namespace OwlCards.Dependencies
{
	internal class CurseHandler
	{
		static object _curseManagerInstance = null;
		static MethodInfo _curseCategoryMethod;
		static MethodInfo _curseColor;
		static MethodInfo _isPickingCurse;
		static MethodInfo _cursePlayer;
		static MethodInfo _getRaw;

		public static void Init(PluginInfo pluginInfo)
		{
			var assembly = pluginInfo.Instance.GetType().Assembly;
			Type curseManagerType = assembly.GetType("WillsWackyManagers.Utils.CurseManager");
			var instanceProperty = curseManagerType.GetProperty("instance", BindingFlags.Static | BindingFlags.Public);
			Type curseTheme = assembly.GetType("WillsWackyManagers.Utils.CurseManager+CurseThemes");
			_curseManagerInstance = instanceProperty.GetValue(null);
			_curseCategoryMethod = AccessTools.PropertyGetter(curseManagerType, "curseCategory");
			_curseColor = AccessTools.PropertyGetter(curseTheme, "CursedPink");
			_isPickingCurse = AccessTools.PropertyGetter(curseManagerType, "CursePick");
			_cursePlayer = AccessTools.Method(curseManagerType, "CursePlayer", new Type[] { typeof(Player), typeof(Action<CardInfo>) });
			_getRaw = AccessTools.Method(curseManagerType, "GetRaw");

#if DEBUG
			OwlCards.Log("Curse test CurseCategory: " + CurseCategory.name);
			OwlCards.Log("Curse test CursedPink: " + CursedPink.ToString());
			OwlCards.Log("Curse test IsPickingCurse: " + IsPickingCurse.ToString());
			OwlCards.Log("Curse test bCurseAvailable: " + bCurseAvailable);
#endif
		}

		public static CardCategory CurseCategory => (CardCategory)_curseCategoryMethod.Invoke(_curseManagerInstance, new object[] { });

		public static CardThemeColor.CardThemeColorType CursedPink => (CardThemeColor.CardThemeColorType)_curseColor.Invoke(null, new object[] { });
		
		public static bool IsPickingCurse => (bool)_isPickingCurse.Invoke(_curseManagerInstance, new object[] { });

		public static void CursePlayer(Player player, Action<CardInfo> callback)
		{
			_cursePlayer.Invoke(_curseManagerInstance, new object[] { player, callback});
		}

		public static bool bCurseAvailable => ((CardInfo[])_getRaw.Invoke(_curseManagerInstance, new object[] { false})).Count() > 0;
	}
}
