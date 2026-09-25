using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace DesktopFarm {
public class LanguageRule {public string Pattern;public string Text;}
public class LanguagePack {public string Code,Name;public Dictionary<string,string> Text=new Dictionary<string,string>();public List<LanguageRule> Rules=new List<LanguageRule>();}
public static class L {
 public static string Code="zh-CN";public static bool IsEnglish {get{return Code=="en";}}
 static LanguagePack pack=new LanguagePack();static readonly Dictionary<string,string> cache=new Dictionary<string,string>();static List<KeyValuePair<string,string>> fragments=new List<KeyValuePair<string,string>>();
 public static string Error="";
 public static void SetLanguage(string code){Code=code=="en"?"en":"zh-CN";cache.Clear();Error="";pack=new LanguagePack();fragments.Clear();if(Code=="zh-CN")return;try{string file=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"assets","languages",Code+".json");pack=new JavaScriptSerializer().Deserialize<LanguagePack>(File.ReadAllText(file));fragments=pack.Text.Where(k=>k.Key.Length>0).OrderByDescending(k=>k.Key.Length).ToList();}catch(Exception e){Code="zh-CN";Error=e.Message;}}
 public static string T(string source){if(string.IsNullOrEmpty(source)||Code=="zh-CN")return source;string translated;if(cache.TryGetValue(source,out translated))return translated;translated=Translate(source,0);if(cache.Count>3000)cache.Clear();cache[source]=translated;return translated;}
 static string Translate(string source,int depth){string result;if(pack.Text.TryGetValue(source,out result))return result;if(!Regex.IsMatch(source,"[\u4e00-\u9fff]"))return source;if(depth<4){foreach(var rule in pack.Rules){var match=Regex.Match(source,rule.Pattern);if(!match.Success)continue;result=rule.Text;for(int i=1;i<match.Groups.Count;i++)result=result.Replace("{"+(i-1)+"}",Translate(match.Groups[i].Value,depth+1));return result;}}
  // Literal token replacement is a fallback for compact inventory rows, never the first choice for sentences.
  result=source;foreach(var pair in fragments)if(result.IndexOf(pair.Key,StringComparison.Ordinal)>=0)result=result.Replace(pair.Key,pair.Value);
  result=result.Replace("：",": ").Replace("。",".").Replace("，",", ").Replace("、",", ").Replace("「","\"").Replace("」","\"");return result;
 }
 public static int Count {get{return pack.Text.Count;}}
 public static string[] Untranslated(IEnumerable<string> sources){return sources.Where(s=>Regex.IsMatch(T(s),"[\u4e00-\u9fff]")).ToArray();}
}
}
