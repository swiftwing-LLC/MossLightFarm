using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
namespace DesktopFarm {
static class MaintenanceTests {
 public static void Run(string path){Directory.CreateDirectory(path);var lines=new List<string>();int passed=0;Action<bool,string> check=(ok,name)=>{if(!ok)throw new Exception("FAIL: "+name);passed++;lines.Add("PASS  "+name);};
  try{
   double now=2000000000;var f=new Farm(()=>now);
   check(Farm.MaintenanceBuild,"Maintenance flavor is enabled");
   check(PlayerStorage.Root.EndsWith("MosslightFarm-Maintenance",StringComparison.OrdinalIgnoreCase),"Maintenance save root is isolated from player saves");
   check(f.S.Coins==999999999&&f.CanAfford(int.MaxValue),"Fresh maintenance farm has unlimited funds");
   check(Farm.BuildingIds.All(id=>f.BuildSeconds(id,1)==60&&f.BuildSeconds(id,2)==60&&f.BuildSeconds(id,3)==60),"Every building and upgrade takes exactly one minute");
   f.S.Coins=0;string planting=f.Plant(4,"wheat");check(f.S.Plots[4].Crop=="wheat"&&f.S.Coins==0&&!String.IsNullOrEmpty(planting),"Planting works without spending coins");
   f.S.Xp=Farm.Threshold(2);string building=f.BuyBuilding("bakery");check(f.S.Construction.ContainsKey("bakery")&&f.S.Coins==0&&!String.IsNullOrEmpty(building),"Building purchase works without spending coins");
   check(Math.Abs(f.S.Construction["bakery"].Ready-now-60)<.01&&f.ProductionPaused,"Construction timer and production pause are applied");
   string blocked=f.Craft("flour");check(blocked.Contains("施工中")&&f.Queue("mill").Count==0,"Production stays blocked during construction");
   now+=61;f.Tick();check(f.Has("bakery")&&!f.ProductionPaused,"Building completes after one minute");
   check(f.CanBuild("bakery"),"Free upgrade is available with unlimited funds");int beforeUpgrade=f.S.Coins;f.BuyBuilding("bakery");check(Math.Abs(f.S.Construction["bakery"].Ready-now-60)<.01&&f.S.Coins==beforeUpgrade,"Building upgrades also take one minute without spending coins");
   f.S.Coins=999999999;using(var ui=new GameForm(f,Path.Combine(path,"unused.json"),true)){using(var image=ui.RenderPanelSnapshot("fields",""))image.Save(Path.Combine(path,"maintenance-fields.png"));var activeHits=(List<Hit>)typeof(GameForm).GetField("hits",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(ui);check(!activeHits.Any(h=>h.Bounds.Width==34&&h.Bounds.Height==34),"Maintenance farm page has no land-tile controls");}
   Farm.Validate(f.S);check(true,"Maintenance save data remains valid");lines.Add("\nTOTAL: "+passed+" passed, 0 failed");File.WriteAllLines(Path.Combine(path,"test-results.txt"),lines,Encoding.UTF8);Environment.ExitCode=0;
  }catch(Exception e){lines.Add(e.ToString());File.WriteAllLines(Path.Combine(path,"test-results.txt"),lines,Encoding.UTF8);Environment.ExitCode=1;}
 }
}
}
