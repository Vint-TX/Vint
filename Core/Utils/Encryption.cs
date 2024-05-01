using System.Security.Cryptography;
using System.Text;
using Vint.Core.ECS.Events.Entrance.Login;

namespace Vint.Core.Utils;

// todo what the fuck...

public class Encryption : IDisposable {
    const string Modulus =
        "rDYJNjOxCUPCHSrbmkvG57oyQyx8jrN9KLdfOsuvJtDIkkwDB91vohoGBSAdr6xRQa2urgbgIca5/s2nOrR9zK0CZQvZTsgRjrMiTf37EXbOiaZG6VJZHz+LmxCjRJ2pQoHl7sbonxNY7rLZ1wVSsW8PcIhCenR5y2A9alPXcI4zVmwdPP1g40kL9Nz/bb2SBL4O7O8nEW+2bIsdj/0QJFxqQAtD5cYmiqN34DIwC4w4n3P5+HveGjWcYxIy3lHOtdO5ForatexhNBma1gtW30tA4PW+EXdy6xmWdXKg6XDFCv3EnbrWXYoYeWciQSY9x7D5/ysh4/noWvWiQ3ArmNbmo9WyYcM3CHmiotNiGFC3ZwVToRURyobE0vKGp5CUo8AGBVbRBIjGiYQVa+OoajQhnEqidCexDJIlb2YAy61Ln8bNgpmEymiUUBztLSrHYgoQuY9ZRYU/ZFuQmIFedcMktneFrqVjiqX5LTEAD4AXx6NG3EWzy0XxUaeBcuvFjOlPFEKZaE21aTca3/3GlTxUlMZHY+7uToqALQwIU3qmZ8AINw/rbSy9Z1s7vOMUdz8a5QS3P3JanYwN9RDBat3bkBtx2CHEL3zot6xuXL3zMTVjNEjuKuiR56igWJD1Omrj3+YXuEKvV8WWFNdj2CoObqiyuE1RL9Jha9U+Foc=";

    const string Exponent = "AQAB";

    const string P =
        "41tz0J8jCVu+dkLjvNiB085f6QEE4oCkcf80W8Z+zpPk1SST5pvmlfyjqm/7B8Zi5OF+wn8kTOMZl9DcmjU+tlhQMyG2eoxA7ht5TRfHzL3mN953Yd6WLGnTLMaJrODUWeSoSpo8pLiQ7/zgo6Pa5WRCRGLkHYjVWx1AeIFCBVMI+cRftdh4qErOyT+jkQeeO+m6s+SBswBVb7nhTGeoOmdkY0mrP9aMQC0lM/te21LGWubgDzs9ZHESvcfUYDi90PKwfQW1eBo3yt3QGaoC8Dg1/ejxXuncBzeoLisA7KbGqNoktkOMqQmmQ73p3Zmlw5dwrDqDuPV9wSVq6bQq0w==";

    const string Q =
        "wegNiJp4yG741amAStAOnnKfRrYzdbuGsxaLa0S9u5EfPqOT+qZP1cbZYbH3JQcJYjw66/FhpDfJD7cGpoRw/xMgxUnhetmnr5l+SGiRiCFHqnz9TMJntvz9RSLT7uHElwYFcMIpbm9/9Iy/g/H1yxJx7a+BYchvMYROvcRJDtHXOXE/5/1loRj1MHt7dIU1iHX8mxuXBKk9Czc8l10vEIJ22x6TUVeXU2NKgFkJz4RalUXZv2KNX1H4eF6otgk9gjvdQzsc0aB3Z/hgjF7gFTmck60mlZ7qTca9dzUJjem6HtOXDGb5I4h39+ssxaqH0Ax3zrflFO1/YAk2sq6s/Q==";

    const string DP =
        "vUT+zygLpNjJX+4jZKzgt5DQa0q75Euvmm8YCWbfXd3k3EONFKeoeChPn62FT12qKlxGiGgLsi5EugvX+lBlGqu+aDKilLZ5vz8D5lfrrthP9SawP7trTSHz+Qx2xIKOhR9DsodgAMjdb8wWvmPD8L9cI04oFSY6Z5WcfDUu3vvXNwZxnxLMr11HdGYUJsIuyjWzhdqu1nNqrI4TruWEOMql2boUZBqZuztaA0I6H19zXW1iDYwFeb5gGblnZYsXCgFfR1UfUxKQa8PoX9UWiSBiYjQv7BHHaAK/cV+/b0MPkrW3ZjwptvcfgLlzHVWxl3hVBivYSHul6srZjCC+nQ==";

    const string DQ =
        "dgMS6YaJ6AKR8wecCuwhWZGNGm4dV5Pv4OLhq0FE7/jcuTS5BR1fDU5eUrULIz/rGBxsB9j6ggVpmuTbaVDFrJgpo6jZT/lTsu2KC+uIVv9aLIqxZpwSny3Nvtv5fDNvgDKr8PpaWNoHWACpNlZ2L0dkizH7XlWsRsitW5UwmZJgmJcyO7Dev3L/FqiwdDP2ZrzYJlZeTPnrKv5NJBoUYMnmKGmtYx4Z/Sg62W5XKDL3Jk5CvdXvEIpE3I1PxKpPAwoIGbjKS1KMGXzy1buQSZqsjrwc9YaoNvmzqe9fhz0uh0Bjd6rcCiIuUXaR4yNsUNSlvevxwhDhKsOwxILGyQ==";

