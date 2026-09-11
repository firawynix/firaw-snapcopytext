using System.Net;
using System.Text.RegularExpressions;

namespace Firaw.SnapCopyText.Services;

public sealed partial class SensitiveDataDetector
{
    public bool IsSensitive(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        if (EmailRegex().IsMatch(text) || ContainsValidCardNumber(text) || ContainsIpAddress(text))
        {
            return true;
        }

        string withoutLongNumberSequences = CardCandidateRegex().Replace(text, string.Empty);
        return BrazilianDocumentRegex().IsMatch(withoutLongNumberSequences) ||
               BrazilianPhoneRegex().IsMatch(withoutLongNumberSequences);
    }

    private static bool ContainsValidCardNumber(string text)
    {
        foreach (Match match in CardCandidateRegex().Matches(text))
        {
            string digits = NonDigitRegex().Replace(match.Value, string.Empty);
            if (digits.Length is >= 13 and <= 19 && PassesLuhn(digits))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsIpAddress(string text)
    {
        foreach (Match match in IpCandidateRegex().Matches(text))
        {
            if (IPAddress.TryParse(match.Value, out IPAddress? address) &&
                address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return true;
            }
        }

        return false;
    }

    private static bool PassesLuhn(string digits)
    {
        int sum = 0;
        bool doubleDigit = false;
        for (int index = digits.Length - 1; index >= 0; index--)
        {
            int value = digits[index] - '0';
            if (doubleDigit)
            {
                value *= 2;
                if (value > 9)
                {
                    value -= 9;
                }
            }

            sum += value;
            doubleDigit = !doubleDigit;
        }

        return sum % 10 == 0;
    }

    [GeneratedRegex(@"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"(?<!\d)(?:\d{3}[.\s-]?\d{3}[.\s-]?\d{3}[-\s]?\d{2}|\d{2}[.\s-]?\d{3}[.\s-]?\d{3}[/\s-]?\d{4}[-\s]?\d{2})(?!\d)")]
    private static partial Regex BrazilianDocumentRegex();

    [GeneratedRegex(@"(?<!\d)(?:\+?55[\s.-]?)?(?:\(?\d{2}\)?[\s.-]?)?9?\d{4}[\s.-]?\d{4}(?!\d)")]
    private static partial Regex BrazilianPhoneRegex();

    [GeneratedRegex(@"(?<!\d)(?:\d[ -]?){13,19}(?!\d)")]
    private static partial Regex CardCandidateRegex();

    [GeneratedRegex(@"(?<![\d.])(?:\d{1,3}\.){3}\d{1,3}(?![\d.])")]
    private static partial Regex IpCandidateRegex();

    [GeneratedRegex(@"\D")]
    private static partial Regex NonDigitRegex();
}
