using System;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Threading;
namespace DesktopFarm {
public static class V010Tests {
 public static void Run(string path,Action<bool,string> check){var f=new Farm(Farm.UtcNow);f.S.Lighting="白天";f.S.Weather="雨天";f.S.Animals.Add(new Animal{Type="chicken"});f.S.Animals.Add(new Animal{Type="cow"});
 var blocks=new List<Rectangle>{new Rectangle(100,80,80,90)};var a=new Wanderer("cow",0,new Point(140,120));var rng=new Random(127);for(int i=0;i<1000;i++)a.Step(.08,new Rectangle(0,0,400,300),rng,blocks);check(a.WalkTime>20&&!Wanderer.Blocked(a.X,a.Y,blocks),"Animal escapes an overlapping object and continues walking");
 using(var scene=new WallpaperForm(f,s=>{},()=>{},true)){scene.ClientSize=new Size(960,540);scene.ExternalHud=true;scene.EditMode=false;using(var b=scene.SceneSnapshot(960,540,true)){}var before=scene.ActorPositions;for(int i=0;i<70;i++){Thread.Sleep(50);using(var b=scene.SceneSnapshot(960,540,true)){} }var after=scene.ActorPositions;check(before.Zip(after,(b,e)=>Math.Abs(b.X-e.X)+Math.Abs(b.Y-e.Y)>1).All(v=>v),"Every animal advances through delivered frames, even with no window message loop and in rain");
 var all=FreeLayout.Objects(f,960,540);Point empty=Point.Empty;for(int y=170;y<260&&empty.IsEmpty;y+=20)for(int x=40;x<920;x+=20)if(!all.Any(o=>o.Bounds.Contains(x,y))&&!after.Any(o=>Math.Abs(o.X-x)<30&&Math.Abs(o.Y-y)<40)){empty=new Point(x,y);break;}check(!empty.IsEmpty,"There is accessible empty ground for direct panning");scene.ClientInput("down",empty.X/960.0,empty.Y/540.0);scene.ClientInput("move",(empty.X-40)/960.0,(empty.Y-20)/540.0);scene.ClientInput("up",(empty.X-40)/960.0,(empty.Y-20)/540.0);check(f.S.CameraX==40&&f.S.CameraY==20&&!scene.PanMode,"Empty-ground drag pans immediately without a mode or settings");check(f.S.DesktopLayout.Count==0,"Panning does not move saved objects");
 scene.PanTo(-800,-200);var tree=FreeLayout.Objects(f,960,540).First(o=>o.Id.StartsWith("scenery:tree:")&&o.Foot.X>100&&o.Foot.X<700&&o.Foot.Y>150&&o.Foot.Y<380);bool moved=false;for(int y=170;y<420&&!moved;y+=30)for(int x=100;x<820&&!moved;x+=30)moved=FreeLayout.Move(f,tree.Id,new Point(x,y),960,540);check(moved&&f.S.DesktopLayout.ContainsKey(tree.Id),"Outer scenery tree has an editable saved world position");string file=Path.Combine(path,"scenery-save.json");f.Save(file);var restored=new Farm(Farm.UtcNow);restored.Load(file);check(restored.S.DesktopLayout[tree.Id].X==f.S.DesktopLayout[tree.Id].X,"Outer tree placement survives save and load");
 f.S.Weather="晴天";scene.PanTo(0,0);using(var b=scene.SceneSnapshot(960,540,true))b.Save(Path.Combine(path,"living-world.png"));}
 var world=AmbientArt.Flight(15,2,new PointF(200,300),70);f.S.CameraX=0;f.S.CameraY=0;var screenA=AmbientArt.Screen(world,f);f.S.CameraX=120;f.S.CameraY=-40;var screenB=AmbientArt.Screen(world,f);check(Math.Abs(screenB.X-screenA.X+120)<.01&&Math.Abs(screenB.Y-screenA.Y-40)<.01,"Butterfly world position is independent of camera translation");var later=AmbientArt.Flight(22,2,new PointF(200,300),70);check(later!=world,"Butterflies follow changing smooth world paths");
 f.S.Coins=20000;f.S.DesktopLayout.Clear();WorldMap.Select(f,"river");check(f.S.Map=="river","Unmoved scenery does not prevent buying river terrain");WorldMap.Select(f,"lake");check(f.S.Map=="lake","Unmoved scenery adapts to lake terrain without blocking purchase");
 using(var sheet=new Bitmap(960,340))using(var g=Graphics.FromImage(sheet)){g.Clear(PixelArt.C("8ba369"));for(int i=0;i<4;i++){AmbientArt.Tree(g,70+i*120,130,i+1,false);AmbientArt.Flowers(g,70+i*120,170,i,false);}var s=g.Save();g.TranslateTransform(80,230);g.ScaleTransform(4,4);AmbientArt.Butterfly(g,0,0,1,1,false);AmbientArt.Bee(g,35,0,1);AmbientArt.Bird(g,75,0,1,false,1,false);AmbientArt.Bird(g,130,0,1,true,1,false);g.Restore(s);sheet.Save(Path.Combine(path,"wildlife-art.png"));}
 }
}
}
