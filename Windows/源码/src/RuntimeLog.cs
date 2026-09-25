using System;
using System.IO;
namespace DesktopFarm {
static class RuntimeLog {
 public static void Write(string message){try{string dir=PlayerStorage.Root;Directory.CreateDirectory(dir);string file=Path.Combine(dir,"runtime.log");if(File.Exists(file)&&new FileInfo(file).Length>65536)File.WriteAllText(file,"");File.AppendAllText(file,DateTime.UtcNow.ToString("o")+" "+message+Environment.NewLine);}catch{}}
}
}
