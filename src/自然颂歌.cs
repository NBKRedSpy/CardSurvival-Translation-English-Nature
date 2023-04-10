using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace 自然颂歌
{ 

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

        static readonly string blank = "a913214948a94d2897846870889f4d11";

        static Dictionary<string, CardData> card_dict = new Dictionary<string, CardData>();
        static Dictionary<string, GameStat> stat_dict = new Dictionary<string, GameStat>();
        static List<CardData> cropPlots = new List<CardData>();
        static List<String> seas = new List<string>
        {
            "749dadaca7677ec4c9f54e35b4343cdb",
            "11c21f04e59b27244abb401684c04f0f",
            "6b87970979841684bb6d6a7471430798",
            "1d8a5586879b6aa489b6b63b1255ed59",
            "c3b8f83d682923643a960fe6c64d19fc",
            "4a02a6fed76406546ad36f6481a910a2",
            "64933f58900541a49bc1becbe7a852c5",
            "115769419d6a5144683d9d61e7cff8e9",
            "a0e8e31d9db3a974b8a08e41295799e0",
        };
        static List<String> lights = new List<string>//南瓜灯
        {
			"f72dda619bd111eda64e047c16184f06",
			"704446cba52c11edb49ec475ab46ec3f",
			"8ff37d3e9bd011ed97f8047c16184f06",
			"bc730cf89bcf11ed9292047c16184f06",
			"4068affda52c11edb883c475ab46ec3f"
		};

		static List<String> 石磨磨坊 = new List<string>
		{
			"930d051293e911ed90a6047c16184f06",//磨坊
			"91c9109d93e511eda299047c16184f06"//石磨
		};

		private static CardData utc(String uniqueID)
		{
			return UniqueIDScriptable.GetFromID<CardData>(uniqueID);
		}

		private static void 添加研磨(CardData 研磨前, CardData 研磨后, bool 是否通电,int 产量 = 1){
            if(研磨前 == null | 研磨后 == null) return;

			LocalizedString name1 = new LocalizedString{
				DefaultText = "研磨",
				ParentObjectID = 是否通电 ? 石磨磨坊[0] : 石磨磨坊[1],
				LocalizationKey = "Guil-更多水果_榨汁"
			};
			LocalizedString name2 = new LocalizedString
			{
				DefaultText = "将材料磨成粉",
				LocalizationKey = "GrindingIntoPowder",
				ParentObjectID = 是否通电 ? 石磨磨坊[0] : 石磨磨坊[1],
			};
			CardOnCardAction action = new CardOnCardAction(name1, name2, 是否通电 ? 0 : 1);
			Array.Resize(ref action.CompatibleCards.TriggerCards, 1);
			action.CompatibleCards.TriggerCards[0] = 研磨前;
			action.GivenCardChanges.ModType = CardModifications.Destroy;
			//action.GivenCardChanges.TransformInto = 研磨后;
			action.StackCompatible = true;

			CardsDropCollection cdc = new CardsDropCollection();
			cdc.CollectionName = "产出";
			cdc.CollectionWeight = 1;

			CardDrop cd = new CardDrop();
			cd.DroppedCard = 研磨后;
			cd.Quantity = new Vector2Int(产量, 产量);
			CardDrop[] cds = new CardDrop[] { cd };
			Traverse.Create(cdc).Field("DroppedCards").SetValue(cds);

			action.ProducedCards = new CardsDropCollection[] { cdc };



			CardData 酿造台 = utc(石磨磨坊[1]);
            CardData 酿造台_通电 = utc(石磨磨坊[0]);

			if (是否通电 == false) {
				Array.Resize(ref 酿造台.CardInteractions, 酿造台.CardInteractions.Length + 1);
				酿造台.CardInteractions[酿造台.CardInteractions.Length - 1] = action;
			}
			else {
				Array.Resize(ref 酿造台_通电.CardInteractions, 酿造台_通电.CardInteractions.Length + 1);
				酿造台_通电.CardInteractions[酿造台_通电.CardInteractions.Length - 1] = action;
			}

		}


	    private void Awake()
	    {
                //读取配置
                自然颂歌.种田返还 = base.Config.Bind<bool>("Nature Setting", "种田返还", true, "原版作物成熟后是否会返还田地，为true时会返还，默认为true（PS：mod的如果想返还，把田的名字改为CropPlotXXX即可）").Value;
                自然颂歌.叶子不腐 = base.Config.Bind<bool>("Nature Setting", "叶子不腐", true, "叶子是否会腐败，为true时不会腐败，默认为true").Value;
                自然颂歌.棕榈不腐 = base.Config.Bind<bool>("Nature Setting", "棕榈不腐", true, "棕榈叶是否会腐败，为true时不会腐败，默认为true").Value;
                自然颂歌.猴子救星 = base.Config.Bind<bool>("Nature Setting", "猴子救星", true, "是否启用猴子救星（受伤的猴子、猴子朋友不咬人），为true时启用，默认为true").Value;
                自然颂歌.蜜蜂静音 = base.Config.Bind<bool>("Nature Setting", "蜜蜂静音", true, "是否启用蜜蜂静音，为true时启用，默认为true").Value;
                自然颂歌.南瓜时间 = base.Config.Bind<int>("Nature Setting", "南瓜时间", 3, "南瓜灯用一根蜡烛可以持续的时间，单位是天，默认为3天").Value;
                自然颂歌.去除减益 = base.Config.Bind<bool>("Nature Setting", "去除减益", true, "去除技能训练减益").Value;
			    //结束

			    //手动Patch
			    var harmony = new Harmony(this.Info.Metadata.GUID);
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
                try
                {
                    var load = new HarmonyMethod(typeof(自然颂歌).GetMethod("SomePatch"));
                    var method = typeof(GameLoad).GetMethod("LoadMainGameData", bindingFlags);
                    if (method == null)
                    {
                        method = typeof(GameLoad).GetMethod("LoadGameData", bindingFlags);
                    }
                    if (method != null)
                        harmony.Patch(method, postfix: load);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarningFormat("{0} {1}", "GameLoadLoadOptionsPostfix", ex.ToString());
                }

            Logger.LogInfo("Plugin 自然颂歌 is loaded!");
	    }

            //[HarmonyPostfix]
            //[HarmonyPatch(typeof(GameLoad), "LoadMainGameData")]
            public static void SomePatch()
            {
                //读取所有卡
                for (int i = 0; i < GameLoad.Instance.DataBase.AllData.Count; i++) {
                    if (GameLoad.Instance.DataBase.AllData[i] is CardData) {
                        card_dict[GameLoad.Instance.DataBase.AllData[i].name] = GameLoad.Instance.DataBase.AllData[i] as CardData;
                        string name = GameLoad.Instance.DataBase.AllData[i].name;
                        bool a = name.IndexOf("CropPlot") >= 0; //名字中有CropPlot
                        bool b = name.EndsWith("田");//以田结尾
                        bool c = name.EndsWith("Empty");//以Empty结尾
                        bool d = name.IndexOf("better") >= 0;//以Empty结尾
                        if ((a || b) && !c && !d) {
                            //Debug.Log(name);
                            cropPlots.Add(GameLoad.Instance.DataBase.AllData[i] as CardData);
                        }
                    }
                    if (GameLoad.Instance.DataBase.AllData[i] is GameStat) {
                        stat_dict[GameLoad.Instance.DataBase.AllData[i].name] = GameLoad.Instance.DataBase.AllData[i] as GameStat;
                    }
                }
                if (种田返还) {
                    //修正水稻田
                    if (card_dict.TryGetValue("RicePaddy", out CardData a) && card_dict.TryGetValue("RicePaddyEmpty", out CardData b)) {
                        a.Progress.OnFull.ReceivingCardChanges.ModType = CardModifications.Transform;
                        a.Progress.OnFull.ReceivingCardChanges.TransformInto = b;
                    }
                    //修正剩余的田
                    if (card_dict.TryGetValue("CropPlotEmpty", out CardData c)) {
                        foreach (CardData crop in cropPlots) {
                            if (crop.Progress?.OnFull?.ReceivingCardChanges.ModType == CardModifications.Destroy) {
                                crop.Progress.OnFull.ReceivingCardChanges.ModType = CardModifications.Transform;
                                crop.Progress.OnFull.ReceivingCardChanges.TransformInto = c;
                            }
                        }
                    }
                }

                if (card_dict.TryGetValue("PalmFronds", out CardData a1) && 棕榈不腐) {
                    DurabilityStat d1 = new DurabilityStat(false, 0);
                    a1.SpoilageTime = d1;
                }

                if (card_dict.TryGetValue("LeavesFresh", out CardData a2) && 叶子不腐) {
                    DurabilityStat d1 = new DurabilityStat(false, 0);
                    a2.SpoilageTime = d1;
                }

                if (猴子救星) {
                    if (card_dict.TryGetValue("MacaqueWounded", out CardData a3)) {
                        a3.CardInteractions[0].ProducedCards[1].CollectionWeight = 0;
                        a3.CardInteractions[1].ProducedCards[1].CollectionWeight = 0;
                        a3.DismantleActions[1].ProducedCards[1].CollectionWeight = 0;
                    }
                    if (card_dict.TryGetValue("MacaqueFriend", out CardData a4)) {
                        List<CardsDropCollection> nums = new List<CardsDropCollection>(a4.CardInteractions[0].ProducedCards);
                        nums.RemoveAt(1);
                        a4.CardInteractions[0].ProducedCards = nums.ToArray();

                        nums = new List<CardsDropCollection>(a4.CardInteractions[1].ProducedCards);
                        nums.RemoveAt(1);
                        a4.CardInteractions[1].ProducedCards = nums.ToArray();

                        nums = new List<CardsDropCollection>(a4.CardInteractions[6].ProducedCards);
                        nums.RemoveAt(1);
                        a4.CardInteractions[6].ProducedCards = nums.ToArray();

                        nums = new List<CardsDropCollection>(a4.DismantleActions[0].ProducedCards);
                        nums.RemoveAt(1);
                        a4.DismantleActions[0].ProducedCards = nums.ToArray();
                    }
                }

                if (蜜蜂静音) {
                    CardData 蜂箱 = UniqueIDScriptable.GetFromID<CardData>("0a2f6fa8b61d1ff4c8796ea73c78c114");
                    if (蜂箱 != null) {
                        蜂箱.Ambience.BackgroundSound = null;
                    }
                }

                //海面投石
                CardData bl = UniqueIDScriptable.GetFromID<CardData>(blank);
                if (bl) {
                    CardData sea;
                    for (int m = 0; m < seas.Count; m++) {
                        sea = UniqueIDScriptable.GetFromID<CardData>(seas[m]);
                        if (sea) {
                            //sea.CardInteractions.AddItem(bl.CardInteractions[0]);
                            Array.Resize(ref sea.CardInteractions, sea.CardInteractions.Length + 1);
                            sea.CardInteractions[sea.CardInteractions.Length - 1] = bl.CardInteractions[0];
                        }
                    }
                }

                //修改南瓜灯时间
                foreach (string cardid in lights) {
                    CardData light = utc(cardid);
                    if (light != null) {
                        light.FuelCapacity.FloatValue = 南瓜时间 * 96;
                        light.FuelCapacity.MaxValue = 南瓜时间 * 96;
                    }
                }

                CardData 豆粕 = utc("ceceb95e1f68491cb264b697b8e88dc9");
                if(豆粕  != null) {
				    foreach (string cardid in 石磨磨坊) {
					    CardData light = utc(cardid);
					    if (light != null) {	
					    CardDrop cd = new CardDrop();
					    cd.DroppedCard = 豆粕;
					    cd.Quantity = new Vector2Int(1, 1);
					    CardDrop[] cds = new CardDrop[] { cd };
                            //light.CardInteractions[2].ProducedCards
                            Traverse.Create(light.CardInteractions[2].ProducedCards[0]).Field("DroppedCards").SetValue(cds);
					    }
				    }
			    }

                //研磨可可豆
                CardData 可可豆 = utc("40bdee90a1e411ed85a0c475ab46ec3f");
                CardData 可可粉 = utc("4159826e61c04b728fa63c332f4e1d4b");
                添加研磨(可可豆, 可可粉, true,2);
                添加研磨(可可豆, 可可粉, false,2);

                //去除减益
                //读取所有的GameStat
                if (去除减益) {
				    foreach(KeyValuePair<string,GameStat> kvp in stat_dict) {
                        if (kvp.Key.IndexOf("Skill") > -1) {
                            kvp.Value.StalenessMultiplier = 1f;
                            kvp.Value.MaxStalenessStack = 0;
                        }
                    }
			    }

		    }
        }
}
