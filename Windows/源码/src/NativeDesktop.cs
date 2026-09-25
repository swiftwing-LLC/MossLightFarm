using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
namespace DesktopFarm {
// Normal child-window rendering, rather than the old layered bitmap surface.
public sealed class NativeDesktop:Form {
 readonly WebView2 web=new WebView2();readonly string url,profile;readonly Func<double,double,bool> hit;readonly Action<string,double,double> gameInput;bool gameGesture;readonly Action<string> report;readonly int monitor;bool preview;Rectangle screenBounds;
 NativeMouse mouse;bool ready,disposed;DateTime lastReload=DateTime.MinValue,lastBind=DateTime.MinValue;RectangleF[] regions=new RectangleF[0];
 public NativeDesktop(string address,string data,int display,Func<double,double,bool> interactive,Action<string> status,Action<string,double,double> input,bool asPreview=false){url=address;profile=data;monitor=display;hit=interactive;gameInput=input;report=status;preview=asPreview;Text="Mosslight Independent Desktop";FormBorderStyle=asPreview?FormBorderStyle.Sizable:FormBorderStyle.None;ShowInTaskbar=asPreview;BackColor=Color.FromArgb(139,163,105);Bounds=Screen.AllScreens[Math.Max(0,Math.Min(display,Screen.AllScreens.Length-1))].Bounds;screenBounds=Bounds;if(asPreview)Bounds=new Rectangle(80,80,1280,800);web.Dock=DockStyle.Fill;Controls.Add(web);Shown+=async(s,e)=>await Initialize();}
 protected override bool ShowWithoutActivation {get{return !preview;}}
 protected override CreateParams CreateParams {get{var p=base.CreateParams;if(!preview)p.ExStyle|=0x08000000|0x80;return p;}}
 public bool Failed;
 public bool Attached {get{return !disposed&&!preview&&NativeShell.ParentAlive(this);}}
 public static bool RuntimeAvailable {get{try{return !string.IsNullOrEmpty(CoreWebView2Environment.GetAvailableBrowserVersionString());}catch{return false;}}}
 async Task Initialize(){lastReload=DateTime.UtcNow;try{var env=await CoreWebView2Environment.CreateAsync(null,profile);if(disposed)return;await web.EnsureCoreWebView2Async(env);if(disposed)return;web.CoreWebView2.Settings.AreDefaultContextMenusEnabled=false;web.CoreWebView2.Settings.IsZoomControlEnabled=false;web.CoreWebView2.Settings.AreDevToolsEnabled=false;web.CoreWebView2.NavigationStarting+=(s,e)=>{if(!e.Uri.StartsWith(url,StringComparison.Ordinal))e.Cancel=true;};web.CoreWebView2.NewWindowRequested+=(s,e)=>e.Handled=true;web.CoreWebView2.PermissionRequested+=(s,e)=>e.State=CoreWebView2PermissionState.Deny;
 web.CoreWebView2.WebMessageReceived+=(s,e)=>{try{var list=new JavaScriptSerializer().Deserialize<float[][]>(e.WebMessageAsJson);regions=list.Select(a=>new RectangleF(a[0],a[1],a[2],a[3])).ToArray();}catch{}};
 web.CoreWebView2.ProcessFailed+=(s,e)=>{ready=false;Failed=e.ProcessFailedKind.ToString()=="BrowserProcessExited";report("Renderer interrupted: "+e.ProcessFailedKind);};
 web.CoreWebView2.NavigationCompleted+=(s,e)=>{ready=e.IsSuccess;report(ready?"Independent desktop connected":"Desktop navigation failed: "+e.WebErrorStatus);};lastReload=DateTime.UtcNow;web.Source=new Uri(url);if(!preview)AttachNow();
 }catch(Exception e){Failed=true;report("Independent desktop: "+e.Message);RuntimeLog.Write(e.ToString());if(!preview)Hide();}}
 public void AttachNow(){lastBind=DateTime.UtcNow;preview=false;ShowInTaskbar=false;FormBorderStyle=FormBorderStyle.None;screenBounds=Screen.AllScreens[Math.Max(0,Math.Min(monitor,Screen.AllScreens.Length-1))].Bounds;Bounds=screenBounds;string detail;if(!NativeShell.Attach(this,screenBounds,out detail))throw new InvalidOperationException(detail);report(detail);if(mouse==null)mouse=new NativeMouse(this,Hit,Pointer);}
 bool Hit(Point p){PointF at=Normalize(p,screenBounds);double x=at.X,y=at.Y;return ready&&(regions.Any(r=>r.Contains((float)x,(float)y))||hit(x,y));}
 public static PointF Normalize(Point p,Rectangle bounds){return new PointF((float)((p.X-bounds.Left)/(double)Math.Max(1,bounds.Width)),(float)((p.Y-bounds.Top)/(double)Math.Max(1,bounds.Height)));}
 void Pointer(string phase,Point p,int delta){if(disposed||!ready)return;PointF at=Normalize(p,screenBounds);double x=at.X,y=at.Y;if(phase=="down")gameGesture=!regions.Any(r=>r.Contains(at));bool routeGame=gameGesture&&phase!="wheel";if(phase=="up"||phase=="cancel")gameGesture=false;
 try{BeginInvoke((Action)(async()=>{try{if(disposed||!ready)return;if(routeGame){gameInput(phase,x,y);if(phase=="down"||phase=="up"||phase=="cancel")await web.CoreWebView2.ExecuteScriptAsync("window.nativeGameHeld="+(phase=="down"?"true":"false")+";dragHeld=window.nativeGameHeld;");}else await web.CoreWebView2.ExecuteScriptAsync("window.nativePointer("+new JavaScriptSerializer().Serialize(new{phase=phase,x=x,y=y,delta=delta})+")");}catch(Exception e){RuntimeLog.Write("Farm gesture: "+e.Message);}}));}catch{}}
 public void Pulse(){if(disposed||preview||web.CoreWebView2==null)return;var bounds=Screen.AllScreens[Math.Max(0,Math.Min(monitor,Screen.AllScreens.Length-1))].Bounds;if((screenBounds!=bounds||!NativeShell.ParentAlive(this))&&(DateTime.UtcNow-lastBind).TotalSeconds>5){try{AttachNow();}catch(Exception e){report(e.Message);}}if(!ready&&(DateTime.UtcNow-lastReload).TotalSeconds>15)Reload();}
 public void RestoreDesktop(){if(disposed||preview||web.CoreWebView2==null)return;try{AttachNow();Reload();}catch(Exception e){Failed=true;report(e.Message);}}
 public void Reload(){if(disposed||web.CoreWebView2==null)return;lastReload=DateTime.UtcNow;try{web.CoreWebView2.Navigate(url);}catch(Exception e){report(e.Message);}}
 protected override void Dispose(bool disposing){if(disposing&&!disposed){disposed=true;if(mouse!=null)mouse.Dispose();web.Dispose();}base.Dispose(disposing);}
}
static class NativeShell {
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern IntPtr FindWindow(string cls,string text);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern IntPtr FindWindowEx(IntPtr parent,IntPtr after,string cls,string text);
 [DllImport("user32.dll")]static extern bool EnumWindows(DesktopHost.EnumProc callback,IntPtr arg);
 [DllImport("user32.dll")]static extern IntPtr SendMessageTimeout(IntPtr h,uint m,IntPtr w,IntPtr l,uint flags,uint ms,out IntPtr result);
 [DllImport("user32.dll",EntryPoint="GetWindowLong")]static extern int GetStyle(IntPtr h,int index);
 [DllImport("user32.dll",EntryPoint="SetWindowLong")]static extern int SetStyle(IntPtr h,int index,int value);
 [DllImport("user32.dll",SetLastError=true)]static extern IntPtr SetParent(IntPtr child,IntPtr parent);
 [DllImport("user32.dll")]static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int w,int height,uint flags);
 [DllImport("user32.dll")]static extern bool ScreenToClient(IntPtr h,ref Point p);
 public static bool ParentAlive(Form f){IntPtr p=DesktopHost.GetParent(f.Handle);return DesktopHost.IsWindow(p)&&(DesktopHost.ClassName(p)=="WorkerW"||DesktopHost.ClassName(p)=="Progman");}
 public static bool Attach(Form f,Rectangle bounds,out string detail){IntPtr prog=FindWindow("Progman",null),host=IntPtr.Zero,icons=IntPtr.Zero,ignored;if(prog==IntPtr.Zero){detail="Waiting for Windows desktop";return false;}SendMessageTimeout(prog,0x052c,new IntPtr(13),new IntPtr(1),2,1000,out ignored);bool raised=(GetStyle(prog,-20)&0x00200000)!=0;
 if(raised){host=prog;icons=FindWindowEx(prog,IntPtr.Zero,"SHELLDLL_DefView",null);}else EnumWindows((h,p)=>{if(FindWindowEx(h,IntPtr.Zero,"SHELLDLL_DefView",null)!=IntPtr.Zero)host=FindWindowEx(IntPtr.Zero,h,"WorkerW",null);return true;},IntPtr.Zero);
 if(host==IntPtr.Zero){detail="Waiting for wallpaper surface";return false;}SetStyle(f.Handle,-16,(GetStyle(f.Handle,-16)&~unchecked((int)0x80000000))|0x40000000);SetParent(f.Handle,host);Point at=bounds.Location;ScreenToClient(host,ref at);SetWindowPos(f.Handle,raised?icons:IntPtr.Zero,at.X,at.Y,bounds.Width,bounds.Height,0x0010|0x0020|0x0040);detail="Native desktop attached: "+DesktopHost.ClassName(host)+"; raised="+raised;return DesktopHost.GetParent(f.Handle)==host;}
}
}
