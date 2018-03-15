using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Kurukuru;
using LastPass;

namespace LastPassAnywhere
{
    class Program
    {
        static void Main(string[] args)
        {
            PrintHeader();
            Vault mainVault = null;

            do
            {
                Console.WriteLine();
                Console.Write("Email: ");
                string email = Console.ReadLine();

                Console.Write("Password: ");
                string password = ReadPassword();

                Console.WriteLine();

                Console.Write("Multi-Factor (Optional): ");

                string multifactor = ReadPassword();

                if (string.IsNullOrWhiteSpace(multifactor))
                    multifactor = null;

                Console.WriteLine();

                Spinner.Start("Logging you in...", (spinner) =>
                {
                    spinner.SymbolSucceed = new SymbolDefinition("✔", "OK");

                    try
                    {
                        mainVault = Vault.Create(email, password, multifactor);
                        spinner.Succeed();
                    }
                    catch (LoginException e)
                    {
                        spinner.Fail(e.Message);
                    }
                    catch (FetchException)
                    {
                        spinner.Fail("Could not download your accounts");
                    }
                    catch (WebException)
                    {
                        spinner.Fail("Network error");
                    }
                });
            } while (mainVault == null);

            Console.WriteLine("Search accounts or quit:");

            string query = Console.ReadLine();
            while (query != "q" && query != "quit")
            {
                int i = 1;
                Account[] results = SearchAccounts(mainVault, query).ToArray();
                foreach (var account in results)
                {
                    Console.WriteLine("[{0}] {1}: {2}", i++, account.Name, account.Username);
                }

                Console.Write("Select account: ");
                string selectedAcc = Console.ReadLine();
                if (int.TryParse(selectedAcc, out int accIndex) && accIndex >= 1 && accIndex <= results.Length)
                {
                    string accPassword = results[accIndex - 1].Password;
                    Console.Write("Copy to clipboard or show to console? ");
                    bool copyToClipboard = Console.ReadLine().ToLower().StartsWith("c");
                    if (copyToClipboard && Clipboard.TryCopy(accPassword))
                        Console.WriteLine("Copied to clipboard.");
                    else
                    {
                        if (copyToClipboard)
                            Console.WriteLine("Clipboard not supported. Copy or input manually:");
                        WritePassword(accPassword);
                        Console.ReadLine();
                        Console.Clear();
                    }
                }

                Console.WriteLine("Search accounts or quit:");
                query = Console.ReadLine();
            }
        }

        static IEnumerable<Account> SearchAccounts(Vault v, string query)
        {
            return v.Accounts.Where(acc =>
                acc.Name.Contains(query) ||
                acc.Username.Contains(query) ||
                acc.Url.Contains(query));
        }

        static void WritePassword(string password)
        {
            // Letters: normal
            // Numbers: Red
            // Special Char: blue

            for (int i = 0; i < password.Length; i++)
            {
                char c = password[i];

                if (char.IsLetter(c))
                    Console.Write(c);
                else if (char.IsDigit(c))
                {
                    ConsoleColor prev = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(c);
                    Console.ForegroundColor = prev;
                }
                else
                {
                    ConsoleColor prev = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(c);
                    Console.ForegroundColor = prev;
                }
            }
        }

        static string ReadPassword()
        {
            ConsoleKeyInfo key;
            string pass = string.Empty;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                    pass += key.KeyChar;
                else if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    pass = pass.Substring(0, (pass.Length - 1));
                }
            }
            // Stops Receving Keys Once Enter is Pressed
            while (key.Key != ConsoleKey.Enter);

            return pass;
        }

        static void PrintHeader()
        {
            Console.Write(@"  _               _  ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(@" _____                              ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
            Console.Write(@" | |             | | ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(@"|  __ \                           _ ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
            Console.Write(@" | |     __ _ ___| |_");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(@"| |__) |_ _ ___ ___              | |");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
            Console.Write(@" | |    / _` / __| __");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(@"|  ___/ _` / __/ __|  _   _   _  | |");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
            Console.Write(@" | |___| (_| \__ \ |_");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(@"| |  | (_| \__ \__ \ (_) (_) (_) | |");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
            Console.Write(@" |______\__,_|___/\__");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(@"|_|   \__,_|___/___/             |_|");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();

        }
    }
}
