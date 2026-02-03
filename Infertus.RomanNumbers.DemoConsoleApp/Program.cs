using Infertus.RomanNumbers;

var number = new Random().Next(100000);
Console.WriteLine($"Number to convert: {number}");

var convertedRoman = RomanNumbersConverter.ConvertToRoman(number);
Console.WriteLine($"Converted to roman: {convertedRoman}");

var convertedRomanSeparated = RomanNumbersConverter.ConvertToRoman(number, true);
Console.WriteLine($"Converted to roman (separated): {convertedRomanSeparated}");

var convertedArabic = RomanNumbersConverter.ConvertToArabicByMap(convertedRoman);
Console.WriteLine($"Converted back to arabic: {convertedArabic}");

var convertedArabicSeparated = RomanNumbersConverter.ConvertToArabicByMap(convertedRomanSeparated);
Console.WriteLine($"Converted back to arabic (separated): {convertedArabicSeparated}");
