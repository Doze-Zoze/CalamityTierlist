using Microsoft.AspNetCore.Components.Web.Virtualization;
using System;
using System.Linq;
using System.Net.Http.Json;

namespace CalamityTierList.Data;

public class TestData
{
    public string tester { get; set; } = string.Empty;
    public string weapon { get; set; } = string.Empty;
    public string? notes { get { return field == string.Empty ? null : field; } set; } = string.Empty;
    public string? tier { get { return field == string.Empty ? null : field; } set; } = string.Empty;

    public List<BossTestData> bosses { get; set; } = new();
}
public class BossTestData
{
    public string name { get; set; } = string.Empty;
    public string timeString
    {
        get
        {
            if (time == 0)
                return string.Empty;
            return $"{time / 60}:{(time % 60).ToString("00")}";
        }
        set
        {
            if (value.Contains(":"))
            {
                var s = value.Split(":");
                time = int.Parse(s[0]) * 60 + int.Parse(s[1]);
            }
            else
                time = int.Parse(value);
        }
    }
    public int time { get; set; }
    public string? note { get { return field == string.Empty ? null : field; } set; } = string.Empty;
    public string? gear { get { return field == string.Empty ? null : field; } set; } = string.Empty;
}

public class AggregateTestData
{
    public string ItemName = "";
    public List<BossTestData> BestTimes = new();
    public Dictionary<string, int> AverageTimes = new();
    public List<TestData> Tests = new();
}
public static class Data
{

    public static List<AggregateTestData> aggregateTests = new();
    public static List<TestData> LoadedData = new();
    public async static Task LoadTestData(HttpClient http)
    {
        if (LoadedData.Count > 0)
            return;

        LoadedData = await http.GetFromJsonAsync<List<TestData>>("2_1_2.json") ?? new();

        foreach (var item in LoadedData)
        {
            if (aggregateTests.Any(x => x.ItemName == item.weapon))
            {
                var ag = aggregateTests.First(x => x.ItemName == item.weapon);
                ag.Tests.Add(item);
            }
            else
            {
                aggregateTests.Add(new() { ItemName = item.weapon, Tests = new() { item } });
            }
        }

        foreach (var item in aggregateTests)
        {
            Dictionary<string, int> avgTime = new();
            Dictionary<string, int> avgCount = new();
            List<BossTestData> best = new();


            foreach (var item1 in item.Tests)
            {
                foreach (var item2 in item1.bosses)
                {
                    if (!best.Any(x => x.name == item2.name))
                        best.Add(item2);
                    else
                    {
                        var b = best.First(x => x.name == item2.name);
                        if (b.time > item2.time)
                            best[best.IndexOf(b)] = item2;
                    }

                    if (!avgTime.ContainsKey(item2.name))
                    {

                        avgTime[item2.name] = item2.time;
                        avgCount[item2.name] = 1;
                    }
                    else
                    {
                        avgTime[item2.name] += item2.time;
                        avgCount[item2.name]++;
                    }
                }
            }

            foreach (var kvp in avgTime)
            {
                item.AverageTimes[kvp.Key] = kvp.Value / avgCount[kvp.Key];
            }
            item.BestTimes = best;
        }
    }

    public static string FormatTime(int time)
    {
        if (time == 0)
            return string.Empty;
        return $"{time / 60}:{(time % 60).ToString("00")}";
    }
}


