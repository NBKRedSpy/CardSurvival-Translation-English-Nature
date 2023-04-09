using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace 自然颂歌;

[BepInPlugin("Plugin.Nature", "Nature", "2.0.0")]
public class 自然颂歌 : BaseUnityPlugin
{
    /// <summary>
    /// Farming
    /// </summary>
    public static bool 种田返还 = true;

    /// <summary>
    /// Leaves are not rotten
    /// </summary>
    public static bool 叶子不腐 = true;

    /// <summary>
    /// Palm is not rotten
    /// </summary>
    public static bool 棕榈不腐 = true;

    /// <summary>
    /// Monkey savior
    /// </summary>
    public static bool 猴子救星 = true;

    /// <summary>
    /// bee mute 
    /// </summary>
    public static bool 蜜蜂静音 = true;

    /// <summary>
    /// remove debuff
    /// </summary>
    public static bool 去除减益 = true;

    /// <summary>
    /// pumpkin time
    /// </summary>
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
		if (!((研磨前 == null) | (研磨后 == null)))
		{
			LocalizedString localizedString = default(LocalizedString);
			localizedString.DefaultText = "研磨";
			localizedString.ParentObjectID = (是否通电 ? 石磨磨坊[0] : 石磨磨坊[1]);
			localizedString.LocalizationKey = "Guil-更多水果_榨汁";
			LocalizedString localizedString2 = localizedString;

			localizedString = default(LocalizedString);
			localizedString.DefaultText = "将材料磨成粉";
			localizedString.LocalizationKey = "GrindingIntoPowder";
			localizedString.ParentObjectID = (是否通电 ? 石磨磨坊[0] : 石磨磨坊[1]);
			LocalizedString desc = localizedString;
			CardOnCardAction cardOnCardAction = new CardOnCardAction(localizedString2, desc, (!是否通电) ? 1 : 0);
			Array.Resize(ref cardOnCardAction.CompatibleCards.TriggerCards, 1);
			cardOnCardAction.CompatibleCards.TriggerCards[0] = 研磨前;
			cardOnCardAction.GivenCardChanges.ModType = CardModifications.Destroy;
			cardOnCardAction.StackCompatible = true;
			CardsDropCollection cardsDropCollection = new CardsDropCollection();
			cardsDropCollection.CollectionName = "产出";
			cardsDropCollection.CollectionWeight = 1;
			CardDrop cardDrop = default(CardDrop);
			cardDrop.DroppedCard = 研磨后;
			cardDrop.Quantity = new Vector2Int(产量, 产量);
			CardDrop[] value = new CardDrop[1] { cardDrop };
			Traverse.Create(cardsDropCollection).Field("DroppedCards").SetValue(value);
			cardOnCardAction.ProducedCards = new CardsDropCollection[1] { cardsDropCollection };
			CardData cardData = utc(石磨磨坊[1]);
			CardData cardData2 = utc(石磨磨坊[0]);
			if (!是否通电)
			{
				Array.Resize(ref cardData.CardInteractions, cardData.CardInteractions.Length + 1);
				cardData.CardInteractions[cardData.CardInteractions.Length - 1] = cardOnCardAction;
			}
			else
			{
				Array.Resize(ref cardData2.CardInteractions, cardData2.CardInteractions.Length + 1);
				cardData2.CardInteractions[cardData2.CardInteractions.Length - 1] = cardOnCardAction;
			}
		}
	}

	private void Awake()
	{
		种田返还 = base.Config.Bind("Nature Setting", "种田返还", defaultValue: true, "原版作物成熟后是否会返还田地，为true时会返还，默认为true（PS：mod的如果想返还，把田的名字改为CropPlotXXX即可）. [Whether the original crop will be returned to the field after maturity, true will be returned, the default is true (PS: mod if you want to return, the name of the field can be changed to CropPlotXXX).]").Value;
		叶子不腐 = base.Config.Bind("Nature Setting", "叶子不腐", defaultValue: true, "叶子是否会腐败，为true时不会腐败，默认为true [If or not the leaf will rot, true will not rot, default is true ]").Value;
		棕榈不腐 = base.Config.Bind("Nature Setting", "棕榈不腐", defaultValue: true, "棕榈叶是否会腐败，为true时不会腐败，默认为true [ If or not the palm leaves will rot, true will not rot, default is true]").Value;
		猴子救星 = base.Config.Bind("Nature Setting", "猴子救星", defaultValue: true, "是否启用猴子救星（受伤的猴子、猴子朋友不咬人），为true时启用，默认为true [Whether to enable monkey savior (injured monkeys, monkey friends don't bite), true when enabled, default is true]").Value;
		蜜蜂静音 = base.Config.Bind("Nature Setting", "蜜蜂静音", defaultValue: true, "是否启用蜜蜂静音，为true时启用，默认为true [Whether to enable bee mute, true when enabled, default is true]").Value;
		南瓜时间 = base.Config.Bind("Nature Setting", "南瓜时间", 3, "南瓜灯用一根蜡烛可以持续的时间，单位是天，默认为3天 [Pumpkin lantern with a candle can last the time in days, the default is 3 days]").Value;
		去除减益 = base.Config.Bind("Nature Setting", "去除减益", defaultValue: true, "去除技能训练减益 [Removal of skill training deductions]").Value;
		Harmony harmony = new Harmony(base.Info.Metadata.GUID);
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		try
		{
			HarmonyMethod postfix = new HarmonyMethod(typeof(自然颂歌).GetMethod("SomePatch"));
			MethodInfo method = typeof(GameLoad).GetMethod("LoadMainGameData", bindingAttr);
			if (method == null)
			{
				method = typeof(GameLoad).GetMethod("LoadGameData", bindingAttr);
			}
			if (method != null)
			{
				harmony.Patch(method, null, postfix);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarningFormat("{0} {1}", "GameLoadLoadOptionsPostfix", ex.ToString());
		}
		base.Logger.LogInfo("Plugin 自然颂歌 is loaded!");
	}

	public static void SomePatch()
	{
		for (int i = 0; i < GameLoad.Instance.DataBase.AllData.Count; i++)
		{
			if (GameLoad.Instance.DataBase.AllData[i] is CardData)
			{
				card_dict[GameLoad.Instance.DataBase.AllData[i].name] = GameLoad.Instance.DataBase.AllData[i] as CardData;
				string text = GameLoad.Instance.DataBase.AllData[i].name;
				bool flag = text.IndexOf("CropPlot") >= 0;
				bool flag2 = text.EndsWith("田");
				bool flag3 = text.EndsWith("Empty");
				bool flag4 = text.IndexOf("better") >= 0;
				if ((flag || flag2) && !flag3 && !flag4)
				{
					cropPlots.Add(GameLoad.Instance.DataBase.AllData[i] as CardData);
				}
			}
			if (GameLoad.Instance.DataBase.AllData[i] is GameStat)
			{
				stat_dict[GameLoad.Instance.DataBase.AllData[i].name] = GameLoad.Instance.DataBase.AllData[i] as GameStat;
			}
		}
		if (种田返还)
		{
			if (card_dict.TryGetValue("RicePaddy", out var value) && card_dict.TryGetValue("RicePaddyEmpty", out var value2))
			{
				value.Progress.OnFull.ReceivingCardChanges.ModType = CardModifications.Transform;
				value.Progress.OnFull.ReceivingCardChanges.TransformInto = value2;
			}
			if (card_dict.TryGetValue("CropPlotEmpty", out var value3))
			{
				foreach (CardData cropPlot in cropPlots)
				{
					DurabilityStat progress = cropPlot.Progress;
					if (progress != null && progress.OnFull?.ReceivingCardChanges.ModType == CardModifications.Destroy)
					{
						cropPlot.Progress.OnFull.ReceivingCardChanges.ModType = CardModifications.Transform;
						cropPlot.Progress.OnFull.ReceivingCardChanges.TransformInto = value3;
					}
				}
			}
		}
		if (card_dict.TryGetValue("PalmFronds", out var value4) && 棕榈不腐)
		{
			DurabilityStat spoilageTime = new DurabilityStat(_Active: false, 0);
			value4.SpoilageTime = spoilageTime;
		}
		if (card_dict.TryGetValue("LeavesFresh", out var value5) && 叶子不腐)
		{
			DurabilityStat spoilageTime2 = new DurabilityStat(_Active: false, 0);
			value5.SpoilageTime = spoilageTime2;
		}
		if (猴子救星)
		{
			if (card_dict.TryGetValue("MacaqueWounded", out var value6))
			{
				value6.CardInteractions[0].ProducedCards[1].CollectionWeight = 0;
				value6.CardInteractions[1].ProducedCards[1].CollectionWeight = 0;
				value6.DismantleActions[1].ProducedCards[1].CollectionWeight = 0;
			}
			if (card_dict.TryGetValue("MacaqueFriend", out var value7))
			{
				List<CardsDropCollection> list = new List<CardsDropCollection>(value7.CardInteractions[0].ProducedCards);
				list.RemoveAt(1);
				value7.CardInteractions[0].ProducedCards = list.ToArray();
				list = new List<CardsDropCollection>(value7.CardInteractions[1].ProducedCards);
				list.RemoveAt(1);
				value7.CardInteractions[1].ProducedCards = list.ToArray();
				list = new List<CardsDropCollection>(value7.CardInteractions[6].ProducedCards);
				list.RemoveAt(1);
				value7.CardInteractions[6].ProducedCards = list.ToArray();
				list = new List<CardsDropCollection>(value7.DismantleActions[0].ProducedCards);
				list.RemoveAt(1);
				value7.DismantleActions[0].ProducedCards = list.ToArray();
			}
		}
		if (蜜蜂静音)
		{
			CardData fromID = UniqueIDScriptable.GetFromID<CardData>("0a2f6fa8b61d1ff4c8796ea73c78c114");
			if (fromID != null)
			{
				fromID.Ambience.BackgroundSound = null;
			}
		}
		CardData fromID2 = UniqueIDScriptable.GetFromID<CardData>(blank);
		if ((bool)fromID2)
		{
			for (int j = 0; j < seas.Count; j++)
			{
				CardData fromID3 = UniqueIDScriptable.GetFromID<CardData>(seas[j]);
				if ((bool)fromID3)
				{
					Array.Resize(ref fromID3.CardInteractions, fromID3.CardInteractions.Length + 1);
					fromID3.CardInteractions[fromID3.CardInteractions.Length - 1] = fromID2.CardInteractions[0];
				}
			}
		}
		foreach (string light in lights)
		{
			CardData cardData = utc(light);
			if (cardData != null)
			{
				cardData.FuelCapacity.FloatValue = 南瓜时间 * 96;
				cardData.FuelCapacity.MaxValue = 南瓜时间 * 96;
			}
		}
		CardData cardData2 = utc("ceceb95e1f68491cb264b697b8e88dc9");
		if (cardData2 != null)
		{
			foreach (string item in 石磨磨坊)
			{
				CardData cardData3 = utc(item);
				if (cardData3 != null)
				{
					CardDrop cardDrop = default(CardDrop);
					cardDrop.DroppedCard = cardData2;
					cardDrop.Quantity = new Vector2Int(1, 1);
					CardDrop[] value8 = new CardDrop[1] { cardDrop };
					Traverse.Create(cardData3.CardInteractions[2].ProducedCards[0]).Field("DroppedCards").SetValue(value8);
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
