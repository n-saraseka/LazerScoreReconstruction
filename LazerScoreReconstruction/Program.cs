using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using LazerScoreReconstruction;
using LazerScoreReconstruction.Entities;
using osu.NET;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using osu.NET.Authorization;
using osu.NET.Enums;

var builder = new HostApplicationBuilder();


var currentDir = Directory.GetCurrentDirectory();

var clientId = builder.Configuration["ClientId"];
var clientSecret = builder.Configuration["ClientSecret"];
var scoresSample = builder.Configuration.GetValue<int>("ScoresSample");
var outputDirectory = builder.Configuration["OutputDirectory"];

var outputPath = Path.Combine(currentDir, outputDirectory);

if (!Directory.Exists(outputPath))
{
    Directory.CreateDirectory(outputPath);
}

var logFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
});

var logger = logFactory.CreateLogger<Program>();

var osuAccessTokenProvider = new OsuClientAccessTokenProvider(clientId, clientSecret);
var osuApiClient = new OsuApiClient(osuAccessTokenProvider, logFactory.CreateLogger<OsuApiClient>());

var scoresCount = 0;
var cursor = "";

var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    NewLine = Environment.NewLine,
    Delimiter = ","
};

var currentDateTime = DateTime.UtcNow;
var dateString =
    $"{currentDateTime.ToShortDateString().Replace("/", "")}-{currentDateTime.ToShortTimeString().Replace(":", "")}";

await using var writer = new StreamWriter(Path.Combine(outputPath, $"scores-{dateString}.csv"));
await using var csv = new CsvWriter(writer, csvConfig);

while (scoresCount < scoresSample)
{
    var searchResult = await osuApiClient.SearchBeatmapSetsAsync(sortType: SearchSortType.RankedDescending, cursor: cursor);
    await Task.Delay(TimeSpan.FromSeconds(1));
    cursor = searchResult.Value!.Cursor;
    var beatmapsets = searchResult.Value!.Sets;
    var beatmaps = beatmapsets.SelectMany(b => b.Beatmaps).ToList();

    for (var i = 0; i < beatmaps.Count && scoresCount < scoresSample; i++)
    {
        var scoresResult = await osuApiClient.GetBeatmapScoresAsync(beatmaps[i].Id);
        await Task.Delay(TimeSpan.FromSeconds(1));
        var scores = scoresResult.Value!.Where(s => s.IsPerfectCombo && s.LegacyTotalScore == 0).ToList();
        logger.LogInformation("Found {scoresCount} relevant scores", scores.Count);
        if (scores.Count == 0) continue;
        scoresCount += scores.Count;

        var scoresData = new List<ScoreData>();

        foreach (var score in scores)
        {
            var accuracy = Utils.GetAccuracyFromScore(score);
            var accPortion = Utils.GetAccuracyPortion(accuracy);
            var comboPortion = Utils.GetComboPortion(accPortion, score);
            var comboScore = Utils.GetComboScore(accuracy, comboPortion);
            scoresData.Add(new ScoreData
            {
                Id = score.Id,
                TotalScoreWithoutMods = score.TotalScoreWithoutMods,
                Accuracy = accuracy,
                Combo = score.MaxCombo,
                AccuracyPortion = accPortion,
                ComboPortion = comboPortion,
                ReconstructedComboScore = comboScore
            });
        }
        await csv.WriteRecordsAsync(scoresData);
    }
}





