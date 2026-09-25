using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
namespace DesktopFarm {
public static class LandscapeArt {
 public static string Weather(Farm f){return f.S.Weather=="自动"?(DateTime.Today.DayOfYear%5==0?"雨天":"晴天"):f.S.Weather;}
 static void Fill(Graphics g,Color c,int x,int y,int w,int h){using(var b=new SolidBrush(c))g.FillRectangle(b,x,y,w,h);}
 public static Bitmap Meadow(Farm f,int w,int h){bool night=f.IsNight;var b=new Bitmap(w,h);var r=new Random(2837);using(var g=Graphics.FromImage(b)){g.Clear(PixelArt.C(night?"344d50":"8ba369"));
  PixelArt.R(g,night?"203b50":"c7dfd2",0,0,w,122);
  for(int x=0;x<w;x+=4){int y=36+(int)(Math.Sin(x*.012)*12+Math.Cos(x*.027)*7);PixelArt.R(g,night?"345567":"9bbab0",x,y,4,130-y);y=70+(int)(Math.Sin(x*.016+2)*14+Math.Cos(x*.009)*10);PixelArt.R(g,night?"3e655e":"789a83",x,y,4,140-y);y=116+(int)(Math.Sin(x*.007)*10+Math.Cos(x*.028)*4);PixelArt.R(g,night?"344d50":"8ba369",x,y,4,35);}
  for(int x=0;x<w;x+=19){int y=104+(int)(Math.Sin(x*.016)*7);PixelArt.Poly(g,night?"365952":"6f9077",x,y-22,x-8,y-8,x-4,y-8,x-11,y+2,x+11,y+2,x+4,y-8,x+8,y-8);}
  for(int i=0;i<30;i++){int cx=r.Next(w),cy=r.Next(155,Math.Max(156,h)),rx=25+r.Next(80),ry=9+r.Next(18);for(int yy=-ry;yy<ry;yy+=3){int len=(int)(rx*Math.Sqrt(Math.Max(0,1-yy*yy/(double)(ry*ry))));PixelArt.R(g,night?(i%2==0?"384f4f":"30494c"):(i%2==0?"90a86d":"839d65"),cx-len,cy+yy,len*2,3);}}
  for(int i=0;i<w*h/190;i++){int x=r.Next(w),y=r.Next(136,Math.Max(137,h));PixelArt.R(g,night?(i%2==0?"456052":"3a5650"):(i%2==0?"a5b77c":"76945f"),x,y,1+i%3,1);if(i%7==0){PixelArt.R(g,night?"425f50":"789856",x,y-2,1,3);PixelArt.R(g,night?"425f50":"789856",x+2,y-1,1,2);}}
  for(int i=0;i<34;i++){int x=i%2==0?r.Next(10,80):w-r.Next(10,80),y=r.Next(148,Math.Max(149,h-25));if(i%3==0){PixelArt.R(g,night?"53665d":"9ba58b",x,y,5,3);PixelArt.R(g,night?"65796c":"b8bf9c",x+1,y-1,3,2);}else{for(int j=0;j<3;j++)PixelArt.Flower(g,x+j*5,y+(j%2)*3,i+j);}}
 }return b;}
 public static void Back(Graphics g,Farm f,double t,int w,int h,IList<WorldObject> objects){if(f.IsNight){if(f.S.CameraY<-420){for(int i=0;i<35;i++){int x=(i*179+31)%w,y=8+(i*29)%64;int a=100+(int)(80*(.5+.5*Math.Sin(t*.8+i)));Fill(g,Color.FromArgb(a,223,234,199),x,y,1+i%2,1);}using(var br=new SolidBrush(PixelArt.C("e4dfb0")))g.FillEllipse(br,w*.72f,18,21,21);using(var br=new SolidBrush(PixelArt.C("203b50")))g.FillEllipse(br,w*.72f+7,15,20,20);}
  foreach(var o in objects.Where(o=>o.Kind=="cabin"||o.Kind=="mill"||o.Type=="lamp")){int radius=o.Type=="lamp"?26:38;for(int ring=3;ring>0;ring--){using(var br=new SolidBrush(Color.FromArgb(8,255,199,92)))g.FillEllipse(br,o.Foot.X-radius*ring/2,o.Foot.Y-radius*ring/4,radius*ring,radius*ring/2);}}
 }else if(f.S.CameraY<-420){for(int i=0;i<5;i++){int x=(int)((i*233+t*(1+i%2))%(w+100))-70,y=15+(i%3)*14;Fill(g,Color.FromArgb(135,239,243,220),x,y,62,5);Fill(g,Color.FromArgb(135,239,243,220),x+10,y-5,37,8);Fill(g,Color.FromArgb(135,239,243,220),x+20,y-9,17,7);}}
 }
 public static void Fore(Graphics g,Farm f,double t,int w,int h,IList<WorldObject> objects){foreach(var o in objects.Where(o=>o.Kind=="cabin")){for(int i=0;i<3;i++){double rise=(t*5+i*10)%32;int x=o.Foot.X+24+(int)(Math.Sin(t*.7+i)*3+rise*.22),y=o.Foot.Y-81-(int)rise;Fill(g,Color.FromArgb((int)(65*(1-rise/32)),218,217,195),x,y,4+(int)rise/9,3);}}
  string weather=Weather(f);if(weather=="雨天"){using(var tint=new SolidBrush(Color.FromArgb(18,37,65,87)))using(var rain=new SolidBrush(Color.FromArgb(125,177,213,219)))using(var splash=new SolidBrush(Color.FromArgb(92,207,232,226))){g.FillRectangle(tint,0,0,w,h);for(int i=0;i<64;i++){int x=(int)(i*83+t*31)%Math.Max(1,w),y=108+(int)(i*47+t*92)%Math.Max(1,h-108),len=4+i%4;g.FillRectangle(rain,x,y,1,len);if(i%5==0){g.FillRectangle(splash,x-1,y+len,4,1);if(y>h*.66)g.FillRectangle(splash,x-2,y+len+2,6,1);}}}}
  else if(weather=="雪天"){using(var snow=new SolidBrush(Color.FromArgb(205,239,242,225)))for(int i=0;i<48;i++){int x=(int)(i*89+t*6+Math.Sin(t*.8+i)*3)%Math.Max(1,w),y=110+(int)(i*37+t*12)%Math.Max(1,h-110);g.FillRectangle(snow,x,y,2,2);}}
  AmbientArt.Draw(g,f,t,objects);
 }
 public static void Path(Graphics g,int x,int y){PixelArt.R(g,"b5a57d",x-12,y-12,24,24);PixelArt.R(g,"c5b88c",x-11,y-10,22,21);PixelArt.R(g,"d2c49a",x-8,y-8,7,6);PixelArt.R(g,"d9cca3",x+2,y-6,7,5);PixelArt.R(g,"d1c297",x-4,y+2,8,6);PixelArt.R(g,"aa9a71",x-10,y+7,3,2);PixelArt.R(g,"87985f",x-12,y-10,2,4);PixelArt.R(g,"87985f",x+10,y+5,2,5);}
}
}
