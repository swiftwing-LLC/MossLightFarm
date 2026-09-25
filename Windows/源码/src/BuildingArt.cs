using System;
using System.Drawing;
namespace DesktopFarm {
// Each workshop has its own silhouette and readable tools, even without colour.
public static class BuildingArt {
 public static void ForFarm(Graphics g,Farm f,int x,int y,string id,double t){Construction job;bool building=f.S.Construction.TryGetValue(id,out job);int level=f.Has(id)?f.S.Buildings[id]:0;if(level>0){if(id=="cabin")PixelArt.Cabin(g,x,y,f.IsNight,0);else if(id=="bakery")Bakery(g,x,y,f.IsNight);else if(id=="mill")PixelArt.Mill(g,x,y,t,f.IsNight);else Draw(g,x,y,id,f.IsNight);Tier(g,x,y,id,level,f.IsNight);}if(building)Scaffold(g,x,y,id,job,t,f.Now);}
 public static void Tier(Graphics g,int x,int y,string id,int level,bool night){if(level<2)return;var s=g.Save();g.TranslateTransform(x,y);int w=id=="cabin"||id=="bakery"?70:48;
 // Tier two: masonry foundations, a functional extension specific to the workshop.
 R(g,"746f5e",-w/2,0,w,5);R(g,"b6b49a",-w/2+1,0,w-2,2);for(int i=-w/2+5;i<w/2;i+=9)R(g,"ded5b4",i,1,1,3);
 if(id=="mill"){R(g,"a78556",-22,-7,12,8);R(g,"e2c69a",-21,-8,10,7);R(g,"b39464",14,-10,12,10);R(g,"efd5a2",15,-11,10,9);}
 else if(id=="loom"){R(g,"9b5b70",-26,-18,29,7);R(g,"e6bd9a",-26,-15,29,2);R(g,"634e41",-27,-19,2,17);}
 else if(id=="dairy"){R(g,"547a80",17,-12,10,13);R(g,"b8d0c2",18,-11,8,10);R(g,"e5e9cd",19,-10,2,8);R(g,"547a80",17,-14,10,3);}
 else if(id=="sugarhouse"){R(g,"627c73",9,-19,17,6);R(g,"a5bcb0",10,-18,15,3);R(g,"6d8d56",-30,-31,2,31);R(g,"b7c97d",-30,-27,2,2);}
 else if(id=="preserves"){R(g,"8b6750",12,-16,14,4);for(int i=0;i<2;i++){R(g,"bd7783",13+i*7,-24,5,8);R(g,"f1ddaa",12+i*7,-25,7,2);}}
 else if(id=="coop"){R(g,"856e4a",-28,-14,13,10);R(g,"dec392",-27,-13,11,7);R(g,"fff0cd",-25,-15,4,5);}
 else if(id=="barn"){R(g,"d3bc89",-28,-4,12,6);R(g,"9d895d",-24,-4,1,6);R(g,"d3bc89",18,-4,12,6);}
 else if(id=="station"){R(g,"856b4d",24,-33,3,36);R(g,"d5b384",19,-35,14,3);R(g,night?"ffe5a0":"e0d8af",20,-32,11,8);}
 else{R(g,"6d634c",-37,-25,4,27);R(g,"6d634c",32,-25,4,27);R(g,"c6a179",-39,-27,78,4);if(id=="cabin"){R(g,"789153",-32,-3,15,6);PixelArt.Flower(g,-29,-4,1);PixelArt.Flower(g,-22,-4,2);}}
 if(level>=3){
 // Tier three: roof loft/vent and a different, readable silhouette, not a recolour.
 int top=id=="cabin"?-68:id=="bakery"?-60:id=="mill"?-85:-47;
 if(id=="mill"){R(g,"6f6653",-19,-39,38,4);R(g,"c0aa7d",-21,-41,42,2);R(g,"8c7958",-19,-47,2,8);R(g,"8c7958",18,-47,2,8);R(g,"857459",25,-57,2,35);P(g,"bc815e",27,-57,39,-53,27,-49);}
 else{R(g,"695c48",-10,top,21,15);R(g,"dfcc9e",-8,top+2,17,11);P(g,"554e43",-14,top+1,0,top-11,14,top+1);P(g,id=="loom"?"a28da3":id=="dairy"?"84a1a5":"b48366",-11,top,0,top-8,11,top);Window(g,-4,top+4,night,8,7);}
 if(id=="bakery"){R(g,"b47e52",-37,-14,9,12);R(g,"e1b777",-36,-13,7,9);}if(id=="loom"){R(g,"7298a2",5,-8,7,10);R(g,"dec495",5,-6,7,1);}if(id=="dairy"){R(g,"678682",-31,-36,3,9);R(g,"a4bfb4",-31,-36,14,3);}
 }
 g.Restore(s);}
 static void Scaffold(Graphics g,int x,int y,string id,Construction job,double t,double now){var s=g.Save();g.TranslateTransform(x,y);int w=id=="cabin"||id=="bakery"?84:60,h=id=="mill"?86:id=="cabin"||id=="bakery"?65:50;
 if(job.FromLevel==0){PixelArt.Shadow(g,0,3,w);R(g,"998971",-w/2+4,-9,w-8,12);for(int i=0;i<4;i++)R(g,"c8b691",-w/2+5+i*(w-10)/4,-10,9,9);R(g,"b18d5d",-16,-19,30,5);R(g,"d7ba88",-19,-14,37,3);}
 foreach(int xx in new[]{-w/2,w/2-3}){R(g,"796047",xx,-h,3,h+7);R(g,"c6a477",xx,-h,1,h+7);}for(int yy=-h+10;yy<0;yy+=18){R(g,"95734d",-w/2,yy,w,3);R(g,"d2b583",-w/2,yy,w,1);}for(int i=0;i<5;i++)R(g,"e1c791",w/2-15,-34+i*7,10,2);R(g,"96724d",w/2-16,-37,2,40);R(g,"96724d",w/2-5,-37,2,40);
 R(g,"5e6550",-23,8,46,6);R(g,"d3cba5",-22,9,44,4);R(g,"8ba56d",-22,9,(int)(44*Math.Max(0,Math.Min(1,(now-job.Started)/(job.Ready-job.Started)))),4);
 int swing=(int)(t*3)%2;R(g,"deb977",-7,-h-9,14,7);R(g,"715b42",-4,-h-1,3,10);R(g,"8b8c79",-6+swing*3,-h-3,10,4);g.Restore(s);}
 static void R(Graphics g,string c,int x,int y,int w,int h){PixelArt.R(g,c,x,y,w,h);}
 static void P(Graphics g,string c,params int[] p){PixelArt.Poly(g,c,p);}
 static void Window(Graphics g,int x,int y,bool night,int w=10,int h=12){R(g,"574e42",x-1,y-1,w+2,h+2);R(g,night?"f7d78b":"83abb2",x,y,w,h);R(g,night?"fff0b9":"c2dad0",x+1,y+1,3,h-2);R(g,"a08561",x+w/2,y,1,h);R(g,"a08561",x,y+h/2,w,1);}
 static void Door(Graphics g,int x,int y,int w=12,int h=22){R(g,"564a40",x-1,y-1,w+2,h+1);R(g,"95704d",x,y,w,h);R(g,"c4a272",x+2,y+1,2,h-2);R(g,"e4cb83",x+w-3,y+h/2,1,2);}
 static void Base(Graphics g,int w,int h,string wall){PixelArt.Shadow(g,0,3,w+8);R(g,"655849",-w/2,-h,w,h+2);R(g,wall,-w/2+2,-h+2,w-4,h-2);R(g,"aa9874",-w/2,0,w,3);}
 static void Roof(Graphics g,int w,int y,int rise,string c){P(g,"514d44",-w/2,y,0,y-rise,w/2,y,w/2,y+5,-w/2,y+5);P(g,c,-w/2+3,y,0,y-rise+3,w/2-3,y);for(int i=4;i<rise;i+=5){int half=i*w/(rise*2);R(g,"c7c3a8",-half,y-rise+i,1,1);R(g,"b9b49a",-half+1,y-rise+i,half*2-2,1);}R(g,"dac39c",-w/2,y+3,w,2);}
 public static void Bakery(Graphics g,int x,int y,bool night){var s=g.Save();g.TranslateTransform(x,y);Base(g,72,39,"dcc89d");R(g,"66594a",23,-76,11,42);R(g,"bb8367",25,-74,7,38);for(int yy=-70;yy<-36;yy+=7)R(g,"e3b798",25,yy,7,1);R(g,"dfc2a0",21,-78,15,4);Roof(g,84,-39,26,"7b9494");Door(g,17,-24,12,24);Window(g,-29,-26,night,35,18);R(g,"806448",-32,-7,42,4);for(int i=0;i<3;i++){P(g,"b77b46",-27+i*11,-10,-27+i*11,-14,-24+i*11,-17,-20+i*11,-17,-17+i*11,-13,-17+i*11,-10);R(g,"edc681",-25+i*11,-15,6,4);R(g,"fff0b8",-23+i*11,-15,1,3);}R(g,"685b49",-35,-30,48,6);for(int i=0;i<8;i++)R(g,i%2==0?"f3dfb3":"ac6e59",-34+i*6,-29,6,7);R(g,"674e40",-9,-49,26,12);R(g,"e7cf9b",-8,-48,24,10);PixelArt.Item(g,"bread",4,-43,1);R(g,"9c7850",-39,-3,10,7);R(g,"d5b77f",-39,-4,10,2);g.Restore(s);}
 public static void Draw(Graphics g,int x,int y,string type,bool night){var s=g.Save();g.TranslateTransform(x,y);
 if(type=="barn"){
 Base(g,52,36,"b87560");P(g,"514b43",-31,-33,-20,-56,0,-65,20,-56,31,-33);P(g,"738989",-27,-35,-17,-53,0,-60,17,-53,27,-35);R(g,"b4c4b2",-20,-48,40,2);Door(g,-18,-27,36,27);for(int i=0;i<23;i++){R(g,"ead7af",-17+i,-25+i,2,2);R(g,"ead7af",16-i,-25+i,2,2);}R(g,"eddaba",-1,-26,2,27);Window(g,-5,-44,night,10,8);R(g,"685541",8,-36,17,12);R(g,"ead8ad",9,-35,15,10);R(g,"73949a",14,-34,5,2);R(g,"f4eed5",13,-32,7,6);R(g,"b0c7bb",13,-29,7,2);R(g,"cbbd8c",-30,0,12,6);for(int i=0;i<3;i++)R(g,"a4925f",-28+i*4,0,1,6);
 }else if(type=="coop"){
 PixelArt.Shadow(g,0,4,58);R(g,"6d5842",-20,-5,4,11);R(g,"6d5842",17,-5,4,11);R(g,"66523f",-24,-29,48,27);R(g,"d6b67d",-22,-27,44,23);Roof(g,56,-29,19,"8b965f");R(g,"5a4b3d",-9,-18,17,15);R(g,"423f34",-6,-16,11,13);P(g,"c2a477",-7,-3,7,-3,16,12,-1,12);for(int i=0;i<4;i++)R(g,"8e7552",-5+i*2,i*3,13,1);Window(g,13,-21,night,6,6);R(g,"9d7950",-29,-6,13,6);R(g,"ead7a0",-28,-6,11,2);R(g,"fff0ce",-25,-10,4,4);R(g,"fff0ce",-20,-9,3,3);
 }else if(type=="dairy"){
 Base(g,46,32,"e3ddbf");Roof(g,57,-32,22,"71949c");Door(g,-7,-23);Window(g,10,-22,night,8,12);R(g,"769593",-29,-28,13,29);R(g,"b5ccc1",-27,-27,9,27);R(g,"e1e7cd",-26,-26,3,24);R(g,"648481",-30,-29,15,3);R(g,"75918b",-28,-11,11,2);R(g,"799893",-17,-14,8,3);R(g,"d9d4b2",-9,-44,18,10);R(g,"6d8887",-3,-45,6,3);R(g,"fff1cd",-4,-42,8,7);R(g,"97b8b0",-4,-39,8,2);
 }else if(type=="loom"){
 Base(g,54,31,"c5ac8e");P(g,"534d45",-31,-28,-22,-48,8,-48,30,-28);P(g,"8e7d91",-27,-30,-20,-45,6,-45,26,-30);R(g,"b9a3ac",-22,-40,34,2);Door(g,13,-23);R(g,"5f5548",-24,-28,30,27);R(g,"8c7156",-22,-25,26,23);for(int i=0;i<10;i++)R(g,"e5d9b5",-20+i*2,-24,1,18);R(g,"b56f79",-20,-14,20,7);R(g,"ead3ab",-20,-10,20,2);R(g,"d0b184",-26,-24,34,3);R(g,"d0b184",-26,-2,34,3);R(g,"ad6679",-27,0,8,6);R(g,"cfb0b4",-17,0,8,6);R(g,"708e9a",-7,0,8,6);
 }else if(type=="sugarhouse"){
 Base(g,50,30,"c9ad7c");R(g,"685949",15,-65,10,39);R(g,"b47c5c",17,-63,6,35);for(int yy=-58;yy<-29;yy+=6)R(g,"deb693",17,yy,6,1);R(g,"dbc39c",13,-67,14,4);Roof(g,58,-30,15,"9c825d");Door(g,-6,-24);R(g,"826b49",-27,-16,14,16);R(g,"bb985f",-25,-15,10,14);R(g,"dac48b",-27,-17,14,3);R(g,"665d46",-27,-6,14,2);for(int i=0;i<3;i++){R(g,"6d8c51",-23+i*3,-37,2,20);R(g,"bfd180",-23+i*3,-32,2,2);}R(g,"e9dbac",10,-14,12,14);R(g,"a99a70",11,-14,10,2);R(g,"fff1c9",13,-10,6,6);
 }else if(type=="preserves"){
 Base(g,54,32,"e2c699");Roof(g,62,-32,24,"ab716d");Door(g,12,-24);Window(g,-23,-26,night,24,14);R(g,"72563f",-29,-12,33,4);for(int i=0;i<3;i++){R(g,"644c40",-26+i*10,-9,8,10);R(g,i==1?"dea457":"b76c79",-25+i*10,-8,6,7);R(g,"f2dcaa",-26+i*10,-10,8,3);R(g,"f3e4ba",-24+i*10,-5,4,3);}R(g,"9b7750",-26,2,26,4);R(g,"c3a170",-26,2,2,8);R(g,"c3a170",-2,2,2,8);R(g,"456d4e",21,-39,3,36);for(int i=0;i<5;i++){R(g,"7d9d5e",18+i%2*5,-37+i*7,7,4);R(g,"cb7585",22,-34+i*7,2,2);}
 }else if(type=="station"){
 Base(g,54,29,"dabd8b");Roof(g,62,-29,18,"77928a");Door(g,-7,-23);Window(g,-22,-22,night,10,12);Window(g,12,-22,night,10,12);R(g,"e8dfba",-5,-42,10,9);R(g,"505749",0,-41,1,5);R(g,"505749",0,-37,3,1);R(g,"655b4c",-29,8,58,2);R(g,"655b4c",-29,13,58,2);for(int i=0;i<8;i++)R(g,"ac956f",-27+i*7,7,2,9);
 }else{Base(g,48,30,"d5bc8d");Roof(g,58,-30,20,"8b967a");Door(g,-6,-22);}
 g.Restore(s);}
}
}
