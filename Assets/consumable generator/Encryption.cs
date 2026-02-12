using System.Text;

public class Encryption {
    private const string key = "L21vtCqjCVn0zWQmP1LgVDHsSi1VkZhXuEZqMpNv8oFTtw129YXF55tP8JkjkZSzUZCoUhiXbB0E14EJb0fmCvs4sMagRUFr2ntILQcMarBMsWq8mtwaOOJkJBk56HaS";

    public static string EncryptDecrypt(string data) {
        StringBuilder result = new();
        for (int i = 0; i < data.Length; i++) {
            result.Append((char)(data[i] ^ key[i % key.Length]));
        }
        return result.ToString();
    }
}
