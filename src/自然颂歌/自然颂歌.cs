using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace 自然颂歌;

[BepInPlugin("Plugin.Nature", "Nature", "1.0.0")]
public class 自然颂歌 : BaseUnityPlugin
{
	public static bool 种田返还 = true;

	public static bool 叶子不腐 = true;

	public static bool 棕榈不腐 = true;

	public static bool 猴子救星 = true;

	public static bool 蜜蜂静音 = true;

	public static bool 去除减益 = true;

	public static int 南瓜时间 = 3;

	private static readonly string blank = "a913214948a94d2897846870889f4d11";

	private static Dictionary<string, CardData> card_dict = new Dictionary<string, CardData>();

	private static Dictionary<string, GameStat> stat_dict = new Dictionary<string, GameStat>();

	private static List<CardData> cropPlots = new List<CardData>();

	private static List<string> seas = new List<string> { "749dadaca7677ec4c9f54e35b4343cdb", "11c21f04e59b27244abb401684c04f0f", "6b87970979841684bb6d6a7471430798", "1d8a5586879b6aa489b6b63b1255ed59", "c3b8f83d682923643a960fe6c64d19fc", "4a02a6fed76406546ad36f6481a910a2", "64933f58900541a49bc1becbe7a852c5", "115769419d6a5144683d9d61e7cff8e9", "a0e8e31d9db3a974b8a08e41295799e0" };

	private static List<string> lights = new List<string> { "f72dda619bd111eda64e047c16184f06", "704446cba52c11edb49ec475ab46ec3f", "8ff37d3e9bd011ed97f8047c16184f06", "bc730cf89bcf11ed9292047c16184f06", "4068affda52c11edb883c475ab46ec3f" };

	private static List<string> 石磨磨坊 = new List<string> { "930d051293e911ed90a6047c16184f06", "91c9109d93e511eda299047c16184f06" };

	private static CardData utc(string uniqueID)
	{
		return UniqueIDScriptable.GetFromID<CardData>(uniqueID);
	}

