using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web.Script.Serialization;

namespace DesktopFarm {
public class CropDef {
 public string Id, Name; public int Cost, Price, Seconds, Level, Xp; public string Color;
 public CropDef(string id,string name,int cost,int price,int seconds,int level,int xp,string color){Id=id;Name=name;Cost=cost;Price=price;Seconds=seconds;Level=level;Xp=xp;Color=color;}
}
public class Recipe {
 public string Id,Name,Building; public int Seconds,Level,Price; public Dictionary<string,int> Inputs;
 public Recipe(string id,string name,string b,int sec,int lv,int price,params object[] inputs){Id=id;Name=name;Building=b;Seconds=sec;Level=lv;Price=price;Inputs=new Dictionary<string,int>();for(int i=0;i<inputs.Length;i+=2)Inputs[(string)inputs[i]]=(int)inputs[i+1];}
}
public class Plot {public string Crop="";public double Planted,Ready;public bool Watered;}
public class Job {public string Recipe;public double Ready;}
public class Animal {public string Type;public double Ready;public bool Fed;}
public class Decoration {public string Key="";public string Type;public int X,Y;}
public class Order {public string Person,Text;public Dictionary<string,int> Items=new Dictionary<string,int>();public int Coins,Xp;}
public class Construction {public int FromLevel,TargetLevel;public double Started,Ready;}
public class SaveData {
 public Dictionary<string,Construction> Construction=new Dictionary<string,Construction>();
 public List<string> OwnedMaps=new List<string>{"meadow"};public string Map="meadow";public int CameraX,CameraY;
 public Dictionary<string,LayoutPoint> DesktopLayout=new Dictionary<string,LayoutPoint>();
 public string Language="zh-CN",SelectedSeed="wheat";public bool WallpaperEnabled=false;
 public int HotkeyMods=0,HotkeyKey=119; public int HudZoom=100; public int Version=4,Coins=180,Xp=0,Land=0,PetLove=0,Harvests=0,Orders=0,Made=0,Sold=0,Planted=0,Petting=0;
 public double LastSeen,LastPet,LastGift,ProductionPausedUntil;
 public List<Plot> Plots=new List<Plot>();
 public Dictionary<string,int> Inventory=new Dictionary<string,int>();
 public Dictionary<string,int> Buildings=new Dictionary<string,int>();
 public Dictionary<string,List<Job>> Queues=new Dictionary<string,List<Job>>();
 public List<Animal> Animals=new List<Animal>();
 public List<Decoration> Decorations=new List<Decoration>();
 public List<Order> OrdersList=new List<Order>();
 public List<string> Achievements=new List<string>();
 public string Lighting="跟随时间"; public string DailyClaim="",Weather="自动",Pet="cat";public bool Sound=false,Music=false,Quiet=false,Desktop=false,IntroSeen=false;
 public int Scale=2,Monitor=0,DecorOwned=0,Guide=0;public double LifetimeCoins=0;
}
public class Farm {
 public static readonly CropDef[] Crops={
 new CropDef("wheat","小麦",3,2,300,1,1,"eac765"),new CropDef("corn","玉米",8,4,600,2,2,"f4d76b"),
 new CropDef("carrot","胡萝卜",12,6,1200,3,3,"e99150"),new CropDef("strawberry","草莓",24,10,3600,4,4,"db647b"),
 new CropDef("potato","土豆",20,9,2400,5,3,"c7a57b"),new CropDef("tomato","西红柿",32,14,5400,6,5,"db6651"),
 new CropDef("cotton","棉花",42,18,7200,7,6,"f4e9ce"),new CropDef("sugarcane","甘蔗",54,24,10800,8,8,"93bd60"),
 new CropDef("pumpkin","南瓜",70,32,14400,9,10,"de8945")};
 public static readonly Recipe[] Recipes={
 new Recipe("flour","面粉","mill",180,1,8,"wheat",2),new Recipe("feed","饲料","mill",300,2,12,"wheat",2,"corn",1),
 new Recipe("bread","乡村面包","bakery",900,2,22,"flour",2),new Recipe("berrybread","莓果面包","bakery",1800,4,52,"flour",2,"strawberry",2),
 new Recipe("cream","奶油","dairy",1800,5,28,"milk",2),new Recipe("cake","草莓蛋糕","bakery",3600,5,90,"flour",2,"cream",1,"strawberry",2),
 new Recipe("cloth","棉布","loom",3600,7,70,"cotton",3),new Recipe("sugar","糖","sugarhouse",2700,8,92,"sugarcane",3),
 new Recipe("jam","草莓果酱","preserves",5400,8,145,"strawberry",3,"sugar",1)};
 public static readonly string[] BuildingIds={"mill","bakery","coop","barn","dairy","loom","sugarhouse","preserves","station","cabin"};
 public static readonly string[] BuildingNames={"风车磨坊","面包房","鸡舍","牛棚","乳品工坊","织布坊","制糖坊","果酱厨房","乡间车站","农舍"};
 public static readonly int[] BuildingCosts={0,320,280,1200,1800,3000,4200,5200,9000,0};
 public static readonly int[] BuildingLevels={1,2,2,5,5,7,8,8,10,1};
 public SaveData S;public Func<double> Clock;public string LoadNotice="";public List<string> Notices=new List<string>();
 public static bool MaintenanceBuild {
#if MAINTENANCE_BUILD
  get{return true;}
#else
  get{return false;}
#endif
 }
 public static double UtcNow(){return (DateTime.UtcNow-new DateTime(1970,1,1)).TotalSeconds;}
 public double Now {get{return Clock();}} public bool ProductionPaused {get{return S.Construction!=null&&S.Construction.Count>0;}}public double ProductionClock {get{return ProductionPaused?Math.Max(Now,S.ProductionPausedUntil):Now;}}public double ProductionRemaining(double ready){return Math.Max(0,ready-ProductionClock);}public bool IsNight {get{return S.Lighting=="夜晚"||(S.Lighting!="白天"&&(DateTime.Now.Hour<6||DateTime.Now.Hour>=19));}}
 public int Level {get{int level=1;while(level<20&&S.Xp>=Threshold(level+1))level++;return level;}}
 public static int Threshold(int level){return (level-1)*45+(level-1)*(level-1)*35;}
 public Farm(Func<double> clock){Clock=clock;S=NewGame(Now);if(MaintenanceBuild)S.Coins=999999999;}
 public static SaveData NewGame(double now){var s=new SaveData();for(int i=0;i<12;i++)s.Plots.Add(new Plot());for(int i=0;i<4;i++){s.Plots[i].Crop="wheat";s.Plots[i].Planted=now-120;s.Plots[i].Ready=now+(i==0?-1:12+i*7);}s.Buildings["mill"]=1;s.Buildings["cabin"]=1;s.Inventory["wheat"]=6;s.LastSeen=now;s.OrdersList=MakeOrders(0,1);return s;}
 public static CropDef Crop(string id){return Crops.FirstOrDefault(c=>c.Id==id);}
 public static Recipe GetRecipe(string id){return Recipes.FirstOrDefault(r=>r.Id==id);}
 public static string ItemName(string id){var c=Crop(id);if(c!=null)return c.Name;var r=GetRecipe(id);if(r!=null)return r.Name;switch(id){case "egg":return "鸡蛋";case "milk":return "牛奶";case "wool":return "羊毛";case "honey":return "蜂蜜";}return id;}
 public static int Price(string id){var c=Crop(id);if(c!=null)return c.Price;var r=GetRecipe(id);if(r!=null)return r.Price;return id=="egg"?8:id=="milk"?12:id=="wool"?18:22;}
 public int Count(string id){return S.Inventory.ContainsKey(id)?S.Inventory[id]:0;}
 public void Add(string id,int n){S.Inventory[id]=Math.Max(0,Count(id)+n);}
 public bool Has(string b){return S.Buildings.ContainsKey(b);}
 public bool CanAfford(int cost){return MaintenanceBuild||S.Coins>=cost;}
 void SpendCoins(int cost){if(!MaintenanceBuild)S.Coins-=cost;}
 public void Gain(int coins,int xp){int before=Level;if(MaintenanceBuild)S.Coins=999999999;else S.Coins+=coins;S.LifetimeCoins+=coins;S.Xp+=xp;if(Level>before)Notices.Add("升到 Lv."+Level+"！商店有新的解锁。");}
 public string Plant(int index,string id){if(ProductionPaused)return L.T("建筑施工中，所有生产暂时暂停。");if(index<0||index>=S.Plots.Count)return "这块土地尚未开垦。";var p=S.Plots[index];if(p.Crop!="")return "这块田里已经有作物了。";var c=Crop(id);if(c==null||Level<c.Level)return "农场 Lv."+(c==null?1:c.Level)+" 解锁。";if(!CanAfford(c.Cost))return "金币不够，可以先出售仓库里的收成。";SpendCoins(c.Cost);p.Crop=id;p.Planted=Now;p.Ready=Now+c.Seconds;p.Watered=false;S.Planted++;return "种下"+c.Name+" · "+TimeText(c.Seconds)+"后成熟";}
 public string Harvest(int index){if(index<0||index>=S.Plots.Count)return "";var p=S.Plots[index];if(p.Crop=="")return "选好种子，再点击空地播种。";if(p.Ready>Now)return "还需 "+TimeText(p.Ready-Now)+" · 浇水可缩短一次生长时间";var c=Crop(p.Crop);Add(c.Id,3);S.Harvests++;Gain(0,c.Xp);p.Crop="";p.Watered=false;CheckAchievements();return c.Name+" +3 · 经验 +"+c.Xp;}
 public string Water(int index){if(index<0||index>=S.Plots.Count)return "";var p=S.Plots[index];if(p.Crop=="")return "先种下作物再浇水。";if(p.Ready<=Now)return Harvest(index);if(ProductionPaused)return L.T("建筑施工中，所有生产暂时暂停。");if(p.Watered)return "土壤已经湿润了，安心等它长大吧。";p.Watered=true;p.Ready=Now+(p.Ready-Now)*0.8;return "浇水完成 · 剩余时间缩短 20%";}
 public string HarvestAll(){int n=0;for(int i=0;i<S.Plots.Count;i++)if(S.Plots[i].Crop!=""&&S.Plots[i].Ready<=Now){Harvest(i);n++;}return n==0?"暂时没有成熟的作物。":"收获了 "+n+" 块田，收成都在仓库里。";}
 public string PlantAll(string id){if(ProductionPaused)return L.T("建筑施工中，所有生产暂时暂停。");var crop=Crop(id);if(crop==null)return "种子不存在。";int n=0;foreach(int i in Enumerable.Range(0,S.Plots.Count)){if(S.Plots[i].Crop==""&&CanAfford(crop.Cost)&&Level>=crop.Level){Plant(i,id);n++;}}return n==0?"没有可播种的空地，或金币不足。":"已播种 "+n+" 块"+ItemName(id)+"。";}
 public string Sell(string id,int qty){qty=Math.Min(qty,Count(id));if(qty<=0)return "仓库里暂时没有这件商品。";int value=Price(id)*qty;Add(id,-qty);Gain(value,0);S.Sold+=qty;CheckAchievements();return "售出 "+qty+" 份"+ItemName(id)+" · 金币 +"+value;}
 public List<Job> Queue(string building){return S.Queues.ContainsKey(building)?S.Queues[building]:new List<Job>();}
 public string Craft(string id){var r=GetRecipe(id);if(r==null)return "配方不存在。";if(ProductionPaused)return L.T("建筑施工中，所有生产暂时暂停。");if(!Has(r.Building))return "请先在建造商店购买"+BuildingName(r.Building)+"。";if(Level<r.Level)return "农场 Lv."+r.Level+" 解锁这张配方。";var q=Queue(r.Building);if(q.Count>=S.Buildings[r.Building]+1)return "生产队列已满。升级建筑可以增加槽位。";foreach(var p in r.Inputs)if(Count(p.Key)<p.Value)return "缺少"+ItemName(p.Key)+"，需要 "+p.Value+" 份。";foreach(var p in r.Inputs)Add(p.Key,-p.Value);double start=q.Count==0?Now:Math.Max(Now,q.Last().Ready);S.Queues[r.Building]=q;q.Add(new Job{Recipe=id,Ready=start+r.Seconds*(S.Buildings[r.Building]>=3?.9:1)});return r.Name+" 已加入队列 · 完成后自动入仓";}
 static void ShiftProduction(SaveData s,double delta,double floor){if(delta<=0)return;foreach(var q in s.Queues.Values)foreach(var j in q){if(j.Ready<floor)j.Ready=floor;j.Ready+=delta;}foreach(var p in s.Plots)if(p.Crop!=""&&p.Ready>floor){p.Planted+=delta;p.Ready+=delta;}foreach(var a in s.Animals)if(a.Fed&&a.Ready>floor)a.Ready+=delta;}
 public void Tick(){CompleteConstruction();if(S.Construction.Count==0)foreach(var pair in S.Queues){var q=pair.Value;while(q.Count>0&&q[0].Ready<=Now){var j=q[0];q.RemoveAt(0);Add(j.Recipe,1);S.Made++;Gain(0,2);Notices.Add(ItemName(j.Recipe)+"完成，已放入仓库。");}}CheckAchievements();if(S.Coins<2&&S.Inventory.Values.All(n=>n==0)&&S.Plots.All(p=>p.Crop=="")&&S.Queues.Values.All(q=>q.Count==0)&&S.Animals.All(a=>!a.Fed)){Gain(10,0);Notices.Add("山谷互助箱送来 10 金币，再种一点小麦吧。");}S.LastSeen=Now;}
 public static string BuildingName(string id){int i=Array.IndexOf(BuildingIds,id);return i<0?id:BuildingNames[i];}
 public int BuilderSlots {get{return Has("cabin")&&S.Buildings["cabin"]>=3?3:2;}}
 public int UpgradeCost(string id){int i=Array.IndexOf(BuildingIds,id);return i<0?0:Math.Max(320,BuildingCosts[i])*(Has(id)&&S.Buildings[id]>=2?5:2);}
 public int BuildSeconds(string id,int target){if(MaintenanceBuild)return 60;int i=Array.IndexOf(BuildingIds,id);int[] minutes={15,30,20,120,180,240,360,480,720,20};int first=i<0?1800:minutes[i]*60;return target==1?first:Math.Min(86400,first*(target==2?4:12));}
 public bool CanBuild(string id){int i=Array.IndexOf(BuildingIds,id);return i>=0&&!S.Construction.ContainsKey(id)&&S.Construction.Count<BuilderSlots&&Level>=BuildingLevels[i]&&Queue(id).Count==0&&(!Has(id)||S.Buildings[id]<3)&&CanAfford(Has(id)?UpgradeCost(id):BuildingCosts[i]);}
 public string BuyBuilding(string id){int i=Array.IndexOf(BuildingIds,id);if(i<0)return "";if(S.Construction.ContainsKey(id))return "这座建筑已经在施工中。";if(S.Construction.Count>=BuilderSlots)return "施工队都在忙，请等待一处工程完工。";if(Has(id)&&S.Buildings[id]>=3)return "建筑已达到最高等级。";if(Queue(id).Count>0)return "请等待生产队列完成，再开始升级。";if(Level<BuildingLevels[i])return "农场 Lv."+BuildingLevels[i]+" 解锁。";int cost=Has(id)?UpgradeCost(id):BuildingCosts[i];if(!CanAfford(cost))return "金币不足，还需要 "+(cost-S.Coins)+" 金币。";double oldUntil=S.Construction.Count==0?Now:Math.Max(Now,S.ProductionPausedUntil);int old=Has(id)?S.Buildings[id]:0;SpendCoins(cost);S.Construction[id]=new Construction{FromLevel=old,TargetLevel=old+1,Started=Now,Ready=Now+BuildSeconds(id,old+1)};double pauseUntil=S.Construction.Values.Max(c=>c.Ready);ShiftProduction(S,pauseUntil-oldUntil,oldUntil);S.ProductionPausedUntil=pauseUntil;return "工程已开工，建筑完工前全农场暂停生产。";}
 public string Upgrade(string id){return BuyBuilding(id);}
 void CompleteConstruction(){foreach(string id in S.Construction.Keys.ToArray()){var c=S.Construction[id];if(c.Ready>Now)continue;S.Buildings[id]=c.TargetLevel;S.Construction.Remove(id);if(c.FromLevel==0&&id=="coop")S.Animals.Add(new Animal{Type="chicken"});if(c.FromLevel==0&&id=="barn")S.Animals.Add(new Animal{Type="cow"});Gain(0,10);Notices.Add(BuildingName(id)+" · Lv."+c.TargetLevel+" · "+L.T("施工完成"));}if(S.Construction.Count==0)S.ProductionPausedUntil=0;}
 public static readonly int[] PlotCounts={12,24,36,45,54,63,72,81};
 public int ExpandLevel {get{return new[]{2,5,7,9,11,13,15,20}[S.Land];}}
 public int ExpandCost {get{return new[]{500,1800,3000,4500,6500,9000,12000,0}[S.Land];}}
 public string Expand(){if(S.Land>=7)return "81 块田已全部开垦。";int cost=ExpandCost;if(Level<ExpandLevel)return "农场 Lv."+ExpandLevel+" 可扩建。";if(!CanAfford(cost))return "扩建需要 "+cost+" 金币。";SpendCoins(cost);S.Land++;while(S.Plots.Count<PlotCounts[S.Land])S.Plots.Add(new Plot());Gain(0,20);CheckAchievements();return "农田已扩建，桌面道路与建筑保持原位。";}
 public string AnimalAction(int index){if(index<0||index>=S.Animals.Count)return "";if(ProductionPaused)return L.T("建筑施工中，所有生产暂时暂停。");var a=S.Animals[index];string product=a.Type=="chicken"?"egg":a.Type=="cow"?"milk":a.Type=="sheep"?"wool":"honey";if(a.Fed){if(a.Ready>Now)return "它正舒服地休息 · "+TimeText(a.Ready-Now)+"后可收集";Add(product,2);a.Fed=false;Gain(0,3);return ItemName(product)+" +2 · 再喂食即可继续生产";}string food=Count("feed")>0?"feed":"wheat";int amount=food=="feed"?1:3;if(Count(food)<amount)return "需要 1 份饲料或 3 份小麦。";Add(food,-amount);a.Fed=true;a.Ready=Now+((a.Type=="chicken"?600:a.Type=="cow"?1800:2700)*(Has(a.Type=="chicken"?"coop":"barn")?1-.1*(S.Buildings[a.Type=="chicken"?"coop":"barn"]-1):1));return "喂食完成，它开心地吃了起来。";}
 public string BuyAnimal(string type){int lv=type=="sheep"?6:8,cost=type=="sheep"?320:400;if(Level<lv)return "农场 Lv."+lv+" 解锁。";if(S.Animals.Any(a=>a.Type==type))return "这位小伙伴已经住在农场里了。";if(!CanAfford(cost))return "需要 "+cost+" 金币。";SpendCoins(cost);S.Animals.Add(new Animal{Type=type});return "新的动物搬来了！";}
 public string Pet(){S.Petting++;if(Now-S.LastPet<60)return "小伙伴蹭了蹭你的手。好感度每分钟可增加一次。";S.LastPet=Now;S.PetLove=Math.Min(999,S.PetLove+2);CheckAchievements();return "摸摸"+(S.Pet=="cat"?"团子":"豆豆")+" · 好感度 +2";}
 public string GiftPet(){if(Now-S.LastGift<300)return "小伙伴还在回味，过一会儿再喂吧。";string food=Count("bread")>0?"bread":"wheat";if(Count(food)<1)return "仓库需要一份小麦或面包。";Add(food,-1);S.LastGift=Now;S.PetLove=Math.Min(999,S.PetLove+5);return "收到小零食 · 好感度 +5";}
 public static List<Order> MakeOrders(int completed,int level){var list=new List<Order>();string[] people={"莉莉 · 花店","诺亚 · 木匠","艾米 · 旅店","山谷集市"};for(int i=0;i<3;i++){int k=(completed+i)%4;var o=new Order{Person=people[k],Text=k==0?"清晨开店前，想备一些新鲜食材。":k==1?"忙完木工活，来一顿简单的午餐。":k==2?"为远道而来的客人准备一点心意。":"这一篮丰收，会让今天更美好。"};if(level<2)o.Items["wheat"]=4+i*2;else if(level<4){o.Items["wheat"]=3+i;o.Items[i==1?"flour":"corn"]=i==1?1:2;}else{o.Items[i==0?"bread":i==1?"carrot":"strawberry"]=i==0?1:3;o.Items["wheat"]=3;}o.Coins=(int)(o.Items.Sum(p=>Price(p.Key)*p.Value)*1.25);o.Xp=5+i*2;list.Add(o);}return list;}
 public bool CanOrder(Order o){return o.Items.All(p=>Count(p.Key)>=p.Value);}
 public string Deliver(int index){if(index<0||index>=S.OrdersList.Count)return "";var o=S.OrdersList[index];if(!CanOrder(o))return "还缺一些商品，仓库数量显示在订单上。";foreach(var p in o.Items)Add(p.Key,-p.Value);Gain(o.Coins,o.Xp);S.Orders++;S.OrdersList[index]=MakeOrders(S.Orders+index,Level)[index];CheckAchievements();return "订单送达 · 金币 +"+o.Coins+" / 经验 +"+o.Xp;}
 public string Train(){if(!Has("station"))return "农场 Lv.10 可建造车站。";if(Count("bread")<5||Count("milk")<6||Count("jam")<2)return "列车需要：面包 ×5、牛奶 ×6、果酱 ×2。";Add("bread",-5);Add("milk",-6);Add("jam",-2);Gain(700+(S.Buildings["station"]-1)*100,25);S.Orders++;return "满载的列车出发了 · 金币 +"+(700+(S.Buildings["station"]-1)*100)+" / 经验 +25";}
 public string Daily(){string day=DateTime.Today.ToString("yyyy-MM-dd");if(S.DailyClaim==day)return "今天的小礼物已领取，明天再来看看。";S.DailyClaim=day;Gain(35,5);Add("wheat",3);return "邮差带来礼物 · 金币 +35 / 小麦 +3";}
 public void CheckAchievements(){Award("first",S.Harvests>=1,"第一缕麦香",15);Award("maker",S.Made>=1,"手作时光",25);Award("orders",S.Orders>=5,"山谷好邻居",70);Award("harvest",S.Harvests>=50,"丰收的日子",100);Award("friend",S.PetLove>=20,"最好的朋友",40);Award("land",S.Land>=1,"更大的世界",50);Award("rich",S.LifetimeCoins>=3000,"小镇的梦想",150);}
 void Award(string id,bool yes,string title,int coins){if(yes&&!S.Achievements.Contains(id)){S.Achievements.Add(id);Gain(coins,5);Notices.Add("成就「"+title+"」 · 金币 +"+coins);}}
 public string GuideTitle {get{string[] t={"收获第一块金色小麦","在空地种下一粒种子","到工坊制作一袋面粉","从仓库卖出一份商品","完成一张邻里订单","摸摸农场里的小猫","扩建你的第一片土地","把日子过成喜欢的样子"};return t[Math.Min(S.Guide,7)];}}
 public string GuideHelp {get{string[] t={"点击田里的金色小麦，收成会放进仓库。","选择底部的种子，点击刚收获的空地。","点「工坊」，用 2 份小麦制作面粉。","打开「仓库」，点击商品旁的出售按钮。","打开「订单」，备齐商品就可以交付。","点击会走动的猫，或打开「伙伴」摸摸它。","达到 Lv.2 后，在「建造」里购买新土地。","安排长时间作物，然后安心去做自己的事。"};return t[Math.Min(S.Guide,7)];}}
 public bool GuideDone {get{switch(S.Guide){case 0:return S.Harvests>0;case 1:return S.Planted>0;case 2:return S.Made>0||S.Queues.Values.Any(q=>q.Count>0);case 3:return S.Sold>0;case 4:return S.Orders>0;case 5:return S.Petting>0;case 6:return S.Land>0;}return false;}}
 public string ClaimGuide(){if(!GuideDone)return GuideHelp;S.Guide++;Gain(20,5);return "小目标完成 · 金币 +20 / 经验 +5";}
 public string BuyDecoration(string type,int x,int y){int cost=DecorCost(type);if(!CanAfford(cost))return "需要 "+cost+" 金币。";if(S.Decorations.Count>=128)return "装饰已达 128 件，可以先移动或收回旧装饰。";SpendCoins(cost);S.Decorations.Add(new Decoration{Key=Guid.NewGuid().ToString("N"),Type=type,X=x,Y=y});S.DecorOwned++;return "装饰放好了，农场更像家了。";}
 public static int DecorCost(string type){return type=="path"?5:type=="flowers"?25:type=="fence"?20:type=="lamp"?55:type=="bench"?45:type=="tree"?70:90;}
 public static string DecorName(string type){return type=="path"?"道路":type=="flowers"?"花坛":type=="fence"?"木围栏":type=="lamp"?"路灯":type=="bench"?"长椅":type=="tree"?"苹果树":"小喷泉";}
 public static string TimeText(double seconds){int s=(int)Math.Ceiling(Math.Max(0,seconds));return s>=3600?(s/3600)+"时 "+((s%3600)/60)+"分":s>=60?(s/60)+"分 "+(s%60)+"秒":s+"秒";}
 public void Save(string path){S.LastSeen=Now;Directory.CreateDirectory(Path.GetDirectoryName(path));string tmp=path+".tmp";string json=new JavaScriptSerializer().Serialize(S);File.WriteAllText(tmp,json,System.Text.Encoding.UTF8);if(File.Exists(path))File.Replace(tmp,path,path+".bak");else File.Move(tmp,path);}
 public void Load(string path){if(!File.Exists(path))return;foreach(string candidate in new[]{path,path+".bak"}){try{var s=new JavaScriptSerializer().Deserialize<SaveData>(File.ReadAllText(candidate));int format=s.Version;Validate(s);if(format<4){if(s.Construction.Count>0){double until=s.Construction.Values.Max(c=>c.Ready),wait=Math.Max(0,until-Now);ShiftProduction(s,wait,Now);s.ProductionPausedUntil=Math.Max(Now,until);}s.Version=4;}else if(s.Construction.Count>0)s.ProductionPausedUntil=Math.Max(s.ProductionPausedUntil,s.Construction.Values.Max(c=>c.Ready));else s.ProductionPausedUntil=0;S=s;if(MaintenanceBuild)S.Coins=999999999;if(candidate!=path)LoadNotice="主存档无法读取，已恢复备份存档。";double away=Math.Max(0,Now-S.LastSeen);Tick();if(away>120)Notices.Add("欢迎回家！离开了 "+TimeText(away)+"，农场一直在生长。");return;}catch(Exception){}}LoadNotice="存档和备份无法读取，已保留原文件；本次使用新农场。";}
 public static void Validate(SaveData s){if(s==null||(s.Version!=1&&s.Version!=2&&s.Version!=3&&s.Version!=4)||s.Land<0||s.Land>7||s.Plots==null||s.Plots.Count!=PlotCounts[s.Land]||s.Inventory==null||s.Queues==null||s.Buildings==null||s.Animals==null||s.Decorations==null||s.OrdersList==null||s.OrdersList.Count!=3||s.Achievements==null||s.Coins<0||s.Xp<0||double.IsNaN(s.ProductionPausedUntil)||double.IsInfinity(s.ProductionPausedUntil))throw new Exception("Invalid save");if(s.Version==1){int lv=1;while(lv<20&&s.Xp>=(lv*20+lv*lv*5))lv++;int oldStart=(lv-1)*20+(lv-1)*(lv-1)*5,oldNext=lv*20+lv*lv*5;s.Xp=lv>=20?Threshold(20):Threshold(lv)+(int)((s.Xp-oldStart)/(double)(oldNext-oldStart)*(Threshold(lv+1)-Threshold(lv)));s.Version=2;}s.Version=4;if(!ShortcutKeys.Valid(s.HotkeyMods,s.HotkeyKey)){s.HotkeyMods=0;s.HotkeyKey=119;}if(s.Construction==null)s.Construction=new Dictionary<string,Construction>();if(s.OwnedMaps==null)s.OwnedMaps=new List<string>();if(!s.OwnedMaps.Contains("meadow"))s.OwnedMaps.Add("meadow");s.OwnedMaps=s.OwnedMaps.Where(m=>WorldMap.Ids.Contains(m)).Distinct().ToList();if(!s.OwnedMaps.Contains(s.Map))s.Map="meadow";s.CameraX=Math.Max(-960,Math.Min(960,s.CameraX));s.CameraY=Math.Max(-540,Math.Min(540,s.CameraY));if(!s.Buildings.ContainsKey("cabin"))s.Buildings["cabin"]=1;foreach(var pair in s.Construction){var c=pair.Value;if(!BuildingIds.Contains(pair.Key)||c==null||c.TargetLevel<1||c.TargetLevel>3||c.FromLevel!=c.TargetLevel-1||!(!s.Buildings.ContainsKey(pair.Key)?c.FromLevel==0:s.Buildings[pair.Key]==c.FromLevel)||double.IsNaN(c.Started)||double.IsInfinity(c.Started)||double.IsNaN(c.Ready)||double.IsInfinity(c.Ready)||c.Ready<=c.Started)throw new Exception("Invalid construction");}foreach(var p in s.Plots)if(p==null||(p.Crop!=""&&Crop(p.Crop)==null)||double.IsNaN(p.Ready)||double.IsInfinity(p.Ready))throw new Exception("Invalid plot");foreach(var q in s.Queues){if(q.Value==null||q.Value.Count>4||(!s.Buildings.ContainsKey(q.Key)&&q.Value.Count>0))throw new Exception("Invalid queue");foreach(var j in q.Value)if(j==null||GetRecipe(j.Recipe)==null||GetRecipe(j.Recipe).Building!=q.Key||double.IsNaN(j.Ready)||double.IsInfinity(j.Ready))throw new Exception("Invalid job");}foreach(var o in s.OrdersList)if(o==null||o.Items==null||o.Items.Count==0||o.Items.Any(p=>p.Value<=0)||o.Coins<0||o.Xp<0)throw new Exception("Invalid order");foreach(var d in s.Decorations)if(d!=null&&string.IsNullOrEmpty(d.Key))d.Key=Guid.NewGuid().ToString("N");if(s.DesktopLayout==null)s.DesktopLayout=new Dictionary<string,LayoutPoint>();foreach(var key in s.DesktopLayout.Keys.ToArray()){var p=s.DesktopLayout[key];if(p==null||double.IsNaN(p.X)||double.IsNaN(p.Y)||double.IsInfinity(p.X)||double.IsInfinity(p.Y))s.DesktopLayout.Remove(key);else{p.X=Math.Max(-1,Math.Min(2,p.X));p.Y=Math.Max(-1,Math.Min(2,p.Y));}}s.Language=s.Language=="en"?"en":"zh-CN";if(Crop(s.SelectedSeed)==null)s.SelectedSeed="wheat";s.Guide=Math.Max(0,Math.Min(7,s.Guide));s.HudZoom=Math.Max(50,Math.Min(150,s.HudZoom));s.Scale=Math.Max(1,Math.Min(3,s.Scale));}
}
}
