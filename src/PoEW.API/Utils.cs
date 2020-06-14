using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;


namespace PoEW.API {
    static public class Utils {

        public static T DeepClone<T>(this T obj) {
            using (var ms = new MemoryStream()) {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }
        static public string GetCsrfToken(string htmlPageContent, string tokenName) {
            string needle = $"name=\"{tokenName}\" value=\"";

            int pos = htmlPageContent.IndexOf(needle);

            if (pos == -1) {
                return "";
            }

            int start = pos + needle.Length;
            int end = htmlPageContent.IndexOf("\"", start);

            if (end == -1) {
                return "";
            }

            return htmlPageContent.Substring(start, end - start);
        }

        static public string GetOnlineCode(string url) {
            return url.Substring(url.IndexOf("poe.trade") + 10);
        }

        static public string FindTextBetween(string htmlPageContent, string leftContent, string rightContent) {
            int first = htmlPageContent.IndexOf(leftContent);
            int last = htmlPageContent.IndexOf(rightContent, first);

            if (first == -1 || last == -1 || last < first) {
                return "";
            }

            return htmlPageContent.Substring(first + leftContent.Length, last - first - leftContent.Length);
        }
    }
}
