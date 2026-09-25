using System;
using System.Drawing;
using System.Web.Script.Serialization;
namespace DesktopFarm {
public static class V04Tests {
 public static void Run(string path,Action<bool,string> check){double now=2000000000;var f=new Farm(()=>now);int opened=0;using(var scene=new WallpaperForm(f,s=>opened++,()=>{},true)){scene.ExternalHud=true;scene.EditMode=false;
  DesktopHud.Apply(f,scene,new DesktopHud.Command{action="seed",id="pumpkin"});check(f.S.SelectedSeed=="wheat","Wallpaper seed picker rejects locked crops");
  int cash=f.S.Coins;DesktopHud.Apply(f,scene,new DesktopHud.Command{action="seed",id="invalid"});check(f.S.Coins==cash&&f.S.SelectedSeed=="wheat","Invalid HUD seed cannot spend currency or corrupt selection");
  DesktopHud.Apply(f,scene,new DesktopHud.Command{action="craft",id="flour"});check(f.Queue("mill").Count==1&&f.Count("wheat")==4,"Inline workshop uses the same recipe costs and queue");
  now+=Farm.GetRecipe("flour").Seconds+1;f.Tick();check(f.Count("flour")==1,"Inline workshop production completes into shared inventory");
  cash=f.S.Coins;DesktopHud.Apply(f,scene,new DesktopHud.Command{action="sell",id="flour"});check(f.S.Coins==cash+Farm.Price("flour")&&f.Count("flour")==0,"Inline storage sale immediately updates currency and stock");
  cash=f.S.Coins;DesktopHud.Apply(f,scene,new DesktopHud.Command{action="sell",id="flour"});check(f.S.Coins==cash,"Inline sale cannot sell an empty stack twice");
  f.S.Inventory["wheat"]=50;int orders=f.S.Orders;DesktopHud.Apply(f,scene,new DesktopHud.Command{action="order",id="0"});check(f.S.Orders==orders+1,"Inline order consumes goods and rewards completion");
  string before=new JavaScriptSerializer().Serialize(f.S);DesktopHud.State(f,scene,"",0);check(new JavaScriptSerializer().Serialize(f.S)==before,"Polling the HUD never mutates farm progress");
  scene.ClientInput("down",.8,.06);scene.ClientInput("up",.8,.06);check(opened==0,"Invisible legacy banner cannot intercept new wallpaper controls");
  scene.ClientInput("down",309/960.0,375/540.0);scene.ClientInput("up",130/960.0,185/540.0);check(f.S.DesktopLayout.ContainsKey("cabin"),"Fast dragging moves the house even if intermediate pointer events are coalesced");
  DesktopHud.Apply(f,scene,new DesktopHud.Command{action="edit"});check(scene.EditMode,"Inline arrangement toggles the actual desktop editor");
  DesktopHud.Apply(f,scene,new DesktopHud.Command{action="language",id="en"});check(f.S.Language=="en"&&L.Code=="en","Inline language change updates persistent and displayed language");
  using(var bmp=scene.SceneSnapshot(960,540))check(bmp.GetPixel(800,25).ToArgb()!=PixelArt.C("3d5241").ToArgb(),"Desktop scene no longer paints the oversized header banner");
  DesktopHud.Apply(f,scene,new DesktopHud.Command{action="language",id="zh-CN"});
 }}
}
}
