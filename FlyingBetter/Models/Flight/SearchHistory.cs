using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using CsvHelper;
using System.Globalization;

namespace FlyingBetter.Models.Flight
{
    public class SearchHistory
    {
        public static List<SearchInfo> searchInfos;

        static SearchHistory()
        {
            readSearchHistory();
        }

        private static void readSearchHistory()
        {
            using (var searchHistoryReader = new StreamReader(Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "App_Data/searchHistory.csv")))
            using (var searchHistoryCsvReader = new CsvReader(searchHistoryReader, CultureInfo.InvariantCulture))
            {
                searchInfos = searchHistoryCsvReader.GetRecords<SearchInfo>().ToList();
            }
        }

        private static void writeSearchHistory()
        {
            using (var searchHistoryWriter = new StreamWriter(Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "App_Data/searchHistory.csv")))
            using (var searchHistoryCsvWriter = new CsvWriter(searchHistoryWriter, CultureInfo.InvariantCulture))
            {
                searchHistoryCsvWriter.WriteRecords(searchInfos);
            }
        }

        public void addSearchInfo(string from, string to)
        {
            searchInfos.Add(new SearchInfo() { from = from, to = to, timestamp = DateTime.Now });
            writeSearchHistory();
        }

        public List<string> getPopularDest()
        {
            readSearchHistory();
            return searchInfos
                    .GroupBy(si => si.to)
                    .Select(g => new { to = g.Key, count = g.Count() })
                    .OrderByDescending(e => e.count)
                    .Take(5)
                    .Select(e => e.to)
                    .ToList();
        }
    }

    public class SearchInfo
    {
        public string from { get; set; }
        public string to { get; set; }
        public DateTime timestamp { get; set; }
    }
}