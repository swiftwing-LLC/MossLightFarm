using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
namespace DesktopFarm {
public static class Scenery {
 public static List<WorldObject> Objects(Farm f,int width,int height){var result=new List<WorldObject>();
 Action<string,string,string,int,int,int> add=(id,kind,type,x,y,index)=>{LayoutPoint saved;bool moved=f.S.DesktopLayout.TryGetValue(id,out saved);if(moved){x=(int)Math.Round(saved.X*width);y=(int)Math.Round(saved.Y*height);}else if(WorldMap.Water(f.S.Map,x,y))return;result.Add(new WorldObject{Id=id,Kind=kind,Type=type,Index=index,Foot=new Point(x-f.S.CameraX,y-f.S.CameraY)});};
 var rng=new Random(7301);for(int i=0;i<90;i++){int x=rng.Next(-910,1870),y=rng.Next(-380,1030);if(x>-10&&x<820&&y>-20&&y<530)continue;add("scenery:tree:"+i,i%3==0?"tree":"pine",i%3==0?"tree":"pine",x,y,i);}
 if(f.S.Map=="lake"){add("scenery:island-tree","tree","tree",-459,47,-1);add("scenery:island-bench","decor","bench",-442,89,-1);for(int i=0;i<12;i++)add("scenery:lake-pine:"+i,"pine","pine",-760+i*55,300+i%3*18,-1);}
 // Fixed seed, stable IDs: scenery is editable and survives camera changes and restarts.
 int[] fx={70,735,865,700,130,405},fy={175,140,285,490,475,105};var flowers=new Random(1926);for(int i=0;i<45;i++){int x=i<6?fx[i]:flowers.Next(-880,1830),y=i<6?fy[i]:flowers.Next(-350,970);add("scenery:flowers:"+i,"decor","wildflowers",x,y,i);}
 return result;
 }
}
public static class AmbientArt {
 static void R(Graphics g,string c,int x,int y,int w,int h){PixelArt.R(g,c,x,y,w,h);}
 public static void Tree(Graphics g,int x,int y,int variant,bool night){if(variant%4==0){PixelArt.Tree(g,x,y,1,true);return;}PixelArt.Shadow(g,x,y+2,48);R(g,"665d47",x-4,y-58,8,60);R(g,night?"758479":"d5ccb0",x-2,y-57,4,56);for(int i=0;i<6;i++)R(g,"727667",x-2,y-7-i*8,3,2);PixelArt.Poly(g,"7c795e",x,y-31,x-18,y-52,x-14,y-54,x+2,y-37);var rng=new Random(variant+83);for(int i=0;i<16;i++){int xx=x+rng.Next(-25,22),yy=y-67+rng.Next(-10,27),size=rng.Next(10,20);string dark=night?"455f57":variant%2==0?"779467":"aa8991",mid=night?"577469":variant%2==0?"9bb474":"c8a4aa",light=night?"6b8271":variant%2==0?"bdcb8b":"e4c1bd";R(g,dark,xx-size/2,yy,size,12);R(g,mid,xx-size/2+2,yy-3,size-3,12);R(g,light,xx-size/2+3,yy-4,size/2,3);if(variant%2==0){R(g,mid,xx,yy+8,2,14);R(g,light,xx+4,yy+7,2,9);}}}
 public static void Flowers(Graphics g,int x,int y,int variant,bool night){R(g,night?"385a4d":"76925c",x-19,y+2,38,7);R(g,night?"426151":"829e66",x-23,y+3,46,4);var rng=new Random(variant*317+53);string[] petal={"e8adab","e9d3a0","aab9cf","d3adcc"},shade={"b47783","c3a569","818da8","a27caa"};for(int i=0;i<17;i++){int dx=rng.Next(-21,22),dy=rng.Next(-7,8),h=rng.Next(4,12),color=(variant+i/5)%4;R(g,night?"3a5d51":"658456",x+dx,y+dy-h,1,h+3);R(g,night?"536958":"88a664",x+dx-2,y+dy-3,3,1);R(g,night?"697775":shade[color],x+dx-2,y+dy-h,5,3);R(g,night?"91a09a":petal[color],x+dx-1,y+dy-h-2,3,6);R(g,night?"acaa81":"f5df95",x+dx,y+dy-h,1,2);}for(int i=0;i<4;i++){int xx=x-16+i*11;R(g,night?"4b675b":"718d5f",xx,y+9,7,2);}}
 static double Noise(int seed,int step){return new Random(unchecked(seed*73856093+step*19349663)).NextDouble();}
 public static PointF Flight(double t,int id,PointF home,float radius){double phase=t/18+id*.61;int n=(int)Math.Floor(phase);double a=phase-n;a=a*a*(3-2*a);double x=(Noise(id+61,n)*2-1)*(1-a)+(Noise(id+61,n+1)*2-1)*a,y=(Noise(id+177,n)*2-1)*(1-a)+(Noise(id+177,n+1)*2-1)*a;return new PointF(home.X+(float)(x*radius),home.Y+(float)(y*radius*.65));}
 public static PointF Screen(PointF world,Farm f){return new PointF(world.X-f.S.CameraX,world.Y-f.S.CameraY);}
 static bool Visible(int x,int y){return x>-50&&x<1010&&y>-50&&y<590;}
 public static void Butterfly(Graphics g,int x,int y,double t,int id,bool night){if(night){int a=80+(int)(140*Math.Max(0,Math.Sin(t*1.2+id)));using(var b=new SolidBrush(Color.FromArgb(a/6,226,227,148)))g.FillRectangle(b,x-3,y-3,8,8);using(var b=new SolidBrush(Color.FromArgb(a,238,230,162)))g.FillRectangle(b,x,y,2,2);return;}int wing=(int)(t*7+id)%3==0?1:3;string c=id%3==0?"c98b68":id%3==1?"9faacf":"d5a5bb";R(g,"79725c",x-wing-1,y-2,wing,5);R(g,c,x-wing,y-3,wing,5);R(g,c,x+2,y-3,wing,5);R(g,"f1dcb5",x-wing+1,y-2,1,2);R(g,"f1dcb5",x+2,y-2,1,2);R(g,"545e4d",x+1,y-2,1,5);}
 public static void Bee(Graphics g,int x,int y,double t){int flap=(int)(t*18)%2;R(g,"e4e7cc",x-2,y-3-flap,3,3);R(g,"c4d7ce",x+2,y-3+flap,3,2);R(g,"74644b",x-3,y,8,3);R(g,"edc266",x-2,y-1,6,4);R(g,"655641",x,y-1,1,4);R(g,"655641",x+3,y,2,2);R(g,"fff0b9",x-2,y-1,2,1);}
 public static void Bird(Graphics g,int x,int y,double t,bool goose,int direction,bool perched){var s=g.Save();g.TranslateTransform(x,y);g.ScaleTransform(direction,1);int wing=(int)(Math.Sin(t*(goose?6:9))*4);if(goose){R(g,"727b70",-9,1,15,4);R(g,"e8e5cf",-8,-1,14,4);R(g,"e8e5cf",5,-6,3,7);R(g,"f5ecd5",5,-8,6,3);R(g,"bc9367",11,-7,3,2);R(g,"494f46",9,-8,1,1);PixelArt.Poly(g,"c4cabc",-3,0,-11,-7+wing,-15,-8+wing,-7,3);PixelArt.Poly(g,"f3edd7",0,0,-3,-8-wing,1,-10-wing,4,1);}else{R(g,"616b62",-5,0,10,4);R(g,"8ba4ad",-4,-2,8,5);R(g,"d5c6a0",0,1,4,3);R(g,"aac1c3",3,-4,4,5);R(g,"454c42",5,-3,1,1);R(g,"d6aa6e",7,-2,2,1);if(!perched)PixelArt.Poly(g,"c1d0c7",-3,0,-10,-5+wing,-8,-7+wing,2,0);else R(g,"8a7050",-1,4,1,2);}g.Restore(s);}
 public static void Draw(Graphics g,Farm f,double t,IList<WorldObject> objects){var flowers=objects.Where(o=>o.Type=="wildflowers").ToArray();for(int i=0;i<flowers.Length;i++){var bed=flowers[i];PointF home=new PointF(bed.Foot.X+f.S.CameraX,bed.Foot.Y+f.S.CameraY);var world=Flight(t,i,home,70);var q=Screen(world,f);int x=(int)q.X,y=(int)q.Y-12+(int)(Math.Sin(t*2+i)*3);if(Visible(x,y))Butterfly(g,x,y,t,i,f.IsNight);if(!f.IsNight&&i%2==0){var bee=Screen(Flight(t*1.7,i+100,home,32),f);int bx=(int)bee.X,by=(int)bee.Y-6;if(Visible(bx,by))Bee(g,bx,by,t);}}
 if(f.IsNight)return;
 var trees=objects.Where(o=>o.Kind=="tree"||o.Kind=="pine").OrderBy(o=>o.Id).ToArray();for(int i=0;i<trees.Length;i+=7){var tree=trees[i];double cycle=(t+i*1.7)%32;bool perched=cycle<8;var home=new PointF(tree.Foot.X+f.S.CameraX,tree.Foot.Y+f.S.CameraY-48);var flight=Flight(t,i+501,home,150);double mix=perched?0:cycle<12?(cycle-8)/4:cycle>28?(32-cycle)/4:1;mix=mix*mix*(3-2*mix);PointF q=Screen(new PointF(home.X+(float)((flight.X-home.X)*mix),home.Y+(float)((flight.Y-home.Y)*mix)),f);if(Visible((int)q.X,(int)q.Y))Bird(g,(int)q.X,(int)q.Y,t,false,i%2==0?1:-1,perched);}
 // One flock crosses a fixed world route; camera movement never carries it along.
 double pass=t%110;if(pass<60){double leadX=-1000+pass*50,leadY=100+Math.Sin(t*.035)*130;for(int i=-3;i<=3;i++){int x=(int)leadX-Math.Abs(i)*23-f.S.CameraX,y=(int)leadY+i*16-f.S.CameraY;if(Visible(x,y))Bird(g,x,y,t+i*.15,true,1,false);}}
 }
}
}
