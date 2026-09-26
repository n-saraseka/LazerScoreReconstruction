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

        var scoreStatSum = GetStatSumFromStatistics(statistics);
        var scoreMaxStatSum = GetStatSumFromStatistics(maximumStatistics);
        
        return scoreStatSum / scoreMaxStatSum;
    }

    public static double GetAccuracyPortion(double accuracy) =>
        AccScorePortion * Math.Pow(accuracy, AccScoreAccExponent);
    
    public static double GetComboPortion(double accuracyPortion, Score score) {
        var totalScoreWithoutMods = score.TotalScoreWithoutMods;
        var statistics = score.Statistics;
        
        var bonusPortion = statistics.SmallBonus ?? 0 * 10 + statistics.LargeBonus ?? 0 * 50;
        
        return totalScoreWithoutMods - bonusPortion - accuracyPortion;
    }
    
    public static double GetComboScore(double accuracy, double comboPortion) => comboPortion / (accuracy * ComboScorePortion);

    private static double GetStatSumFromStatistics(ScoreStatistics statistics)
    {
        var statSum = 0;
        
        statSum += statistics.Meh ?? 0 * 50;
        statSum += statistics.Ok ?? 0 * 100;
        statSum += statistics.Good ?? 0 * 200;
        statSum += statistics.Great ?? 0 * 300;
        statSum += statistics.Perfect ?? 0 * 300;
        statSum += statistics.SmallTickHit ?? 0 * 10;
        statSum += statistics.LargeTickHit ?? 0 * 30;
        statSum += statistics.SliderTailHit ?? 0 * 150;
        
        return statSum;
    }
}