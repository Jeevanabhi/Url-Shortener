using System.Text;
public class Base62Encoder
{
    private const string Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public static string Encode(long id)
    {
        if (id == 0) return Chars[0].ToString();

        var sb = new StringBuilder();
        while (id > 0)
        {
            int remainder = (int)(id % 62);
            sb.Insert(0, Chars[remainder]);
            id /= 62;
        }
        return sb.ToString();
    }
}