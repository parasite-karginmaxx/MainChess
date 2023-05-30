namespace MainChess.Model;

public enum Color
{
    White, Black
}

public static class Extensions
{
    public static bool IsWhite(this Color color) => color == Color.White;
    public static bool IsBlack(this Color color) => color == Color.Black;

}