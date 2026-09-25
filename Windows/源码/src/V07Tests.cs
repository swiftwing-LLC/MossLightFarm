using System;
using System.Drawing;
namespace DesktopFarm {
public static class V07Tests {
 public static void Run(Action<bool,string> check){
 var p=NativeDesktop.Normalize(new Point(1920,1080),new Rectangle(0,0,3840,2160));check(Math.Abs(p.X-.5)<.001&&Math.Abs(p.Y-.5)<.001,"Physical desktop coordinates normalize correctly on a 4K monitor");
 p=NativeDesktop.Normalize(new Point(-960,540),new Rectangle(-1920,0,1920,1080));check(Math.Abs(p.X-.5)<.001&&Math.Abs(p.Y-.5)<.001,"Negative monitor origins preserve drag coordinates");
 var farm=new Farm(()=>2000000000);using(var scene=new WallpaperForm(farm,s=>{},()=>{},true)){scene.ExternalHud=true;scene.EditMode=false;scene.SceneSnapshot(960,540).Dispose();scene.ClientInput("down",309/960.0,375/540.0);scene.ClientInput("move",430/960.0,210/540.0);scene.ClientInput("up",430/960.0,210/540.0);check(farm.S.DesktopLayout.ContainsKey("cabin")&&farm.S.DesktopLayout["cabin"].Y<.5,"Direct native game input moves and persists the house without browser mouse events");}
 }
}
}
