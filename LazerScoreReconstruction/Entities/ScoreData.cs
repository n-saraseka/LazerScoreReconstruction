using osu.NET.Enums;

namespace LazerScoreReconstruction.Entities;

public class ScoreData
{
    public long Id { get; set; }
    public Ruleset Ruleset { get; set; }
    public int TotalScoreWithoutMods { get; set; }
    public int Combo { get; set; }
    public int BeatmapMaxCombo { get; set; }
    public double Accuracy { get; set; }
    public double AccuracyPortion { get; set; }
    public double ComboPortion { get; set; }
    public double ReconstructedComboScore { get; set; }
}