	private static void 添加研磨(CardData 研磨前, CardData 研磨后, bool 是否通电, int 产量 = 1)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		if (!(((Object)(object)研磨前 == (Object)null) | ((Object)(object)研磨后 == (Object)null)))
		{
			LocalizedString val = default(LocalizedString);
			val.DefaultText = "研磨";
			val.ParentObjectID = (是否通电 ? 石磨磨坊[0] : 石磨磨坊[1]);
			val.LocalizationKey = "Guil-更多水果_榨汁";
			LocalizedString val2 = val;
			val = default(LocalizedString);
			val.DefaultText = "将材料磨成粉";
			val.ParentObjectID = (是否通电 ? 石磨磨坊[0] : 石磨磨坊[1]);
			LocalizedString val3 = val;
			CardOnCardAction val4 = new CardOnCardAction(val2, val3, (!是否通电) ? 1 : 0);
			Array.Resize(ref val4.CompatibleCards.TriggerCards, 1);
			val4.CompatibleCards.TriggerCards[0] = 研磨前;
			val4.GivenCardChanges.ModType = (CardModifications)3;
			((CardAction)val4).StackCompatible = true;
			CardsDropCollection val5 = new CardsDropCollection();
			val5.CollectionName = "产出";
			val5.CollectionWeight = 1;
			CardDrop val6 = default(CardDrop);
			val6.DroppedCard = 研磨后;
			val6.Quantity = new Vector2Int(产量, 产量);
			CardDrop[] value = (CardDrop[])(object)new CardDrop[1] { val6 };
			Traverse.Create((object)val5).Field("DroppedCards").SetValue((object)value);
			((CardAction)val4).ProducedCards = (CardsDropCollection[])(object)new CardsDropCollection[1] { val5 };
			CardData val7 = utc(石磨磨坊[1]);
			CardData val8 = utc(石磨磨坊[0]);
			if (!是否通电)
			{
				Array.Resize(ref val7.CardInteractions, val7.CardInteractions.Length + 1);
				val7.CardInteractions[val7.CardInteractions.Length - 1] = val4;
			}
			else
			{
				Array.Resize(ref val8.CardInteractions, val8.CardInteractions.Length + 1);
				val8.CardInteractions[val8.CardInteractions.Length - 1] = val4;
			}
		}
	}

	private void Awake()
	{
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Expected O, but got Unknown
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Expected O, but got Unknown
		种田返还 = ((BaseUnityPlugin)this).Config.Bind<bool>("Nature Setting", "种田返还", true, "原版作物成熟后是否会返还田地，为true时会返还，默认为true（PS：mod的如果想返还，把田的名字改为CropPlotXXX即可）").Value;
		叶子不腐 = ((BaseUnityPlugin)this).Config.Bind<bool>("Nature Setting", "叶子不腐", true, "叶子是否会腐败，为true时不会腐败，默认为true").Value;
		棕榈不腐 = ((BaseUnityPlugin)this).Config.Bind<bool>("Nature Setting", "棕榈不腐", true, "棕榈叶是否会腐败，为true时不会腐败，默认为true").Value;
		猴子救星 = ((BaseUnityPlugin)this).Config.Bind<bool>("Nature Setting", "猴子救星", true, "是否启用猴子救星（受伤的猴子、猴子朋友不咬人），为true时启用，默认为true").Value;
		蜜蜂静音 = ((BaseUnityPlugin)this).Config.Bind<bool>("Nature Setting", "蜜蜂静音", true, "是否启用蜜蜂静音，为true时启用，默认为true").Value;
		南瓜时间 = ((BaseUnityPlugin)this).Config.Bind<int>("Nature Setting", "南瓜时间", 3, "南瓜灯用一根蜡烛可以持续的时间，单位是天，默认为3天").Value;
		去除减益 = ((BaseUnityPlugin)this).Config.Bind<bool>("Nature Setting", "去除减益", true, "去除技能训练减益").Value;
		Harmony val = new Harmony(((BaseUnityPlugin)this).Info.Metadata.GUID);
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		try
		{
			HarmonyMethod val2 = new HarmonyMethod(typeof(自然颂歌).GetMethod("SomePatch"));
			MethodInfo method = typeof(GameLoad).GetMethod("LoadMainGameData", bindingAttr);
			if (method == null)
			{
				method = typeof(GameLoad).GetMethod("LoadGameData", bindingAttr);
			}
			if (method != null)
			{
				val.Patch((MethodBase)method, (HarmonyMethod)null, val2, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarningFormat("{0} {1}", new object[2]
			{
				"GameLoadLoadOptionsPostfix",
				ex.ToString()
			});
		}
		((BaseUnityPlugin)this).Logger.LogInfo((object)"Plugin 自然颂歌 is loaded!");
	}

	public static void SomePatch()
	{
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Expected O, but got Unknown
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Expected O, but got Unknown
		//IL_063c: Unknown result type (might be due to invalid IL or missing references)
		//IL_064e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0653: Unknown result type (might be due to invalid IL or missing references)
		//IL_0660: Unknown result type (might be due to invalid IL or missing references)
		//IL_0662: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < GameLoad.Instance.DataBase.AllData.Count; i++)
		{
			if (GameLoad.Instance.DataBase.AllData[i] is CardData)
			{
				Dictionary<string, CardData> dictionary = card_dict;
				string name = ((Object)GameLoad.Instance.DataBase.AllData[i]).name;
				UniqueIDScriptable obj = GameLoad.Instance.DataBase.AllData[i];
				dictionary[name] = (CardData)(object)((obj is CardData) ? obj : null);
				string name2 = ((Object)GameLoad.Instance.DataBase.AllData[i]).name;
				bool flag = name2.IndexOf("CropPlot") >= 0;
				bool flag2 = name2.EndsWith("田");
				bool flag3 = name2.EndsWith("Empty");
				bool flag4 = name2.IndexOf("better") >= 0;
				if ((flag || flag2) && !flag3 && !flag4)
				{
					List<CardData> list = cropPlots;
					UniqueIDScriptable obj2 = GameLoad.Instance.DataBase.AllData[i];
					list.Add((CardData)(object)((obj2 is CardData) ? obj2 : null));
				}
			}
			if (GameLoad.Instance.DataBase.AllData[i] is GameStat)
			{
				Dictionary<string, GameStat> dictionary2 = stat_dict;
				string name3 = ((Object)GameLoad.Instance.DataBase.AllData[i]).name;
				UniqueIDScriptable obj3 = GameLoad.Instance.DataBase.AllData[i];
				dictionary2[name3] = (GameStat)(object)((obj3 is GameStat) ? obj3 : null);
			}
		}
		if (种田返还)
		{
			if (card_dict.TryGetValue("RicePaddy", out var value) && card_dict.TryGetValue("RicePaddyEmpty", out var value2))
			{
				value.Progress.OnFull.ReceivingCardChanges.ModType = (CardModifications)2;
				value.Progress.OnFull.ReceivingCardChanges.TransformInto = value2;
			}
			if (card_dict.TryGetValue("CropPlotEmpty", out var value3))
			{
				foreach (CardData cropPlot in cropPlots)
				{
					DurabilityStat progress = cropPlot.Progress;
					if (progress != null && progress.OnFull?.ReceivingCardChanges.ModType == (CardModifications?)3)
					{
						cropPlot.Progress.OnFull.ReceivingCardChanges.ModType = (CardModifications)2;
						cropPlot.Progress.OnFull.ReceivingCardChanges.TransformInto = value3;
					}
				}
			}
		}
		if (card_dict.TryGetValue("PalmFronds", out var value4) && 棕榈不腐)
		{
			DurabilityStat spoilageTime = new DurabilityStat(false, 0);
			value4.SpoilageTime = spoilageTime;
		}
		if (card_dict.TryGetValue("LeavesFresh", out var value5) && 叶子不腐)
		{
			DurabilityStat spoilageTime2 = new DurabilityStat(false, 0);
			value5.SpoilageTime = spoilageTime2;
		}
		if (猴子救星)
		{
			if (card_dict.TryGetValue("MacaqueWounded", out var value6))
			{
				((CardAction)value6.CardInteractions[0]).ProducedCards[1].CollectionWeight = 0;
				((CardAction)value6.CardInteractions[1]).ProducedCards[1].CollectionWeight = 0;
				((CardAction)value6.DismantleActions[1]).ProducedCards[1].CollectionWeight = 0;
			}
			if (card_dict.TryGetValue("MacaqueFriend", out var value7))
			{
				List<CardsDropCollection> list2 = new List<CardsDropCollection>(((CardAction)value7.CardInteractions[0]).ProducedCards);
				list2.RemoveAt(1);
				((CardAction)value7.CardInteractions[0]).ProducedCards = list2.ToArray();
				list2 = new List<CardsDropCollection>(((CardAction)value7.CardInteractions[1]).ProducedCards);
				list2.RemoveAt(1);
				((CardAction)value7.CardInteractions[1]).ProducedCards = list2.ToArray();
				list2 = new List<CardsDropCollection>(((CardAction)value7.CardInteractions[6]).ProducedCards);
				list2.RemoveAt(1);
				((CardAction)value7.CardInteractions[6]).ProducedCards = list2.ToArray();
				list2 = new List<CardsDropCollection>(((CardAction)value7.DismantleActions[0]).ProducedCards);
				list2.RemoveAt(1);
				((CardAction)value7.DismantleActions[0]).ProducedCards = list2.ToArray();
			}
		}
		if (蜜蜂静音)
		{
			CardData fromID = UniqueIDScriptable.GetFromID<CardData>("0a2f6fa8b61d1ff4c8796ea73c78c114");
			if ((Object)(object)fromID != (Object)null)
			{
				fromID.Ambience.BackgroundSound = null;
			}
		}
		CardData fromID2 = UniqueIDScriptable.GetFromID<CardData>(blank);
		if (Object.op_Implicit((Object)(object)fromID2))
		{
			for (int j = 0; j < seas.Count; j++)
			{
				CardData fromID3 = UniqueIDScriptable.GetFromID<CardData>(seas[j]);
				if (Object.op_Implicit((Object)(object)fromID3))
				{
					Array.Resize(ref fromID3.CardInteractions, fromID3.CardInteractions.Length + 1);
					fromID3.CardInteractions[fromID3.CardInteractions.Length - 1] = fromID2.CardInteractions[0];
				}
			}
		}
		foreach (string light in lights)
		{
			CardData val = utc(light);
			if ((Object)(object)val != (Object)null)
			{
				((OptionalFloatValue)val.FuelCapacity).FloatValue = 南瓜时间 * 96;
				val.FuelCapacity.MaxValue = 南瓜时间 * 96;
			}
		}
		CardData val2 = utc("ceceb95e1f68491cb264b697b8e88dc9");
		if ((Object)(object)val2 != (Object)null)
		{
			foreach (string item in 石磨磨坊)
			{
				CardData val3 = utc(item);
				if ((Object)(object)val3 != (Object)null)
				{
					CardDrop val4 = default(CardDrop);
					val4.DroppedCard = val2;
					val4.Quantity = new Vector2Int(1, 1);
					CardDrop[] value8 = (CardDrop[])(object)new CardDrop[1] { val4 };
					Traverse.Create((object)((CardAction)val3.CardInteractions[2]).ProducedCards[0]).Field("DroppedCards").SetValue((object)value8);
				}
			}
		}
		CardData 研磨前 = utc("40bdee90a1e411ed85a0c475ab46ec3f");
		CardData 研磨后 = utc("4159826e61c04b728fa63c332f4e1d4b");
		添加研磨(研磨前, 研磨后, 是否通电: true, 2);
		添加研磨(研磨前, 研磨后, 是否通电: false, 2);
		if (!去除减益)
		{
			return;
		}
		foreach (KeyValuePair<string, GameStat> item2 in stat_dict)
		{
			if (item2.Key.IndexOf("Skill") > -1)
			{
				item2.Value.StalenessMultiplier = 1f;
				item2.Value.MaxStalenessStack = 0;
			}
		}
	}
}
