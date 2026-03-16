using System.Text.RegularExpressions;

namespace CyberRegistration;

public class SecurityService
{
    private static readonly Regex SuspiciousPattern = new(
        @"\b(SELECT|INSERT|DELETE|DROP)\b|--|;",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly (Regex Pattern, string Label)[] AttackPatterns =
    [
        (new Regex(@"\b(DROP)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled), "SQL Injection - DROP (suppression de table)"),
        (new Regex(@"\b(DELETE)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled), "SQL Injection - DELETE (suppression de donnees)"),
        (new Regex(@"\b(SELECT)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled), "SQL Injection - SELECT (extraction de donnees)"),
        (new Regex(@"\b(INSERT)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled), "SQL Injection - INSERT (insertion malveillante)"),
        (new Regex(@"--", RegexOptions.Compiled), "SQL Injection - Commentaire SQL (--)"),
        (new Regex(@";", RegexOptions.Compiled), "SQL Injection - Terminaison de requete (;)"),
    ];

    public bool EstAttaque(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        return SuspiciousPattern.IsMatch(input);
    }

    public string DetecterTypeAttaque(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        foreach (var (pattern, label) in AttackPatterns)
        {
            if (pattern.IsMatch(input))
            {
                return label;
            }
        }

        return "Entree suspecte inconnue";
    }

    public bool MotDePasseFort(string pwd)
    {
        if (string.IsNullOrEmpty(pwd) || pwd.Length < 8)
        {
            return false;
        }

        var hasUppercase = pwd.Any(char.IsUpper);
        var hasDigit = pwd.Any(char.IsDigit);

        return hasUppercase && hasDigit;
    }
}