using System;
using System.Linq;
namespace DesktopFarm {
// The wallpaper and management window use the same authoritative farm model.
public static class DesktopHud {
 public class Command {public string action {get;set;}public string id {get;set;}}
 public static object State(Farm f,WallpaperForm scene,string panel,int revision,bool startup=false,bool startupAllowed=true){return new {
  shortcut=ShortcutKeys.Label(f.S.HotkeyMods,f.S.HotkeyKey),productionPaused=f.ProductionPaused,plots=f.S.Plots.Select((p,i)=>new{id=i,crop=p.Crop,watered=p.Watered,seconds=f.ProductionRemaining(p.Ready),ready=p.Crop!=""&&p.Ready<=f.Now}).ToArray(),
  animals=f.S.Animals.Select((a,i)=>new{id=i,type=a.Type,fed=a.Fed,seconds=f.ProductionRemaining(a.Ready),can=!f.ProductionPaused&&(a.Fed?a.Ready<=f.Now:f.Count("feed")>=1||f.Count("wheat")>=3)}).ToArray(),feed=f.Count("feed"),wheat=f.Count("wheat"),
  pan=scene.PanMode,cameraX=f.S.CameraX,cameraY=f.S.CameraY,map=f.S.Map,builders=f.BuilderSlots,busyBuilders=f.S.Construction.Count,maps=WorldMap.Ids.Select(id=>new{id=id,name=WorldMap.Name(id),description=WorldMap.Description(id),cost=WorldMap.Cost(id),owned=f.S.OwnedMaps.Contains(id)}).ToArray(),quiet=f.S.Quiet,startup=startup,startupAllowed=startupAllowed,weather=f.S.Weather,zoom=f.S.HudZoom,coins=f.S.Coins,level=f.Level,xp=f.S.Xp-Farm.Threshold(f.Level),xpMax=f.Level>=20?0:Farm.Threshold(f.Level+1)-Farm.Threshold(f.Level),language=f.S.Language,seed=f.S.SelectedSeed,edit=scene.EditMode,pending=scene.PendingName,note=scene.CurrentNote,panel=panel,revision=revision,
  ready=f.S.Plots.Count(p=>p.Crop!=""&&p.Ready<=f.Now),empty=f.S.Plots.Count(p=>p.Crop==""),land=f.S.Plots.Count,expandCost=f.ExpandCost,canExpand=f.S.Land<7&&f.Level>=f.ExpandLevel&&f.S.Coins>=f.ExpandCost,
  crops=Farm.Crops.Select(c=>new{id=c.Id,name=L.T(c.Name),cost=c.Cost,seconds=c.Seconds,level=c.Level,owned=f.Count(c.Id)}).ToArray(),
  inventory=f.S.Inventory.Where(p=>p.Value>0).Select(p=>new{id=p.Key,name=L.T(Farm.ItemName(p.Key)),count=p.Value,price=Farm.Price(p.Key)}).ToArray(),
  recipes=Farm.Recipes.Select(r=>new{id=r.Id,name=L.T(r.Name),seconds=r.Seconds,building=L.T(Farm.BuildingName(r.Building)),level=r.Level,can=!f.ProductionPaused&&f.Has(r.Building)&&f.Level>=r.Level&&f.Queue(r.Building).Count<f.S.Buildings[r.Building]+1&&r.Inputs.All(p=>f.Count(p.Key)>=p.Value),ingredients=string.Join(" · ",r.Inputs.Select(p=>L.T(Farm.ItemName(p.Key))+" "+f.Count(p.Key)+"/"+p.Value))}).ToArray(),
  jobs=f.S.Queues.SelectMany(q=>q.Value.Select(j=>new{name=L.T(Farm.ItemName(j.Recipe)),seconds=f.ProductionRemaining(j.Ready)})).ToArray(),
  orders=f.S.OrdersList.Select((o,i)=>new{id=i,name=L.T(o.Person),coins=o.Coins,xp=o.Xp,can=f.CanOrder(o),items=string.Join(" · ",o.Items.Select(p=>L.T(Farm.ItemName(p.Key))+" "+f.Count(p.Key)+"/"+p.Value))}).ToArray(),
  buildings=Farm.BuildingIds.Select((id,i)=>new{id=id,name=L.T(Farm.BuildingName(id)),owned=f.Has(id),tier=f.Has(id)?f.S.Buildings[id]:0,level=Farm.BuildingLevels[i],cost=f.Has(id)?f.UpgradeCost(id):Farm.BuildingCosts[i],can=f.CanBuild(id),busy=f.S.Construction.ContainsKey(id),seconds=f.S.Construction.ContainsKey(id)?Math.Max(0,f.S.Construction[id].Ready-f.Now):f.BuildSeconds(id,f.Has(id)?f.S.Buildings[id]+1:1),target=f.S.Construction.ContainsKey(id)?f.S.Construction[id].TargetLevel:f.Has(id)?Math.Min(3,f.S.Buildings[id]+1):1,queued=f.Queue(id).Count>0}).ToArray(),
  decor=new[]{"path","flowers","fence","lamp","bench","tree","fountain"}.Select(id=>new{id=id,name=L.T(Farm.DecorName(id)),cost=Farm.DecorCost(id)}).ToArray(),guide=L.T(f.GuideTitle),guideDone=f.GuideDone,lighting=f.S.Lighting
 };}
 public static string Apply(Farm f,WallpaperForm scene,Command c){if(c==null)return "";switch(c.action){
  case "navigate":scene.Navigate(c.id);return "";
  case "map":int ox=f.S.CameraX,oy=f.S.CameraY;string result=WorldMap.Select(f,c.id);int nx=f.S.CameraX,ny=f.S.CameraY;f.S.CameraX=ox;f.S.CameraY=oy;scene.PanTo(nx,ny);return result;
  case "quiet":if(c.id=="on"||c.id=="off")f.S.Quiet=c.id=="on";return "";
  case "zoom":int zoom;if(int.TryParse(c.id,out zoom)&&new[]{50,75,85,100,125,150}.Contains(zoom))f.S.HudZoom=zoom;return "";
  case "seed":var crop=Farm.Crop(c.id);if(crop!=null&&crop.Level<=f.Level)f.S.SelectedSeed=c.id;return "";
  case "plot":int plot;if(int.TryParse(c.id,out plot)&&plot>=0&&plot<f.S.Plots.Count){var p=f.S.Plots[plot];return p.Crop==""?f.Plant(plot,f.S.SelectedSeed):p.Ready<=f.Now?f.Harvest(plot):f.Water(plot);}return "";
  case "animal":int animal;if(int.TryParse(c.id,out animal))return f.AnimalAction(animal);return "";
  case "harvest":return f.HarvestAll();case "plant":return f.PlantAll(f.S.SelectedSeed);
  case "sell":return f.Count(c.id)>0?f.Sell(c.id,1):"";
  case "craft":return Farm.GetRecipe(c.id)!=null?f.Craft(c.id):"";
  case "order":int index;if(int.TryParse(c.id,out index))return f.Deliver(index);return "";
  case "build":return Array.IndexOf(Farm.BuildingIds,c.id)>=0?f.BuyBuilding(c.id):"";
  case "expand":return f.Expand();case "guide":return f.ClaimGuide();case "daily":return f.Daily();
  case "edit":scene.ClientInput("toggle",0,0);return "";
  case "decor":if(new[]{"path","flowers","fence","lamp","bench","tree","fountain"}.Contains(c.id))scene.ChooseDecoration(c.id);return "";
  case "cancel":scene.ClientInput("cancel",0,0);return "";
  case "language":f.S.Language=c.id=="en"?"en":"zh-CN";L.SetLanguage(f.S.Language);return "";
  case "weather":if(new[]{"自动","晴天","雨天","雪天"}.Contains(c.id))f.S.Weather=c.id;return "";
  case "light":if(new[]{"白天","夜晚","跟随时间"}.Contains(c.id))f.S.Lighting=c.id;return "";
  case "call":scene.CallAnimals();return "";
 }return "";}
}
}
