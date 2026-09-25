using System;
using System.Drawing;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DesktopFarm {
// Legacy native drawing diagnostics and shared Windows desktop helpers.
public static class DesktopHost {
 public delegate bool EnumProc(IntPtr h,IntPtr p);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern IntPtr FindWindow(string cls,string title);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern IntPtr FindWindowEx(IntPtr parent,IntPtr after,string cls,string title);
 [DllImport("user32.dll")]static extern bool EnumWindows(EnumProc callback,IntPtr arg);
 [DllImport("user32.dll",SetLastError=true)]static extern IntPtr SetParent(IntPtr child,IntPtr parent);
 [DllImport("user32.dll")]public static extern IntPtr GetParent(IntPtr h);
 [DllImport("user32.dll")]public static extern bool IsWindow(IntPtr h);
 [DllImport("user32.dll")]static extern bool IsWindowVisible(IntPtr h);
 [DllImport("user32.dll")]static extern IntPtr GetWindow(IntPtr h,uint command);
 [DllImport("user32.dll",EntryPoint="GetWindowLong")]static extern int GetStyle(IntPtr h,int index);
 [DllImport("user32.dll",EntryPoint="SetWindowLong")]static extern int SetStyle(IntPtr h,int index,int style);
 [DllImport("user32.dll")]static extern bool SetLayeredWindowAttributes(IntPtr h,uint key,byte alpha,uint flags);
 [DllImport("user32.dll")]static extern IntPtr SendMessageTimeout(IntPtr h,uint msg,IntPtr w,IntPtr l,uint flags,uint timeout,out IntPtr result);
 [DllImport("user32.dll")]static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int w,int ht,uint flags);
 [DllImport("user32.dll")]static extern bool ScreenToClient(IntPtr h,ref Point p);
 [DllImport("user32.dll")]public static extern short GetAsyncKeyState(int key);
 [DllImport("user32.dll")]static extern IntPtr WindowFromPoint(Point p);
 [DllImport("user32.dll")]static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern int GetClassName(IntPtr h,StringBuilder b,int n);
 [DllImport("user32.dll")]static extern IntPtr GetAncestor(IntPtr h,uint flags);
 public static string ClassName(IntPtr h){var b=new StringBuilder(100);GetClassName(h,b,b.Capacity);return b.ToString();}
 public static bool DesktopUnderPointer(Point p,IntPtr own){IntPtr w=WindowFromPoint(p);string c=ClassName(w);string root=ClassName(GetAncestor(w,2));return w==own||root=="Progman"||root=="WorkerW";}
 public static bool DesktopForeground {get{string c=ClassName(GetForegroundWindow());return c=="Progman"||c=="WorkerW"||c=="Shell_TrayWnd";}}
 public static bool Attach(Form form,Rectangle bounds,out string detail){
  IntPtr prog=FindWindow("Progman",null),icons=IntPtr.Zero,host=IntPtr.Zero;
  if(prog==IntPtr.Zero){detail="Windows desktop is not available.";return false;}
  IntPtr ignored;SendMessageTimeout(prog,0x052c,new IntPtr(13),new IntPtr(1),2,1000,out ignored);
  bool raised=(GetStyle(prog,-20)&0x00200000)!=0;
  EnumWindows(delegate(IntPtr h,IntPtr unused){IntPtr view=FindWindowEx(h,IntPtr.Zero,"SHELLDLL_DefView",null);if(view!=IntPtr.Zero){icons=view;host=FindWindowEx(IntPtr.Zero,h,"WorkerW",null);}return true;},IntPtr.Zero);
  if(raised){icons=FindWindowEx(prog,IntPtr.Zero,"SHELLDLL_DefView",null);host=prog;}
  if(host==IntPtr.Zero){detail="No wallpaper host was found. Use the preview while Explorer becomes available.";return false;}
  IntPtr hwnd=form.Handle;int style=GetStyle(hwnd,-16);SetStyle(hwnd,-16,(style&~unchecked((int)0x80000000))|0x40000000);
  SetStyle(hwnd,-20,GetStyle(hwnd,-20)|0x00080000|0x08000000|0x00000080);
  SetParent(hwnd,host);if(GetParent(hwnd)!=host){detail="Desktop parenting failed ("+Marshal.GetLastWin32Error()+").";return false;}
  SetStyle(hwnd,-20,GetStyle(hwnd,-20)&~0x00080000);SetStyle(hwnd,-20,GetStyle(hwnd,-20)|0x00080000);
  Point origin=bounds.Location;ScreenToClient(host,ref origin);
  SetWindowPos(hwnd,raised&&icons!=IntPtr.Zero?icons:IntPtr.Zero,origin.X,origin.Y,bounds.Width,bounds.Height,0x0010|0x0020|0x0040);
  detail="Attached: "+ClassName(host)+"; raised="+raised+"; icons="+(icons!=IntPtr.Zero)+"; "+bounds.Width+"x"+bounds.Height;
  return true;
 }
 [StructLayout(LayoutKind.Sequential,Pack=1)]struct Blend {public byte Operation,Flags,Alpha,Format;}
 [DllImport("user32.dll",SetLastError=true)]static extern bool UpdateLayeredWindow(IntPtr h,IntPtr screen,ref Point destination,ref Size size,IntPtr source,ref Point origin,uint key,ref Blend blend,uint flags);
 [DllImport("user32.dll")]static extern IntPtr GetDC(IntPtr h);
 [DllImport("user32.dll")]static extern int ReleaseDC(IntPtr h,IntPtr dc);
 [DllImport("gdi32.dll")]static extern IntPtr CreateCompatibleDC(IntPtr dc);
 [DllImport("gdi32.dll")]static extern IntPtr SelectObject(IntPtr dc,IntPtr item);
 [DllImport("gdi32.dll")]static extern bool DeleteObject(IntPtr item);
 [DllImport("gdi32.dll")]static extern bool DeleteDC(IntPtr dc);
 public static void Present(Form form,Bitmap bitmap,Rectangle screenBounds){IntPtr screen=GetDC(IntPtr.Zero),dc=CreateCompatibleDC(screen),handle=bitmap.GetHbitmap(Color.FromArgb(0)),old=SelectObject(dc,handle);try{var location=screenBounds.Location;var size=bitmap.Size;var origin=Point.Empty;var blend=new Blend{Alpha=255,Format=1};if(!UpdateLayeredWindow(form.Handle,screen,ref location,ref size,dc,ref origin,0,ref blend,2))throw new InvalidOperationException("Live wallpaper frame failed: "+Marshal.GetLastWin32Error());}finally{SelectObject(dc,old);DeleteObject(handle);DeleteDC(dc);ReleaseDC(IntPtr.Zero,screen);}}
 public static void Detach(Form form){if(!form.IsHandleCreated)return;SetParent(form.Handle,IntPtr.Zero);}
 // Diagnostic for the game's own surface; never captures desktop files or screen contents.
 public static string Verify(Form form){IntPtr h=form.Handle,parent=GetParent(h);string cls=ClassName(parent);if(cls!="Progman"&&cls!="WorkerW")throw new InvalidOperationException("Wallpaper parent is invalid: "+cls);if(!IsWindowVisible(h))throw new InvalidOperationException("Wallpaper surface is hidden");int ex=GetStyle(h,-20);if((ex&0x00080000)==0||(ex&0x08000000)==0)throw new InvalidOperationException("Wallpaper lost its layered/no-activate styles");if(cls=="Progman"){bool iconsAbove=false,wallpaperBelow=false;for(IntPtr p=GetWindow(h,3);p!=IntPtr.Zero;p=GetWindow(p,3))if(ClassName(p)=="SHELLDLL_DefView")iconsAbove=true;for(IntPtr p=GetWindow(h,2);p!=IntPtr.Zero;p=GetWindow(p,2))if(ClassName(p)=="WorkerW")wallpaperBelow=true;if(!iconsAbove||!wallpaperBelow)throw new InvalidOperationException("Wallpaper z-order is invalid: iconsAbove="+iconsAbove+", wallpaperBelow="+wallpaperBelow);}
 return "PASS: visible layered surface; parent="+cls+"; behind desktop icons; no activation; "+form.ClientSize.Width+"x"+form.ClientSize.Height;}
}
}
