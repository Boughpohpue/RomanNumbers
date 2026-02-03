using System.Text.RegularExpressions;

namespace Infertus.RomanNumbers;

public static class RomanNumbersConverter
{
    /// <summary>
    /// Separates particular roman number parts (ex. 2378 -> MM CD LXX VIII)
    /// </summary>
    private const char _romanNumberSeparator = ' ';

    /// <summary>
    /// Vinculum char wraps wraps the number that should be multiplied by 1000 (ex. ^M^ = 1000x1000 = 1.000.000)
    /// </summary>
    private const char _vinculumChar = '^';

    /// <summary>
    /// Regex to obtain roman value wrapped with vinculum char
    /// </summary>
    private static readonly Regex _vinculumNumberRex = new Regex($"\\{_vinculumChar}[A-Z]+\\{_vinculumChar}");

    /// <summary>
    /// Roman numbers pattern map (1-10)
    /// </summary>
    private static readonly string[] _patternMap = { "E", "EE", "EEE", "EF", "F", "FE", "FEE", "FEEE", "EG", "G" };

    /// <summary>
    /// Roman number sets
    /// </summary>
    private static readonly string[][] _romansSets = new string[][]
    {
        new string[] { "I", "V", "X" },         // 0 - 1, 5, 10
        new string[] { "X", "L", "C" },         // 1 - 10, 50, 100
        new string[] { "C", "D", "M" },         // 2 - 100, 500, 1.000
        new string[] { "M", "^V^", "^X^" },     // 3 - 1.000, 5.000, 10.000
        new string[] { "^X^", "^L^", "^C^" },   // 4 - 10.000, 50.000, 100.000
        new string[] { "^C^", "^D^", "^M^" }    // 5 - 100.000, 500.000, 1.000.000
    };

    /// <summary>
    /// Converts arabic number to it's roman representation
    /// </summary>
    /// <param name="number">Arabic number to convert</param>
    /// <param name="separateParts">Determines whether to separate roman number parts</param>
    /// <returns>Roman number string</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static string ConvertToRoman(int number, bool separateParts = false)
    {
        if (number < 0 || number >= 1000000)
        {
            throw new ArgumentOutOfRangeException("Unable to convert provided number! It must be grater than 0 and lower than 1M.");
        }

        var retval = string.Empty;
        var numberParts = number.
            ToString().
            ToCharArray();

        for (var x = 0; x < numberParts.Length; x++)
        {
            retval += MapArabicNumberToRoman(
                int.Parse(numberParts[x].ToString()),
                GetRomansSet(numberParts.Length - 1 - x)) + _romanNumberSeparator;
        }

        return separateParts ?
            retval :
            retval
            .Replace(_romanNumberSeparator.ToString(), "")
            .Replace(new string(_vinculumChar, 2), "");
    }

    /// <summary>
    /// Converts roman number string to it's int value using map
    /// </summary>
    /// <param name="romanString">Roman number string</param>
    /// <returns>Integer value of roman number string</returns>
    public static int ConvertToArabicByMap(string romanString)
    {
        if (!romanString.Contains(_romanNumberSeparator))
            return ConvertToArabicBasic(romanString);

        var retval = 0;
        var romanParts = romanString.
            ToUpper().
            Split(new char[] { _romanNumberSeparator }, StringSplitOptions.RemoveEmptyEntries);

        for (var x = 0; x < romanParts.Length; x++)
            retval += MapRomanNumberToArabic(
                romanParts[x],
                romanParts.Length - 1 - x);

        return retval;
    }

