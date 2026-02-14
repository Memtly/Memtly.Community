using System.Text;
using System.Text.RegularExpressions;

namespace WeddingShare.Helpers
{
    public class PasswordHelper
    {
        public static string GenerateGallerySecretKey()
        {
            return GenerateTempPassword(lower: true, upper: true, numbers: true, symbols: false, length: 30);
        }

        public static string GenerateSecretCode()
        {
            return EncodingHelper.Base64Encode(GenerateTempPassword(lower: true, upper: true, numbers: true, symbols: true, length: 20));
        }

        public static string GenerateTempPassword()
        {
            return GenerateTempPassword(lower: true, upper: true, numbers: true, symbols: true);
        }

        public static string GenerateTempPassword(bool lower, bool upper, bool numbers, bool symbols, int length = 12)
        {
            var rand = new Random();
            var characterSet = BuildCharacterSet(lower, upper, numbers, symbols);

            if (characterSet != null && characterSet.Length > 0)
            { 
                var passwordBuilder = new StringBuilder();
                for (var i = 0; i < length; i++)
                {
                    passwordBuilder.Append(PickRandomCharacter(rand, characterSet));
                }

                var password = passwordBuilder.ToString();

                if (lower && !HasLowerCaseLetter(password))
                {
                    password = ReplaceRandomCharacter(rand, password, PickRandomCharacter(rand, BuildCharacterSet(lower: true, upper: false, numbers: false, symbols: false)));
                }

                if (upper && !HasUpperCaseLetter(password))
                {
                    password = ReplaceRandomCharacter(rand, password, PickRandomCharacter(rand, BuildCharacterSet(lower: false, upper: true, numbers: false, symbols: false)));
                }

                if (numbers && !HasNumber(password))
                {
                    password = ReplaceRandomCharacter(rand, password, PickRandomCharacter(rand, BuildCharacterSet(lower: false, upper: false, numbers: true, symbols: false)));
                }

                if (symbols && !HasSymbol(password))
                {
                    password = ReplaceRandomCharacter(rand, password, PickRandomCharacter(rand, BuildCharacterSet(lower: false, upper: false, numbers: false, symbols: true)));
                }

                return password.ToString();
            }

            return string.Empty;
        }

        public static bool IsValid(string? password)
        {
            return !string.IsNullOrWhiteSpace(password?.Trim())
                && HasRequiredLength(password)
                && HasLowerCaseLetter(password)
                && HasUpperCaseLetter(password)
                && HasNumber(password)
                && HasSymbol(password);
        }

        public static bool IsWeak(string? password)
        {
            password = password?.Trim();

            if (!string.IsNullOrWhiteSpace(password))
            { 
                var weakPasswordsList = new List<string> {
                    "password1!",
                    "admin1!",
                };

                return weakPasswordsList.Exists(x => string.Equals(x, password, StringComparison.OrdinalIgnoreCase));
            }

            return true;
        }

        public static bool HasRequiredLength(string? password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 8 && password.Length <= 100;
        }

        public static bool HasLowerCaseLetter(string? password)
        {
            return !string.IsNullOrWhiteSpace(password) && Regex.IsMatch(password, @"^(?=.*?[a-z]+?)", RegexOptions.Compiled);
        }

        public static bool HasUpperCaseLetter(string? password)
        {
            return !string.IsNullOrWhiteSpace(password) && Regex.IsMatch(password, @"^(?=.*?[A-Z]+?)", RegexOptions.Compiled);
        }

        public static bool HasNumber(string? password)
        {
            return !string.IsNullOrWhiteSpace(password) && Regex.IsMatch(password, @"^(?=.*?[0-9]+?)", RegexOptions.Compiled);
        }

        public static bool HasSymbol(string? password)
        {
            return !string.IsNullOrWhiteSpace(password) && Regex.IsMatch(password, @"^(?=.*?[^a-zA-Z0-9]+?)", RegexOptions.Compiled);
        }

        private static string BuildCharacterSet(bool lower = true, bool upper = true, bool numbers = true, bool symbols = true)
        {
            var characterSetBuilder = new StringBuilder();

            if (lower)
            {
                characterSetBuilder.Append("abcdefghijklmnopqrstuvwxyz");
            }

            if (upper)
            {
                characterSetBuilder.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            }

            if (numbers)
            {
                characterSetBuilder.Append("0123456789");
            }

            if (symbols)
            {
                characterSetBuilder.Append("!@#$%^&*()-_=+[]{}|;:,.<>?");
            }

            return characterSetBuilder.ToString();
        }

        private static char PickRandomCharacter(Random rand, string characterSet)
        {
            return characterSet[rand.Next(characterSet.Length)];
        }

        private static string ReplaceRandomCharacter(Random rand, string baseString, char character)
        {
            var characterArr = baseString.ToCharArray();
            characterArr[rand.Next(baseString.Length)] = character;

            return new string(characterArr);
        }
    }
}