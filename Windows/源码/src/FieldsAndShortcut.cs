using System;
using System.Drawing;
using System.Windows.Forms;
namespace DesktopFarm {
public static class ShortcutKeys {
 public static bool Valid(int mods,int key){if(key==0)return mods==0;if(mods<0||mods>7)return false;bool function=key>=112&&key<=135&&key!=123;return function||(mods>0&&((key>=65&&key<=90)||(key>=48&&key<=57)||key==32));}
 public static string Label(int mods,int key){return key==0?L.T("未设置"):( (mods&2)!=0?"Ctrl + ":"")+((mods&1)!=0?"Alt + ":"")+((mods&4)!=0?"Shift + ":"")+((Keys)key).ToString();}
}
public partial class GameForm {
 bool recordingShortcut;int shortcutId=1;DateTime shortcutRetry;
 void EnsureShortcut(){if(recordingShortcut||Testing||hotkeyRegistered||F.S.HotkeyKey==0||DateTime.UtcNow<shortcutRetry)return;shortcutRetry=DateTime.UtcNow.AddSeconds(30);hotkeyRegistered=RegisterHotKey(Handle,shortcutId,(uint)F.S.HotkeyMods|0x4000,(uint)F.S.HotkeyKey);}
 public string ChangeShortcut(string input){var parts=(input??"").Split(',');int mods,key;if(parts.Length!=2||!int.TryParse(parts[0],out mods)||!int.TryParse(parts[1],out key)||!ShortcutKeys.Valid(mods,key))return L.T("请使用功能键，或 Ctrl / Alt / Shift 加字母、数字。");if(mods==F.S.HotkeyMods&&key==F.S.HotkeyKey&&hotkeyRegistered)return L.T("快捷键已保存。");int next=shortcutId==1?2:1;if(key!=0&&!Testing&&!RegisterHotKey(Handle,next,(uint)mods|0x4000,(uint)key))return L.T("这个快捷键已被占用，请换一个。原快捷键保持不变。");if(!Testing)UnregisterHotKey(Handle,shortcutId);shortcutId=next;F.S.HotkeyMods=mods;F.S.HotkeyKey=key;hotkeyRegistered=key!=0;SaveGame();return L.T("快捷键已保存。");}
 public void ChooseShortcut(){recordingShortcut=true;UnregisterHotKey(Handle,shortcutId);hotkeyRegistered=false;try{using(var dialog=new Form{Text=L.T("设置快捷键"),ClientSize=new Size(460,155),StartPosition=FormStartPosition.CenterParent,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false,KeyPreview=true}){var label=new Label{Text=L.T("直接按下想使用的快捷键，例如 F8 或 Alt + F。Esc 取消。"),Location=new Point(20,20),Size=new Size(420,80)};dialog.Controls.Add(label);var off=new Button{Text=L.T("取消快捷键"),Location=new Point(20,109),Size=new Size(150,30)};off.Click+=(s,e)=>{Toast(ChangeShortcut("0,0"));dialog.Close();};dialog.Controls.Add(off);dialog.KeyDown+=(s,e)=>{e.SuppressKeyPress=true;if(e.KeyCode==Keys.Escape){dialog.Close();return;}int mods=(e.Alt?1:0)|(e.Control?2:0)|(e.Shift?4:0);if(!ShortcutKeys.Valid(mods,(int)e.KeyCode))return;string msg=ChangeShortcut(mods+","+(int)e.KeyCode);label.Text=msg;if(F.S.HotkeyMods==mods&&F.S.HotkeyKey==(int)e.KeyCode)dialog.Close();};dialog.ShowDialog(this);}}finally{recordingShortcut=false;shortcutRetry=DateTime.MinValue;EnsureShortcut();}}
 void DrawFields(Graphics g){
  TextAt(g,"选好种子后播种空地；桌面壁纸上的田地仍会保留，也可以自由移动。",182,184,14,Muted,false,830);
  for(int i=0;i<Farm.Crops.Length;i++){
   var crop=Farm.Crops[i];int x=182+i%3*282,y=222+i/3*98;bool unlocked=F.Level>=crop.Level,chosen=selected==crop.Id;
   Color fill=chosen?PixelArt.C("e3ead4"):PixelArt.C("edead7");Fill(g,fill,x,y,267,84);Border(g,chosen?Green:Line,x,y,267,84,chosen?2:1);Fill(g,PixelArt.C("fff9e5"),x+1,y+1,265,2);
   PixelArt.Item(g,crop.Id,x+34,y+39,2);TextAt(g,crop.Name,x+69,y+13,18,Ink,true,158);TextAt(g,"种子 · "+crop.Cost+" G / 生长 "+Farm.TimeText(crop.Seconds),x+69,y+43,12,Muted,false,181);
   TextAt(g,chosen?"已选择":unlocked?"选择":"Lv."+crop.Level+" 解锁",x+205,y+31,11,chosen?Green:Muted,true,54);
   if(!unlocked)Fill(g,Color.FromArgb(82,244,240,222),x,y,267,84);
   HitArea(x,y,267,84,()=>{if(!unlocked){Toast("农场 Lv."+crop.Level+" 解锁。");return;}selected=crop.Id;F.S.SelectedSeed=selected;SaveGame();},crop.Name+" · "+crop.Cost+" G · "+Farm.TimeText(crop.Seconds));
  }
  Button(g,"收获成熟作物",182,527,196,40,()=>Act(F.HarvestAll,"harvest"));
  Button(g,"播种空地 · "+Farm.ItemName(selected),391,527,282,40,()=>Act(()=>F.PlantAll(selected)),true,!F.ProductionPaused);
  string expand=F.S.Land>=7?"土地已全部购入":F.Level<F.ExpandLevel?"购买土地 · Lv."+F.ExpandLevel+" 解锁":"购买土地 · "+F.ExpandCost+" G";
  Button(g,expand,686,527,328,40,()=>Act(F.Expand),false,F.S.Land<7&&F.Level>=F.ExpandLevel&&F.CanAfford(F.ExpandCost));
  TextAt(g,"收成自动放入仓库。扩建只增加壁纸农田，不会移动或遮住道路。",182,581,12,Muted,false,830);
 }
}
}
