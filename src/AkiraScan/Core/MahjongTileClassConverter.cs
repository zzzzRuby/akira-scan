namespace AkiraScan.Core;

public sealed class MahjongTileClassConverter : IClassConverter<MahjongTile>
{
    private MahjongTileClassConverter() { }

    public static MahjongTile? Convert(int klass)
    {
        return klass switch
        {
            0 => MahjongTile.Suo1,
            1 => MahjongTile.Suo2,
            2 => MahjongTile.Suo3,
            3 => MahjongTile.Suo4,
            4 => MahjongTile.Suo5,
            5 => MahjongTile.Suo6,
            6 => MahjongTile.Suo7,
            7 => MahjongTile.Suo8,
            8 => MahjongTile.Suo9,
            9 => MahjongTile.Wan1,
            10 => MahjongTile.Wan2,
            11 => MahjongTile.Wan3,
            12 => MahjongTile.Wan4,
            13 => MahjongTile.Wan5,
            14 => MahjongTile.Wan6,
            15 => MahjongTile.Wan7,
            16 => MahjongTile.Wan8,
            17 => MahjongTile.Wan9,
            18 => MahjongTile.Tong1,
            19 => MahjongTile.Tong2,
            20 => MahjongTile.Tong3,
            21 => MahjongTile.Tong4,
            22 => MahjongTile.Tong5,
            23 => MahjongTile.Tong6,
            24 => MahjongTile.Tong7,
            25 => MahjongTile.Tong8,
            26 => MahjongTile.Tong9,
            27 => MahjongTile.East,
            28 => MahjongTile.Green,
            29 => MahjongTile.North,
            30 => MahjongTile.Red,
            31 => MahjongTile.South,
            32 => MahjongTile.West,
            33 => MahjongTile.White,
            _ => null,
        };
    }
}
