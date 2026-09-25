using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
namespace DesktopFarm {
public class LayoutPoint {public double X,Y;}
public class WorldObject {
 public string Id,Kind,Type;public int Index=-1,Length=48;public Point Foot;
 public Rectangle Bounds {get{int x=Foot.X,y=Foot.Y;switch(Kind){case "plot":return new Rectangle(x-12,y-34,26,37);case "cabin":return new Rectangle(x-46,y-82,92,94);case "mill":return new Rectangle(x-48,y-108,96,115);case "building":return new Rectangle(x-32,y-70,64,88);case "pond":return new Rectangle(x-54,y-28,108,65);case "pine":return new Rectangle(x-26,y-64,52,69);case "tree":return new Rectangle(x-32,y-74,64,82);case "fence":return new Rectangle(x-Length/2-2,y-13,Length+4,19);case "path":return new Rectangle(x-12,y-12,24,24);default:return Type=="tree"?new Rectangle(x-32,y-74,64,82):new Rectangle(x-18,y-38,36,46);}}}
 public Rectangle Solid {get{int x=Foot.X,y=Foot.Y;switch(Kind){case "plot":return new Rectangle(x-11,y-20,22,20);case "path":return new Rectangle(x-10,y-8,20,16);case "cabin":return new Rectangle(x-36,y-36,72,43);case "mill":return new Rectangle(x-22,y-39,44,44);case "building":return new Rectangle(x-24,y-25,48,31);case "pond":return new Rectangle(x-49,y-22,98,50);case "tree":case "pine":return new Rectangle(x-9,y-10,18,15);case "fence":return new Rectangle(x-Length/2,y-4,Length,8);default:return Rectangle.Empty;}}}
 public string Label {get{return Type=="wildflowers"?"野花丛":Kind=="plot"?"田地 "+(Index+1):Kind=="cabin"?(Type=="bakery"?Farm.BuildingName(Type):"木屋"):Kind=="mill"?"风车磨坊":Kind=="building"?Farm.BuildingName(Type):Kind=="pond"?"池塘":Kind=="pine"?"松树":Kind=="tree"?"苹果树":Kind=="path"?"道路":Kind=="fence"?"木围栏":Farm.DecorName(Type);}}
 public void Draw(Graphics g,Farm f,double t,bool selected){int x=Foot.X,y=Foot.Y;if(Id.StartsWith("scenery:tree:")&&Kind=="tree"){AmbientArt.Tree(g,x,y,Index,f.IsNight);return;}if(Type=="wildflowers"){AmbientArt.Flowers(g,x,y,Index,f.IsNight);return;}if(Kind=="cabin"||Kind=="mill"||Kind=="building"){BuildingArt.ForFarm(g,f,x,y,Id,t);return;}switch(Kind){case "plot":var p=f.S.Plots[Index];PixelArt.Crop(g,x-11,y-20,p.Crop,p.Crop==""?0:(f.Now-p.Planted)/Math.Max(1,p.Ready-p.Planted),p.Watered,selected,t);break;case "cabin":PixelArt.Cabin(g,x,y,f.IsNight,Type=="bakery"?1:0);break;case "mill":PixelArt.Mill(g,x,y,t,f.IsNight);break;case "building":PixelArt.SmallBuilding(g,x,y,Type,f.IsNight);break;case "pond":PixelArt.Pond(g,x,y,t);break;case "tree":PixelArt.Tree(g,x,y,1,true);break;case "pine":PixelArt.Pine(g,x,y);break;case "fence":PixelArt.Fence(g,x-Length/2,y,Length);break;case "path":LandscapeArt.Path(g,x,y);break;default:PixelArt.Decor(g,Type,x,y,f.IsNight);break;}}
}
public static class FreeLayout {
 public static List<WorldObject> Objects(Farm f,int width,int height){var list=new List<WorldObject>();int ox=(width-600)/2,oy=height-330;ox-=f.S.CameraX;oy-=f.S.CameraY;
  Action<string,string,string,int,int,int> add=(id,kind,type,x,y,index)=>{var item=new WorldObject{Id=id,Kind=kind,Type=type,Foot=new Point(ox+x,oy+y),Index=index};LayoutPoint p;if(f.S.DesktopLayout.TryGetValue(id,out p))item.Foot=new Point((int)Math.Round(p.X*width)-f.S.CameraX,(int)Math.Round(p.Y*height)-f.S.CameraY);list.Add(item);};
  add("cabin","cabin","home",129,184,-1);add("mill","mill","mill",424,184,-1);add("pond","pond","pond",501,241,-1);
  foreach(var id in Farm.BuildingIds){if(id=="mill"||id=="cabin"||(!f.Has(id)&&!f.S.Construction.ContainsKey(id)))continue;int x=id=="bakery"?82:id=="coop"?169:id=="barn"?421:id=="dairy"?284:id=="loom"?226:id=="sugarhouse"?342:id=="preserves"?499:90;int y=id=="bakery"?270:id=="coop"?270:id=="barn"?275:id=="loom"?105:id=="dairy"||id=="sugarhouse"?105:id=="preserves"?164:80;add(id,id=="bakery"?"cabin":"building",id,x,y,-1);}
  // Keep new plots on the open meadow beside the town. Existing saved placements are preserved.
  for(int i=0;i<f.S.Plots.Count;i++)add("plot:"+i,"plot","plot",620+(i%9)*24,120+(i/9)*23,i);
  int[,] trees={{47,185},{552,173},{201,145},{40,273},{569,290}};for(int i=0;i<5;i++)add("tree:"+i,"tree","tree",trees[i,0],trees[i,1],-1);
  for(int i=0;i<6;i++)add("pine:"+i,"pine","pine",i<3?28+i*25:458+(i-3)*25,122+(i%3)*4,-1);
  add("fence:0","fence","fence",121,209,-1);add("fence:1","fence","fence",416,222,-1);add("lamp:0","decor","lamp",197,220,-1);add("bench:0","decor","bench",558,230,-1);
  for(int i=0;i<10;i++)add("path:"+i,"path","path",178+i*24,205,-1);
  for(int i=0;i<f.S.Decorations.Count;i++){var d=f.S.Decorations[i];add("decor:"+(string.IsNullOrEmpty(d.Key)?"legacy:"+i:d.Key),d.Type=="path"?"path":"decor",d.Type,d.X,d.Y,i);}
  list.AddRange(Scenery.Objects(f,width,height));
  return list;
 }
 public static Point Clamp(WorldObject item,Point desired,int width,int height){Rectangle r=item.Bounds;int left=item.Foot.X-r.Left,right=r.Right-item.Foot.X,top=item.Foot.Y-r.Top,bottom=r.Bottom-item.Foot.Y;return new Point(Math.Max(left+6,Math.Min(width-right-6,(int)Math.Round(desired.X/2.0)*2)),Math.Max(top+60,Math.Min(height-bottom-66,(int)Math.Round(desired.Y/2.0)*2)));}
 public static bool CanPlace(WorldObject item,Point point,IEnumerable<WorldObject> objects){Point old=item.Foot;item.Foot=point;Rectangle area=item.Solid;item.Foot=old;if(area.IsEmpty)return true;return !objects.Any(o=>o.Id!=item.Id&&!o.Solid.IsEmpty&&o.Solid.IntersectsWith(area));}
 public static bool Move(Farm f,string id,Point desired,int width,int height){var all=Objects(f,width,height);var item=all.FirstOrDefault(x=>x.Id==id);if(item==null)return false;var pos=Clamp(item,desired,width,height);if(!CanPlace(item,pos,all))return false;Point old=item.Foot;item.Foot=pos;bool dry=WorldMap.Dry(f,item.Solid.IsEmpty?item.Bounds:item.Solid);item.Foot=old;if(!dry)return false;f.S.DesktopLayout[id]=new LayoutPoint{X=(pos.X+f.S.CameraX)/(double)width,Y=(pos.Y+f.S.CameraY)/(double)height};return true;}
 public static Bitmap Meadow(Farm f,int width,int height){return LandscapeArt.Meadow(f,width,height);}
}
}