    /// <summary>
    /// Converts roman number string to it's int value
    /// </summary>
    /// <param name="romanString">Roman number string</param>
    /// <returns>Integer value of roman number string</returns>
    public static int ConvertToArabicBasic(string romanString)
    {
        int retval = 0;

        int prevValue = 0;
        bool isVinculumStarted = false;
        for (var x = romanString.Length - 1; x >= 0; x--)
        {
            if (romanString[x] == _vinculumChar)
            {
                isVinculumStarted = !isVinculumStarted;
                continue;
            }

            var intValue = 0;
            try
            {
                // try to get int value from current char in roman string
                intValue = (int)Enum.Parse(typeof(RomanNumbersEnum), romanString[x].ToString(), true);
            }
            catch
            {
                // current char is not roman, skip it
                continue;
            }

            if (prevValue == 0)
            {
                retval += intValue;
                prevValue = intValue;
                continue;
            }

            if (isVinculumStarted)
            {
                intValue *= 1000;
            }

            retval += intValue >= prevValue ?
                intValue :
                intValue * -1;

            prevValue = intValue;
        }

        return retval;
    }

    /// <summary>
    /// Checks if provided string is a correct roman number
    /// </summary>
    /// <param name="s">Roman number string</param>
    /// <returns>True if provided string is correct roman number, otherwise false</returns>
    public static bool IsCorrectRomanNumber(string s)
    {
        var romanString = s.ToUpper();

        var prev = 0;
        var occu = 1;
        var prevGreater = false;
        for (var x = romanString.Length - 1; x >= 0; x--)
        {
            var intValue = (int)Enum.Parse(typeof(RomanNumbersEnum), romanString[x].ToString(), true);

            if (intValue > prev)
            {
                occu = 1;
                prevGreater = false;
            }
            else if (intValue < prev)
            {
                occu = 1;
                prevGreater = true;
            }
            else if (prev == intValue)
            {
                occu++;

                if (prevGreater)
                {
                    if (occu > 1)
                    {
                        return false;
                    }
                }
                else if (occu > 3)
                {
                    return false;
                }
            }

            prev = intValue;
        }

        return true;
    }

    private static string MapArabicNumberToRoman(int number, string[] romanSet)
    {
        return _patternMap[number - 1].
            Replace("E", romanSet[0]).
            Replace("F", romanSet[1]).
            Replace("G", romanSet[2]).
            Replace(new string(_vinculumChar, 2), "");
    }

    private static int MapRomanNumberToArabic(string romanNumber, int level)
    {
        var formattedRomanNumber = romanNumber.Contains(_vinculumChar.ToString()) ?
            FormatVinculumNumber(romanNumber) :
            romanNumber;

        var romanPattern = string.Empty;
        var currentPart = string.Empty;
        var vinculumStarted = false;
        for (var x = 0; x < formattedRomanNumber.Length; x++)
        {
            if (formattedRomanNumber[x] == _vinculumChar)
            {
                vinculumStarted = !vinculumStarted;
                continue;
            }

            currentPart = vinculumStarted ?
                $"{_vinculumChar}{formattedRomanNumber[x]}{_vinculumChar}" :
                formattedRomanNumber[x].ToString();

            switch (Array.IndexOf(GetRomansSet(level), currentPart))
            {
                case 0:
                    romanPattern += "E";
                    break;

                case 1:
                    romanPattern += "F";
                    break;

                case 2:
                    romanPattern += "G";
                    break;

                default:
                    break;
            }
        }

        return (Array.IndexOf(_patternMap, romanPattern) + 1) * (int)Math.Pow(10, level);
    }

    private static string[] GetRomansSet(int level)
    {
        if (level < 0)
        {
            throw new ArgumentOutOfRangeException("Level must be a zero or positive number!");
        }
        if (level >= _romansSets.Length)
        {
            throw new ArgumentOutOfRangeException("Roman set for provided level not available!");
        }

        return _romansSets[level];
    }

    /// <summary>
    /// Formats provided roman number with vinculum (ex. ^MM^ -> ^M^^M^)
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    private static string FormatVinculumNumber(string n)
    {
        var match = _vinculumNumberRex.Match(n);

        var newVinculumValue = string.Empty;
        foreach (var nvv in match.Value.Replace(_vinculumChar.ToString(), ""))
        {
            newVinculumValue += _vinculumChar.ToString() + nvv.ToString() + _vinculumChar.ToString();
        }

        return n.Replace(match.Value, newVinculumValue);
    }
}
