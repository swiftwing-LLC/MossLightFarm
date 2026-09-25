using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace DesktopFarm {
// Routes only blank-desktop farm gestures. File icons and other applications retain input.
sealed class NativeMouse:IDisposable {
 delegate IntPtr Hook(int code,IntPtr message,IntPtr data);readonly Hook callback;IntPtr hook;readonly Form surface;readonly Func<Point,bool> hit;readonly Action<string,Point,int> send;bool held;long lastMove;
 [StructLayout(LayoutKind.Sequential)]struct MouseData {public Point Point;public uint Data,Flags,Time;public UIntPtr Extra;}
 [DllImport("user32.dll")]static extern IntPtr SetWindowsHookEx(int id,Hook callback,IntPtr module,uint thread);
 [DllImport("user32.dll")]static extern bool UnhookWindowsHookEx(IntPtr h);
 [DllImport("user32.dll")]static extern IntPtr CallNextHookEx(IntPtr h,int code,IntPtr msg,IntPtr data);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]static extern IntPtr GetModuleHandle(string name);
 [DllImport("user32.dll")]static extern IntPtr WindowFromPoint(Point p);
 [DllImport("user32.dll")]static extern bool ScreenToClient(IntPtr h,ref Point p);
 [DllImport("user32.dll")]static extern uint GetWindowThreadProcessId(IntPtr h,out uint pid);
 [DllImport("user32.dll")]static extern IntPtr SendMessageTimeout(IntPtr h,uint msg,IntPtr w,IntPtr l,uint flags,uint timeout,out IntPtr result);
 [DllImport("kernel32.dll")]static extern IntPtr OpenProcess(uint access,bool inherit,uint pid);
 [DllImport("kernel32.dll")]static extern IntPtr VirtualAllocEx(IntPtr process,IntPtr address,UIntPtr size,uint allocation,uint protection);
 [DllImport("kernel32.dll")]static extern bool WriteProcessMemory(IntPtr process,IntPtr address,byte[] bytes,int size,out IntPtr written);
 [DllImport("kernel32.dll")]static extern bool VirtualFreeEx(IntPtr process,IntPtr address,UIntPtr size,uint type);
 [DllImport("kernel32.dll")]static extern bool CloseHandle(IntPtr handle);
 public NativeMouse(Form form,Func<Point,bool> interactive,Action<string,Point,int> forward){surface=form;hit=interactive;send=forward;callback=OnMouse;hook=SetWindowsHookEx(14,callback,GetModuleHandle(null),0);if(hook==IntPtr.Zero)throw new InvalidOperationException("Desktop input unavailable: "+Marshal.GetLastWin32Error());}
 static bool IsIcon(Point p){IntPtr list=WindowFromPoint(p);if(DesktopHost.ClassName(list)!="SysListView32")return false;uint pid;GetWindowThreadProcessId(list,out pid);IntPtr process=OpenProcess(0x28,false,pid);if(process==IntPtr.Zero)return true;IntPtr memory=IntPtr.Zero;try{ScreenToClient(list,ref p);byte[] data=new byte[24];Array.Copy(BitConverter.GetBytes(p.X),0,data,0,4);Array.Copy(BitConverter.GetBytes(p.Y),0,data,4,4);memory=VirtualAllocEx(process,IntPtr.Zero,new UIntPtr(24),0x3000,4);IntPtr written,result;if(memory==IntPtr.Zero||!WriteProcessMemory(process,memory,data,data.Length,out written))return true;if(SendMessageTimeout(list,0x1012,IntPtr.Zero,memory,2,40,out result)==IntPtr.Zero)return true;return unchecked((int)result.ToInt64())>=0;}finally{if(memory!=IntPtr.Zero)VirtualFreeEx(process,memory,UIntPtr.Zero,0x8000);CloseHandle(process);}}
 IntPtr OnMouse(int code,IntPtr msg,IntPtr data){if(code<0)return CallNextHookEx(hook,code,msg,data);try{var m=(MouseData)Marshal.PtrToStructure(data,typeof(MouseData));int kind=msg.ToInt32();bool usable=DesktopHost.DesktopUnderPointer(m.Point,surface.Handle);
 if(kind==0x201&&usable&&(Control.ModifierKeys&(Keys.Shift|Keys.Control))==0&&hit(m.Point)&&!IsIcon(m.Point)){held=true;send("down",m.Point,0);return new IntPtr(1);}if(kind==0x202&&held){held=false;send("up",m.Point,0);return new IntPtr(1);}if(kind==0x200&&held){if(Environment.TickCount-lastMove>25){lastMove=Environment.TickCount;send("move",m.Point,0);}return CallNextHookEx(hook,code,msg,data);}if(kind==0x204&&usable&&hit(m.Point)&&!IsIcon(m.Point)){held=false;send("cancel",m.Point,0);return CallNextHookEx(hook,code,msg,data);}if(kind==0x20a&&usable&&hit(m.Point)&&!IsIcon(m.Point)){send("wheel",m.Point,(short)(m.Data>>16));return new IntPtr(1);}
 }catch(Exception e){held=false;RuntimeLog.Write("Desktop input: "+e.Message);}return CallNextHookEx(hook,code,msg,data);}
 public void Dispose(){if(hook!=IntPtr.Zero){UnhookWindowsHookEx(hook);hook=IntPtr.Zero;}held=false;}
}
}
