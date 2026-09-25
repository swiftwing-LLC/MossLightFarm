using System;
using System.IO;
using System.Web.Script.Serialization;
namespace DesktopFarm {
public static class V06Tests {
 public static void Run(string path,Action<bool,string> check){
 check(GameForm.ShouldStayInTray(System.Windows.Forms.CloseReason.UserClosing,true,false,false),"Closing the management window retains an active wallpaper");
 check(!GameForm.ShouldStayInTray(System.Windows.Forms.CloseReason.WindowsShutDown,true,false,false),"Windows shutdown is never cancelled by close-to-tray");
 check(!GameForm.ShouldStayInTray(System.Windows.Forms.CloseReason.UserClosing,true,true,false),"Explicit save-and-exit still quits");
 check(!GameForm.ShouldStayInTray(System.Windows.Forms.CloseReason.UserClosing,false,false,false),"Standalone game without wallpaper can close normally");
 check(!GameForm.ShouldStayInTray(System.Windows.Forms.CloseReason.UserClosing,true,false,true),"Test forms are not held open in the tray");
 check(StartupManager.StartupCommand(@"C:\Users\A & B\Farm\MosslightFarm.exe")=="\"C:\\Users\\A & B\\Farm\\MosslightFarm.exe\" --startup","Startup command quotes paths with spaces and ampersands");
 check(LiveFarmHost.ShouldRecover(31,121,true,true),"Stalled visible farm schedules recovery");
 check(!LiveFarmHost.ShouldRecover(31,121,false,true),"Paused hidden wallpaper does not trigger recovery");
 check(!LiveFarmHost.ShouldRecover(31,121,true,false),"Recovery never overrides a different wallpaper");
 check(!LiveFarmHost.ShouldRecover(31,90,true,true),"Recovery backs off instead of reopening repeatedly");
 check(!LiveFarmHost.ShouldRecover(10,121,true,true),"Healthy wallpaper is not restarted");
 var f=new Farm(()=>2000000000);using(var scene=new WallpaperForm(f,s=>{},()=>{},true)){
 double ready=f.S.Plots[0].Ready;DesktopHud.Apply(f,scene,new DesktopHud.Command{action="quiet",id="on"});check(f.S.Quiet&&f.S.Plots[0].Ready==ready,"Power saving changes animation preference without changing crop timers");DesktopHud.Apply(f,scene,new DesktopHud.Command{action="quiet",id="off"});check(!f.S.Quiet,"Power saving can be disabled from the wallpaper");
 foreach(string weather in new[]{"晴天","雨天","雪天"}){DesktopHud.Apply(f,scene,new DesktopHud.Command{action="weather",id=weather});check(LandscapeArt.Weather(f)==weather,"Weather selection applies: "+weather);}
 DesktopHud.Apply(f,scene,new DesktopHud.Command{action="weather",id="invalid"});check(f.S.Weather=="雪天","Invalid weather cannot overwrite preference");
 string file=Path.Combine(path,"weather.json");f.Save(file);var copy=new Farm(()=>2000000000);copy.Load(file);check(copy.S.Weather=="雪天","Weather preference survives restart");
 foreach(string lighting in new[]{"白天","夜晚"})foreach(string weather in new[]{"晴天","雨天","雪天"}){f.S.Lighting=lighting;f.S.Weather=weather;string before=new JavaScriptSerializer().Serialize(f.S);using(var frame=scene.SceneSnapshot(960,540)){check(frame.Width==960&&frame.GetPixel(0,0).A==255,"Day/night weather scene has an opaque background");}check(new JavaScriptSerializer().Serialize(f.S)==before,"Atmospheric rendering preserves economy and layout");}
 }
 }
}
}
