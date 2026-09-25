using System;
using System.IO;
using System.Security.Principal;
using Microsoft.Win32;
using System.Web.Script.Serialization;
namespace DesktopFarm {
public static class StartupManager {
 const string RunKey=@"Software\Microsoft\Windows\CurrentVersion\Run";
#if MAINTENANCE_BUILD
 const string ValueName="Mosslight Farm Maintenance";
#else
 const string ValueName="Mosslight Farm";
#endif

 public static string StartupCommand(string exe){return "\""+exe.Replace("\"","")+"\" --startup";}
 static DateTime checkedAt=DateTime.MinValue;static bool cached;
 static string TaskName {get{
#if MAINTENANCE_BUILD
  return "MosslightFarm-Maintenance-"+WindowsIdentity.GetCurrent().User.Value;
#else
  return "MosslightFarm-"+WindowsIdentity.GetCurrent().User.Value;
#endif
 }}
 static dynamic Scheduler(){dynamic service=Activator.CreateInstance(Type.GetTypeFromProgID("Schedule.Service"));service.Connect();return service;}
 public static bool Enabled {get{if((DateTime.UtcNow-checkedAt).TotalSeconds<20)return cached;checkedAt=DateTime.UtcNow;cached=false;try{dynamic task=Scheduler().GetFolder("\\").GetTask(TaskName);cached=task.Enabled && (string)task.Definition.Actions.Item(1).Path==System.Windows.Forms.Application.ExecutablePath;return cached;}catch{}try{using(var key=Registry.CurrentUser.OpenSubKey(RunKey)){cached=key!=null&&Convert.ToString(key.GetValue(ValueName))==StartupCommand(System.Windows.Forms.Application.ExecutablePath);}}catch{}return cached;}}
 public static void SetEnabled(bool enabled){checkedAt=DateTime.MinValue;bool scheduled=false;try{dynamic service=Scheduler(),root=service.GetFolder("\\");if(enabled){dynamic task=service.NewTask(0);string user=WindowsIdentity.GetCurrent().User.Value;task.RegistrationInfo.Description="Mosslight Farm: start the interactive desktop farm after sign-in.";task.Principal.UserId=user;task.Principal.LogonType=3;task.Principal.RunLevel=0;task.Settings.Enabled=true;task.Settings.StartWhenAvailable=true;task.Settings.DisallowStartIfOnBatteries=false;task.Settings.StopIfGoingOnBatteries=false;task.Settings.ExecutionTimeLimit="PT0S";task.Settings.MultipleInstances=2;task.Settings.RestartCount=3;task.Settings.RestartInterval="PT1M";dynamic trigger=task.Triggers.Create(9);trigger.UserId=user;trigger.Delay="PT15S";dynamic action=task.Actions.Create(0);action.Path=System.Windows.Forms.Application.ExecutablePath;action.Arguments="--startup";action.WorkingDirectory=AppDomain.CurrentDomain.BaseDirectory;root.RegisterTaskDefinition(TaskName,task,6,user,null,3,null);scheduled=true;}else{try{root.DeleteTask(TaskName,0);}catch{try{dynamic existing=root.GetTask(TaskName);existing.Enabled=false;}catch{}}}}catch(Exception e){RuntimeLog.Write("Startup task: "+e.Message);}
 using(var key=Registry.CurrentUser.CreateSubKey(RunKey)){if(enabled&&!scheduled)key.SetValue(ValueName,StartupCommand(System.Windows.Forms.Application.ExecutablePath));else key.DeleteValue(ValueName,false);}RuntimeLog.Write("Startup registration: "+(enabled?(scheduled?"scheduled logon + 15 seconds, retry enabled":"Run fallback"):"disabled"));}

}
}
