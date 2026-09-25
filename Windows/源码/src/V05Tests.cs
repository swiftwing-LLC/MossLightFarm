using System;
using System.Drawing;
using System.IO;
namespace DesktopFarm {
public static class V05Tests {
 static bool Same(Bitmap a,Bitmap b,Rectangle r){for(int y=r.Top;y<r.Bottom;y++)for(int x=r.Left;x<r.Right;x++)if(a.GetPixel(x,y)!=b.GetPixel(x,y))return false;return true;}
 public static void Run(string path,Action<bool,string> check){var f=new Farm(()=>2000000000);using(var scene=new WallpaperForm(f,s=>{},()=>{},true)){scene.ExternalHud=true;scene.EditMode=false;
  using(var before=scene.SceneSnapshot(960,540)){scene.ClientInput("down",309/960.0,375/540.0);scene.ClientInput("move",120/960.0,200/540.0);using(var drag=scene.SceneSnapshot(960,540)){check(!Same(before,drag,new Rectangle(275,330,60,55)),"Dragging removes the house from its original position, without a duplicate ghost");check(!Same(before,drag,new Rectangle(85,165,70,65)),"The actual house sprite follows the pointer during a drag");}
   check(f.S.DesktopLayout.Count==0,"A drag preview does not persist an unfinished move");scene.ClientInput("cancel",0,0);using(var cancelled=scene.SceneSnapshot(960,540))check(Same(before,cancelled,new Rectangle(0,70,960,400)),"Cancelling a direct drag restores the original scene without a selection frame");}
  DesktopHud.Apply(f,scene,new DesktopHud.Command{action="zoom",id="150"});string file=Path.Combine(path,"hud-zoom.json");f.Save(file);var loaded=new Farm(()=>2000000000);loaded.Load(file);check(loaded.S.HudZoom==150,"Interface size persists across save and restart");
  DesktopHud.Apply(f,scene,new DesktopHud.Command{action="zoom",id="999"});check(f.S.HudZoom==150,"Unsupported zoom cannot hide controls with an extreme scale");
 }}
}
}
