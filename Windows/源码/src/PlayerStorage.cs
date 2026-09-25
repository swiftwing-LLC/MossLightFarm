using System;
using System.IO;
namespace DesktopFarm {
public static class PlayerStorage {
 // Saved Games is outside packaged-app LocalAppData redirection. A login launch
 // and a launch from an editor must read the very same farm and renderer profile.
 public static string Root {get{
#if MAINTENANCE_BUILD
  return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),"Saved Games","MosslightFarm-Maintenance");
#else
  return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),"Saved Games","MosslightFarm");
#endif
 }}
 public static string SavePath {get{Directory.CreateDirectory(Root);string path=Path.Combine(Root,"save.json");if(!File.Exists(path)){string legacy=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"MosslightFarm","save.json");if(File.Exists(legacy)){File.Copy(legacy,path,false);if(File.Exists(legacy+".bak"))File.Copy(legacy+".bak",path+".bak",false);}}return path;}}
}
}
