using System;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Web.Script.Serialization;
namespace DesktopFarm {
public static class V03Tests {
 public static void Run(string path,Action<bool,string> check){var f=new Farm(()=>2000000000);var json=new JavaScriptSerializer();int coins=f.S.Coins;string crops=json.Serialize(f.S.Plots);
  var initialFields=FreeLayout.Objects(f,960,540).Where(o=>o.Kind=="plot").ToArray();check(initialFields.Length==f.S.Plots.Count&&initialFields.All(o=>!FreeLayout.Objects(f,960,540).Any(r=>r.Kind=="path"&&r.Solid.IntersectsWith(o.Solid))),"Unlocked fields stay visible on wallpaper without covering roads");
  check(FreeLayout.Move(f,"tree:0",new Point(120,180),960,540),"A tree can move outside the original farm footprint");
  check(FreeLayout.Move(f,"cabin",new Point(440,170),960,540),"The house can move to the upper desktop");
  check(f.S.Coins==coins&&json.Serialize(f.S.Plots)==crops,"Moving owned objects never charges money or resets crop timers");
  var plot=FreeLayout.Objects(f,960,540).First(o=>o.Id=="tree:0");check(plot.Foot==new Point(120,180),"Moved tree rendering and hit target share its new coordinates");
  string oldLayout=json.Serialize(f.S.DesktopLayout);check(!FreeLayout.Move(f,"cabin",new Point(120,185),960,540)&&json.Serialize(f.S.DesktopLayout)==oldLayout,"Invalid overlap is rejected without altering the saved layout");
  string file=Path.Combine(path,"layout-save.json");f.Save(file);var restored=new Farm(()=>2000000010);restored.Load(file);check(restored.S.DesktopLayout.Count==2&&restored.S.DesktopLayout["tree:0"].X==.125,"Free layout survives save and restart");
  check(FreeLayout.Objects(restored,1920,1080).First(o=>o.Id=="tree:0").Foot==new Point(240,360),"Layout adapts proportionally to another screen size");
  f.BuyDecoration("path",390,185);var decoration=f.S.Decorations.Last();string id="decor:"+decoration.Key;check(f.S.Coins==coins-5&&decoration.Key.Length>0,"Paths are affordable and decorations have stable identities");
  check(FreeLayout.Move(f,id,new Point(700,170),960,540),"A newly purchased decoration can move anywhere on the desktop");
  Farm.Validate(f.S);check(f.S.DesktopLayout.ContainsKey(id),"Save validation preserves decoration layout identities");
  var animal=new Wanderer("cat",-1,new Point(55,110));animal.TargetX=145;animal.TargetY=110;var block=new Rectangle(80,90,40,40);bool entered=false;var random=new Random(8);for(int i=0;i<2000;i++){animal.Step(.1,new Rectangle(22,65,916,405),random,new[]{block});entered|=block.Contains((int)animal.X,(int)animal.Y);}check(!entered,"Companions route around moved object footprints");
  var interactive=new Farm(()=>2000000000);using(var screen=new WallpaperForm(interactive,s=>{},()=>{},true)){DesktopHud.Apply(interactive,screen,new DesktopHud.Command{action="plot",id="0"});check(interactive.S.Harvests==1&&interactive.S.Plots[0].Crop=="","Fields page harvest action collects a ripe crop");int money=interactive.S.Coins;DesktopHud.Apply(interactive,screen,new DesktopHud.Command{action="plot",id="0"});check(interactive.S.Coins==money-Farm.Crop("wheat").Cost&&interactive.S.Plots[0].Crop=="wheat","Fields page replants and charges once");money=interactive.S.Coins;DesktopHud.Apply(interactive,screen,new DesktopHud.Command{action="plot",id="0"});check(interactive.S.Coins==money&&interactive.S.Plots[0].Watered,"Clicking a growing crop waters without buying twice");DesktopHud.Apply(interactive,screen,new DesktopHud.Command{action="plot",id="81"});check(interactive.S.Plots.Count==12,"Locked fields cannot be planted by forged input");}
  using(var scene=new WallpaperForm(f,s=>{},()=>{},true))using(var bitmap=scene.SceneSnapshot(960,540)){bitmap.Save(Path.Combine(path,"free-layout.png"));check(bitmap.Width==960&&bitmap.GetPixel(0,0).A==255,"Custom layouts render as a complete wallpaper");}
 }
}
}
