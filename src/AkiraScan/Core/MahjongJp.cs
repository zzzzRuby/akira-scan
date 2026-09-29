using System;

namespace AkiraScan.Core;

public sealed class MahjongJpGame
{
    public required MahjongTile[] Tiles { get; set; }
    public required MahjongTile[] Dora { get; set; }
    public required MahjongTile[] UraDora { get; set; }
    public required MahjongTile WinningTile { get; set; }
    public required MahjongWind RoundWind { get; set; }
    public required MahjongWind SeatWind { get; set; }
    public required bool IsTsumo { get; set; }
    public required bool IsOpen { get; set; }
    public required bool IsRiichi { get; set; }
    public required bool IsDoubleRiichi { get; set; }
    public required bool IsIppatsu { get; set; }
    public required bool IsRinShan { get; set; }
    public required bool IsChankan { get; set; }
    public required bool IsLastTile { get; set; }
    public required bool IsTenhou { get; set; }
    public required bool IsChiihou { get; set; }
    public required int Aka { get; set; }
}

public sealed class MahjongJpGameResult
{
    public required uint TotalScore { get; set; }
    public required uint ScoreFromDealer { get; set; }
    public required uint ScoreFromNonDealer { get; set; }
    public required uint ScoreFromDiscarder { get; set; }
    public required MahjongJpYaku[] Yaku { get; set; }
    public required bool YakuMan { get; set; }
}

public static class MahjongJp
{
    public static MahjongJpGameResult? CalcJpGame(MahjongJpGame args)
    {
        throw new NotImplementedException();
    }
}
