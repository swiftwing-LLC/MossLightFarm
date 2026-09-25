using System;
using System.Linq;
using System.Drawing;
namespace DesktopFarm {
public static class WorldMap {
 public const int Width=2880,Height=1620;
 public static readonly string[] Ids={"meadow","river","lake"};
 public static readonly int[] Costs={0,2500,6000};
 public static string Name(string id){return L.T(id=="river"?"清溪河湾":id=="lake"?"松林湖畔":"原野草甸");}
 public static string Description(string id){return L.T(id=="river"?"蜿蜒溪流、两座木桥与芦苇河岸。":id=="lake"?"湖心小岛、松林与开阔草地。":"九屏大小的开放草甸，适合自由建造。");}
 public static int Cost(string id){int i=Array.IndexOf(Ids,id);return i<0?int.MaxValue:Costs[i];}
 public static int RiverX(int y){return 980+(int)(Math.Sin((y+540)*.005)*80);}
 public static bool Water(string map,int x,int y){if(map=="river")return Math.Abs(x-RiverX(y))<55&&Math.Abs(y-180)>21&&Math.Abs(y-780)>21;if(map=="lake"){double a=(x+440)/260.0,b=(y-70)/185.0;return a*a+b*b<1&&((x+455)*(x+455)/3600.0+(y-60)*(y-60)/1600.0>1);}return false;}
 public static bool Dry(Farm f,Rectangle area,string map=null){if(area.IsEmpty)return true;for(int y=area.Top;y<=area.Bottom;y+=4)for(int x=area.Left;x<=area.Right;x+=4)if(Water(map??f.S.Map,x+f.S.CameraX,y+f.S.CameraY))return false;return true;}
 public static string Select(Farm f,string id){
  if(!Ids.Contains(id))return "";if(!f.S.OwnedMaps.Contains(id)&&f.S.Coins<Cost(id))return L.T("金币不足，先继续经营农场吧。");
  string oldMap=f.S.Map;int oldX=f.S.CameraX,oldY=f.S.CameraY;var oldLayout=new System.Collections.Generic.Dictionary<string,LayoutPoint>(f.S.DesktopLayout);
  f.S.Map=id;f.S.CameraX=id=="river"?320:id=="lake"?-850:0;f.S.CameraY=id=="lake"?-200:0;
  var wet=FreeLayout.Objects(f,960,540).Where(o=>o.Kind=="plot"&&!Dry(f,o.Solid,id)).ToArray();
  foreach(var plot in wet){bool placed=false;for(int radius=1;radius<=20&&!placed;radius++)for(int dy=-radius;dy<=radius&&!placed;dy++)for(int dx=-radius;dx<=radius;dx++){if(Math.Max(Math.Abs(dx),Math.Abs(dy))!=radius)continue;if(FreeLayout.Move(f,plot.Id,new Point(plot.Foot.X+dx*24,plot.Foot.Y+dy*23),960,540)){placed=true;break;}}if(!placed){f.S.Map=oldMap;f.S.CameraX=oldX;f.S.CameraY=oldY;f.S.DesktopLayout=oldLayout;return L.T("这片地图的水边没有空位，请先整理农田或装饰。");}}
  if(FreeLayout.Objects(f,960,540).Any(o=>o.Kind!="plot"&&(!o.Id.StartsWith("scenery:")||f.S.DesktopLayout.ContainsKey(o.Id))&&!Dry(f,o.Solid.IsEmpty?o.Bounds:o.Solid,id))){f.S.Map=oldMap;f.S.CameraX=oldX;f.S.CameraY=oldY;f.S.DesktopLayout=oldLayout;return L.T("有物品位于新地图的水面上，请先移开再切换。");}
  if(!f.S.OwnedMaps.Contains(id)){f.S.Coins-=Cost(id);f.S.OwnedMaps.Add(id);}return L.T("地图已切换，农场与布局已保留。");
 }
 public static Bitmap Backdrop(Farm f){var b=LandscapeArt.Meadow(f,Width,Height);using(var g=Graphics.FromImage(b)){g.TranslateTransform(960,540);bool night=f.IsNight;string grass=night?"344d50":"8ba369",water=night?"355f70":"629fa9",light=night?"547884":"9ac8c0";var rng=new Random(7301);
  if(f.S.Map=="river"){for(int y=-540;y<1080;y+=4){int x=RiverX(y);PixelArt.R(g,night?"647761":"c5c492",x-71,y,142,4);PixelArt.R(g,water,x-56,y,112,4);PixelArt.R(g,night?"426b78":"7cb7b8",x-42,y,75,4);if(y%24==0)PixelArt.R(g,light,x-30,y,31,1);}foreach(int y in new[]{180,780}){int x=RiverX(y);PixelArt.R(g,"695a48",x-83,y-22,166,44);PixelArt.R(g,"c5a777",x-82,y-18,164,36);for(int xx=x-80;xx<x+83;xx+=7)PixelArt.R(g,"9b7e56",xx,y-18,1,36);PixelArt.R(g,"efcea0",x-85,y-24,170,3);PixelArt.R(g,"806344",x-85,y+20,170,4);for(int xx=x-81;xx<=x+81;xx+=27){PixelArt.R(g,"766046",xx,y-29,3,9);PixelArt.R(g,"766046",xx,y+17,3,11);}}for(int i=0;i<50;i++){int y=-450+i*28,x=RiverX(y)+(i%2==0?74:-77);PixelArt.R(g,"5e8156",x,y-9,2,10);PixelArt.R(g,"b3a476",x,y-12,2,5);}}
  if(f.S.Map=="lake"){for(int dy=-194;dy<=194;dy+=3){int dx=(int)(270*Math.Sqrt(Math.Max(0,1-dy*dy/(194.0*194))));PixelArt.R(g,night?"647761":"c5c492",-440-dx,70+dy,Math.Max(1,dx*2),3);}for(int dy=-185;dy<=185;dy+=3){int dx=(int)(260*Math.Sqrt(Math.Max(0,1-dy*dy/(185.0*185))));PixelArt.R(g,water,-440-dx,70+dy,Math.Max(1,dx*2),3);if((dy+185)%15==0)PixelArt.R(g,light,-430-dx/2,70+dy,Math.Max(1,dx/2),1);}using(var brush=new SolidBrush(PixelArt.C(grass)))g.FillEllipse(brush,-515,20,120,80);}
 }return b;}
 public static Bitmap Preview(string id){var b=new Bitmap(192,90);using(var g=Graphics.FromImage(b)){g.Clear(PixelArt.C("91aa70"));for(int i=0;i<28;i++)PixelArt.R(g,"a5bd81",i*71%192,i*31%90,4,1);if(id=="river"){for(int y=0;y<90;y+=2){int x=125+(int)(Math.Sin(y*.04)*13);PixelArt.R(g,"d5ca94",x-16,y,32,2);PixelArt.R(g,"76b8bc",x-12,y,24,2);}PixelArt.R(g,"c49c67",103,49,44,9);}if(id=="lake"){using(var brush=new SolidBrush(PixelArt.C("76b8bc")))g.FillEllipse(brush,12,13,87,60);PixelArt.R(g,"a2b477",49,38,18,10);}for(int i=0;i<4;i++){var s=g.Save();g.TranslateTransform(20+i*47,80);g.ScaleTransform(.35f,.35f);PixelArt.Pine(g,0,0);g.Restore(s);}PixelArt.R(g,"e6cca0",85,54,19,15);PixelArt.Poly(g,"a56c5d",81,54,94,42,107,54);}return b;}
}
}
