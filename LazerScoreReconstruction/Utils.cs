using osu.NET.Enums;
using osu.NET.Models.Scores;

namespace LazerScoreReconstruction;

public static class Utils
{
    private const double ComboScorePortion = 500000;
    private const double AccScorePortion = 500000;
    private const double AccScoreAccExponent = 5;
    
    public static double GetAccuracyFromScore(Score score)
    {
        var statistics = score.Statistics;
        var maximumStatistics = score.MaximumStatistics;

        var scoreStatSum = GetStatSumFromStatistics(statistics, score.Ruleset);
        var scoreMaxStatSum = GetStatSumFromStatistics(maximumStatistics, score.Ruleset);
        
        return scoreStatSum / scoreMaxStatSum;
    }

    public static double GetAccuracyPortion(double accuracy) =>
        AccScorePortion * Math.Pow(accuracy, AccScoreAccExponent);
    
    public static double GetComboPortion(double accuracyPortion, Score score) {
        var totalScoreWithoutMods = score.TotalScoreWithoutMods;
        var statistics = score.Statistics;
        
        var bonusPortion = (statistics.SmallBonus ?? 0) * 10 + (statistics.LargeBonus ?? 0) * 50;
        
        return totalScoreWithoutMods - bonusPortion - accuracyPortion;
    }
    
    public static double GetComboScore(double accuracy, double comboPortion) => comboPortion / (accuracy * ComboScorePortion);

    private static double GetStatSumFromStatistics(ScoreStatistics statistics, Ruleset ruleset)
    {
        var statSum = 0.0;
        
        statSum += (statistics.Meh ?? 0) * 50;
        statSum += (statistics.Ok ?? 0) * (ruleset != Ruleset.Taiko ? 100 : 150);
        statSum += (statistics.Good ?? 0) * 200;
        statSum += (statistics.Great ?? 0) * 300;
        statSum += (statistics.Perfect ?? 0) * (ruleset != Ruleset.Mania ? 300 : 305);
        statSum += (statistics.SmallTickHit ?? 0) * (ruleset != Ruleset.Catch ? 10 : 300);
        statSum += (statistics.LargeTickHit ?? 0) * (ruleset != Ruleset.Catch ? 30 : 300);
        statSum += (statistics.SliderTailHit ?? 0) * 150;
        
        return statSum;
    }
}