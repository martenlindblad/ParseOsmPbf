using OsmSharp.Streams;
using OsmSharp.Geo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Threading;

namespace ParseOsmPbf
{
    class Program
    {
        static double Radians(double deg)
        {
            return (deg * Math.PI / 180.0);
        }
        static double DistanceBetweenPlaces(double lon1, double lat1, double lon2, double lat2)
        {
            double R = 6371; // km

            double sLat1 = Math.Sin(Radians(lat1));
            double sLat2 = Math.Sin(Radians(lat2));
            double cLat1 = Math.Cos(Radians(lat1));
            double cLat2 = Math.Cos(Radians(lat2));
            double cLon = Math.Cos(Radians(lon1) - Radians(lon2));

            double cosD = sLat1 * sLat2 + cLat1 * cLat2 * cLon;

            double d = Math.Acos(cosD);

            double dist = R * d;

            return dist;
        }
        static bool YesNo(string[] checks, OsmSharp.Tags.TagsCollectionBase tags)
        {
            foreach (var tag in tags)
            {
                if (checks.Contains(tag.Key))
                {
                    return true;
                }
            }
            return false;
        }
        static string RemoveChars(string text)
        {
            char tab = '\u0009';
            return text.Replace("\"", " ").Replace(tab.ToString(), " ").Trim();
        }
        static void Main(string[] args)
        {

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            using (var fileStream = new FileInfo(args[0]).OpenRead())
            using (var outStream = new StreamWriter(args[1]))
            {
                var myInClause = new string[] { "shop" };
                var source = new PBFOsmStreamSource(fileStream);
                foreach (var element in source.Where(x => x.Tags.Count > 0 && x.Type == OsmSharp.OsmGeoType.Node && YesNo(myInClause, x.Tags)))
                {
                    List<string> outList = new List<string>();
                    var lonlat = JObject.FromObject(element);
                    outList.Add(element.Id.Value.ToString());
                    outList.Add(lonlat["Latitude"].ToString());
                    outList.Add(lonlat["Longitude"].ToString());
                    outList.Add(String.Join(" ", element.Tags.Where(x => myInClause.Contains(x.Key)).Select(x => x.Key)));
                    outList.Add(String.Join(" ", element.Tags.Where(x => myInClause.Contains(x.Key)).Select(x => x.Value)));
                    outList.Add(String.Join(" ", element.Tags.Where(x => x.Key == "amenity").Select(x => x.Value)));
                    outList.Add(String.Join(" ", element.Tags.Where(x => x.Key == "name").Select(x => x.Value)));
                    string outString = String.Join("\t", outList.Select(x => RemoveChars(x)));
                    outStream.WriteLine(outString);
                }
            }
        }
    }
}
