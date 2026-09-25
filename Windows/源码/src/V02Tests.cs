using System;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;

namespace DesktopFarm {
public static class V02Tests {
 public static void Run(string path,Action<bool,string> check){
  var f=new Farm(()=>2000000000);f.S.Lighting="白天";
  var json=new JavaScriptSerializer();var old=json.Deserialize<Dictionary<string,object>>(json.Serialize(f.S));old.Remove("Language");old.Remove("WallpaperEnabled");old.Remove("SelectedSeed");var migrated=json.Deserialize<SaveData>(json.Serialize(old));Farm.Validate(migrated);check(migrated.Coins==180&&migrated.Plots.Count==12&&migrated.Language=="zh-CN","v0.1 save migrates without resetting progress");
  string stateBefore=json.Serialize(f.S);L.SetLanguage("en");check(L.Code=="en"&&L.Count>350,"English language pack loads");check(L.T("小麦")=="Wheat"&&L.T("设置")=="Settings","Core labels translate");
  check(L.T("种下小麦 · 2分 0秒后成熟")=="Wheat planted · Ready in 2m 0s","Dynamic crop message translates as a complete sentence");
  check(L.T("售出 3 份小麦 · 金币 +15")=="Sold 3 Wheat · +15 G","Dynamic sale message preserves quantities");
  string[] messages={f.Plant(-1,"wheat"),f.Water(11),f.Craft("cake"),f.GuideTitle,f.GuideHelp,f.TimeForTest(),"好感度  35  /  亲密","农场 Lv.5 解锁。","小麦 6/2  玉米 4/1","背景预览 · 直接点击互动 / 拖动小动物"};check(L.Untranslated(messages).Length==0,"Model errors, timers, tooltips and inventory rows translate");
  L.SetLanguage("zh-CN");check(L.T("小麦")=="小麦"&&json.Serialize(f.S)==stateBefore,"Switching language does not mutate the farm");
  var roaming=new Wanderer("cat",-1,new Point(480,270));var area=new Rectangle(22,37,916,477);var random=new Random(137);var quadrants=new HashSet<int>();double lastX=roaming.X,lastY=roaming.Y;bool inBounds=true,smooth=true;for(int i=0;i<12000;i++){roaming.Step(.1,area,random,new List<Rectangle>());inBounds&=roaming.X>=area.Left&&roaming.X<area.Right&&roaming.Y>=area.Top&&roaming.Y<area.Bottom;smooth&=Math.Sqrt(Math.Pow(roaming.X-lastX,2)+Math.Pow(roaming.Y-lastY,2))<2;lastX=roaming.X;lastY=roaming.Y;quadrants.Add((roaming.X>480?1:0)+(roaming.Y>270?2:0));}
  check(inBounds,"Free-roaming pets remain inside visible screen bounds");check(smooth,"Pet movement is continuous without teleporting");check(quadrants.Count==4,"Pets explore all four screen quadrants, not just the farm");
  roaming.MoveTo(new Point(-20,900),area);check(roaming.X==area.Left&&roaming.Y==area.Bottom-1,"Dragging clamps pets to the visible screen");
  var obstacle=new Rectangle(0,0,50,50);var escape=new Wanderer("cat",-1,new Point(25,25));escape.TargetX=100;escape.TargetY=25;for(int i=0;i<60;i++)escape.Step(.1,new Rectangle(0,0,150,100),new Random(1),new[]{obstacle});check(escape.X>=50,"Pet dropped on an obstacle can walk out again");
  foreach(var size in new[]{new Size(960,540),new Size(1280,720),new Size(640,960)}){using(var bitmap=PixelArt.Wallpaper(f,10,size.Width,size.Height,-1)){check(bitmap.Width==size.Width&&bitmap.Height==size.Height&&bitmap.GetPixel(0,size.Height-1).A==255&&bitmap.GetPixel(size.Width-1,0).ToArgb()!=Color.Magenta.ToArgb(),"Wallpaper fully covers "+size.Width+"x"+size.Height);}}
  f.S.Language="en";using(var form=new GameForm(f,Path.Combine(path,"unused-v02.json"),true)){foreach(string screen in new[]{"inventory","workshop","orders","build","pet","journal","settings","welcome"})using(var bitmap=form.RenderPanelSnapshot(screen,"")){bitmap.Save(Path.Combine(path,"en-"+screen+".png"));check(bitmap.Width==1200,"English layout renders: "+screen);}}
  f.S.Language="zh-CN";L.SetLanguage("zh-CN");using(var scene=PixelArt.Wallpaper(f,5,960,540,-1))scene.Save(Path.Combine(path,"wallpaper-meadow.png"));
  string sources=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"src");if(Directory.Exists(sources)){var literals=new List<string>();foreach(string file in Directory.GetFiles(sources,"*.cs")){if(Path.GetFileName(file)=="Localization.cs"||Path.GetFileName(file)=="V02Tests.cs")continue;foreach(Match match in Regex.Matches(File.ReadAllText(file),"\"(?:[^\"\\\\]|\\\\.)*\"")){string literal;try{literal=json.Deserialize<string>(match.Value);}catch{continue;}if(Regex.IsMatch(literal,"[\u4e00-\u9fff]")&&literal!="中文 / English")literals.Add(literal);}}
   L.SetLanguage("en");string[] missing=L.Untranslated(literals.Distinct());File.WriteAllLines(Path.Combine(path,"untranslated.txt"),missing);check(missing.Length==0,"All shipped Chinese UI literals have English coverage");L.SetLanguage("zh-CN");}
 }
 static string TimeForTest(this Farm f){return Farm.TimeText(3722);}
}
}