    const string InverseQ =
        "nroBO5sN4hhcZNrqK/MAvNPHlyLLJ5xGe6s+Jb0bxQFBjf2oMax1IzKfo4c85l/u7c8Fa/MrFxsvRfeTy/9REe2J69Ul78T7br7L2QbRXDpbXjRQYzDNc//kdXGn+uHvp4CZydHLV2pgv8KDROehAic5vqHlgwTGu4mBppddQweemIxLbq4bzHPxr0mhOTb0m/wvgYvaJrkHtqwllV04ZIDYcoPYBm4C76GV70Fxx6ab5svrJOSkst0BBShhO0H49kYURVhL7blVQuU/+KrZUyq8eIV4kouzS48DvapN2AqkYB+MIA6yKkDFuTtzcGfcL68keEHeIW4T+2G6Y5rE6Q==";

    const string D =
        "klrGD2N09Ku6P/GZepMl8vAiIUaggOJ33u+gpiZOr81GeFbTSI7ffcNiFF8L+62VzRyfVNURDIOItMzwb4rPUvBCFcAxKawMh9sjr2bHRjtTIlbG8yoCBfWKXvmP2BOpspUU5Y30SqU1sn8bdYrXkWYSmA+ld86fZ8Fd0Ix6jvS7Gpp9asTn36HH4I0lI421mAQySRiiekYXoj3EnQdz/YLw5YbtaEyzmqV8jnP4swezY4g8C/qxclLXmrbvtlOOOqc6KLdPUscWgSG1nm06okChXc5bfxql9rnjTeNib6JgQ4wR2f7cidiAHkURLo8ieCCnBj4Q6mAMIamaAnhFA5Sy0O858PfRl1gjB1E6VwcwV+SJrdW2M2LgWIletUFeAjRuy1RKTk8HtUQIDwvhJ2aUI7WdFZccnXj/9ggEW/eYdg791l0uxbQVhnZpfcrQMesBOg1Fb9NTbbloXzyd7thXa+8FouBsXXiQpis3OTRZ710JOlw1hcvdxiFXyRxPOBj2n1QbvqMXBDiZtTUAWLTqzIC/fWP2LWGHslkEqXkkY8ESRCzqD693MO2Osdut/TjNXrn2flyXAk2xjxy7yv7vNA5uJkpJINdS0aAVA5JTmCvbeFUdapGlDhxXPUX4ym+z5piZCAarpafbWtrPgrmcxHYDd0yUSc6gnI4ffjk=";

    public Encryption() {
        Parameters = new RSAParameters {
            Modulus = Convert.FromBase64String(Modulus),
            Exponent = Convert.FromBase64String(Exponent),
            P = Convert.FromBase64String(P),
            Q = Convert.FromBase64String(Q),
            DP = Convert.FromBase64String(DP),
            DQ = Convert.FromBase64String(DQ),
            InverseQ = Convert.FromBase64String(InverseQ),
            D = Convert.FromBase64String(D)
        };

        Provider = new RSACryptoServiceProvider(4096);
        Sha256 = SHA256.Create();
        Passcode = Convert.FromBase64String(PersonalPasscodeEvent.Passcode);

        Provider.ImportParameters(Parameters);
    }

    RSAParameters Parameters { get; }

    byte[] Passcode { get; }
    RSACryptoServiceProvider Provider { get; }
    SHA256 Sha256 { get; }

    public string PublicKey => $"{Modulus}:{Exponent}";

    public void Dispose() {
        Provider.Dispose();
        Sha256.Dispose();

        GC.SuppressFinalize(this);
    }

    public byte[] GetLoginPasswordHash(byte[] passwordHash) {
        byte[] hashPasscodeXor = XorArrays(passwordHash, Passcode);
        byte[] concat = ConcatenateArrays(hashPasscodeXor, passwordHash);

        return Sha256.ComputeHash(concat);
    }

    public byte[] EncryptAutoLoginToken(byte[] token, byte[] passwordHash) => XorArrays(token, passwordHash);

    public byte[] DecryptAutoLoginToken(byte[] encryptedToken, byte[] passwordHash) =>
        XorArrays(encryptedToken, passwordHash);

    public byte[] RsaEncrypt(byte[] data) => RsaUtils.Encrypt(data, Parameters, 520);

    public byte[] RsaEncryptString(string password) => RsaEncrypt(Encoding.UTF8.GetBytes(password));

    public byte[] RsaDecrypt(byte[] encrypted) => RsaUtils.Decrypt(encrypted, Parameters, 520);

    public string RsaDecryptString(byte[] encrypted) => Encoding.UTF8.GetString(RsaDecrypt(encrypted));

    static byte[] XorArrays(byte[] a, byte[] b) {
        byte[] array = new byte[b.Length];

        for (int i = 0; i < b.Length; i++) {
            array[i] = (byte)(b[i] ^ a[i % a.Length]);
        }

        return array;
    }

    static byte[] ConcatenateArrays(byte[] a, byte[] b) {
        byte[] array = new byte[a.Length + b.Length];
        a.CopyTo(array, 0);
        b.CopyTo(array, a.Length);
        return array;
    }
}

public static class RsaUtils {
    public static byte[] Encrypt(byte[] data, RSAParameters parameters, int keySize) {
        using RSACryptoServiceProvider rsa = new(keySize);
        rsa.ImportParameters(parameters);

        return rsa.Encrypt(data, false);
    }

    public static byte[] Decrypt(byte[] encrypted, RSAParameters parameters, int keySize) {
        using RSACryptoServiceProvider rsa = new(keySize);
        rsa.ImportParameters(parameters);

        return rsa.Decrypt(encrypted, false);
    }
}
