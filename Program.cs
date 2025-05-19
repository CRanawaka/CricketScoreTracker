using System;
using System.ComponentModel.DataAnnotations;
using System.Formats.Asn1;

class Program
{

    const int MaxRuns = 300;
    const double MinOvers = 0.1;
    const double MaxOvers = 20;
    const int MaxNameLength = 30;

    private static readonly Random _rand = new Random();
    private const string MatchHistoryPath = "matches.txt";


    static void DisplayWelcomeMessage()
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("🏏 Sri Lanka Cricket Score Tracker");
        Console.WriteLine(@"  
   _____     _    _ _   _____       _      
  / ____|   | |  | | | |_   _|     | |     
 | |    _ __| |  | | |   | |  _ __ | | ___ 
 | |   | '__| |  | | |   | | | '_ \| |/ __|
 | |___| |  | |__| | |  _| |_| | | | | (__ 
  \_____|_|   \____/|_| |_____|_| |_|_|\___|");
        Console.ResetColor();

    }
    static int GetValidNumber(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int number))
                if (number >= min && number <= max)
                    return number;

            Console.WriteLine($"🚫 Invalid! Please enter a number between {min}-{max}");
        }
    }
    static double GetValidDouble(string prompt, double min, double max)
    {
        while (true)
        {
            Console.Write(prompt);
            if (double.TryParse(Console.ReadLine(), out double value))
                if (value >= min && value <= max)
                    return value;

            Console.WriteLine($"🚫 Must be between {min} and {max}");
        }
    }
    static string GetValidName(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine()?.Trim();

            // Validation checks
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("❌ Name cannot be empty!");
                continue;
            }

            if (input.Length > MaxNameLength)
            {
                Console.WriteLine("❌ Name too long (max 30 chars)");
                continue;
            }

            return input;
        }
    }
    static (int runs, double overs, string potm, string opponent, string result) GetMatchInputs()
    {
        string opponent = GetValidName("Opponent Team: ");
        int runs = GetValidNumber("Enter total runs: ", min: 0, max: MaxRuns);
        double overs = GetValidDouble($"Enter overs ({MinOvers}-{MaxOvers}): ", MinOvers, MaxOvers);
        string potm = GetValidName("Player of the match: ");
        string result = GetValidResult("Match result (W/L/D): ");

        return (runs, overs, potm, opponent, result);
    }

    static string GetValidResult(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine()?.Trim().ToUpper();
            if (input == "W" || input == "L" || input == "D")
                return input;
            Console.WriteLine("🚫 Enter W (Win), L (Lose) or D (Draw)");
        }
    }

    //Calculate
    static double CalculateRunRate(int runs, double overs)
    {
        if (overs == 0) return 0; // Prevent division by zero
        return runs / overs;
    }
    static void Main()
    {
        while (true)
        {
            Console.Clear();
            DisplayWelcomeMessage();

            Console.WriteLine("\n1. Track New Match");
            Console.WriteLine("2. View History");
            Console.WriteLine("3. View Statistics");
            Console.WriteLine("4. Exit");

            var choice = GetValidNumber("Choice: ", 1, 4);

            switch (choice)
            {
                case 1: TrackNewMatch(); break;
                case 2: DisplayHistory(LoadMatchHistory()); break;
                case 3: ShowStats(LoadMatchHistory()); break;
                case 4: return;
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

    }

    static void TrackNewMatch()
    {
        var (runs, overs, potm, opponent, result) = GetMatchInputs();
        double runRate = CalculateRunRate(runs, overs);
        DisplayMatchStats(runs, overs, potm, runRate, opponent, result);

        string[] predictions = {
        "Sri Lanka needs better bowling!",
        "This pitch favors batsmen!",
        "Match could go either way!"
        };
        Console.WriteLine(predictions[_rand.Next(predictions.Length)]);

        SaveToFile(runs, overs, potm, opponent, result);
    }

    static void DisplayMatchStats(int runs, double overs, string potm, double runRate, string opponent, string result)
    {
        //Display results
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n📊 MATCH SUMMARY");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\nMatch Stats: ");
        Console.WriteLine($"🏟️ SL vs {opponent}");
        Console.WriteLine($"Run Rate: {runRate:F2}");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Projected 20-over score: {(runRate * 20):F0}");

        //Fun analysis
        string analysis = runRate > 6 ? "💥 Aggressive batting!" : "🛡️ Defensive strategy";
        Console.WriteLine(analysis);

        Console.WriteLine($" {potm} is the hero today!");
    }
    static void SaveToFile(int runs, double overs, string potm, string opponent, string result)
    {
        string data = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}: SL vs {opponent} | Runs={runs}, Overs={overs}, Result={result}, PotM={potm}\n";

        try
        {
            File.AppendAllText(MatchHistoryPath, data);
            Console.WriteLine($"\n✅ Saved to {Path.GetFullPath(MatchHistoryPath)}");
            // Add to the success message
            Console.WriteLine($"📊 Total Matches Recorded: {File.ReadLines(MatchHistoryPath).Count()}");
        }
        catch (IOException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"💥 Failed to save: {ex.Message}");
            Console.ResetColor();

        }

    }

    static List<string> LoadMatchHistory()
    {
        if (!File.Exists(MatchHistoryPath))
        {
            Console.WriteLine("No matches found. Play some games first!");
            return new List<string>();
        }
        return File.ReadAllLines(MatchHistoryPath).ToList();
    }

    static void DisplayHistory(List<string> matches)
    {
        if (!matches.Any()) return;

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("\n📜 Match HISTORY of Last 5 games");
        Console.ResetColor();

        foreach (string match in matches.TakeLast(5))
        {
            Console.WriteLine($"- {match}");
        }
    }

    static void ShowStats(List<string> matches)
    {
        if (!matches.Any())
        {
            Console.WriteLine("📊 No matches to analyze");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n📊 MATCH STATISTICS");
        Console.ResetColor();

        // 1. Basic Match Stats (already working)
        try
        {
            var stats = matches
                .Select(m => m.Split('|').Last().Split(','))
                .Where(parts => parts.Length >= 2)
                .Select(parts => new
                {
                    Runs = int.Parse(parts[0].Split('=')[1].Trim()),
                    Overs = double.Parse(parts[1].Split('=')[1].Trim())
                });

            Console.WriteLine($"\n📈 Highest Score: {stats.Max(m => m.Runs)}");
            Console.WriteLine($"📉 Average Runs: {stats.Average(m => m.Runs):F0}");
            Console.WriteLine($"⚡ Best Run Rate: {stats.Max(m => m.Runs / m.Overs):F2}");
        }
        catch
        {
            Console.WriteLine("⚠️ Could not calculate basic match statistics");
        }

        // 2. Win/Loss Analysis - FIXED PARSING
        try
        {
            var results = matches
                .Select(m =>
                {
                    var parts = m.Split('|').Last().Split(',');
                    if (parts.Length < 4) return null;

                    var resultPart = parts[2].Trim(); // Changed from [3] to [2] for Result
                    if (!resultPart.StartsWith("Result=")) return null;

                    return resultPart.Split('=')[1].Trim();
                })
                .Where(r => r != null)
                .ToList();

            if (results.Any())
            {
                int wins = results.Count(r => r == "W");
                int losses = results.Count(r => r == "L");
                int draws = results.Count(r => r == "D");

                Console.WriteLine($"\n🏆 Win Rate: {wins * 100.0 / results.Count:F1}%");
                Console.WriteLine($"💔 Loss Rate: {losses * 100.0 / results.Count:F1}%");
                Console.WriteLine($"🤝 Draws: {draws}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Could not calculate win/loss statistics: {ex.Message}");
        }

        // 3. Recent Form - FIXED PARSING
        try
        {
            var recentResults = matches
                .TakeLast(5)
                .Select(m =>
                {
                    var parts = m.Split('|').Last().Split(',');
                    return parts.Length >= 3 ? parts[2].Split('=')[1].Trim() : null;
                })
                .Where(r => r != null)
                .ToList();

            if (recentResults.Any())
            {
                Console.WriteLine("\n🔥 Recent Form (Last 5): " +
                    string.Join(" → ", recentResults.Select(r =>
                        r == "W" ? "WIN" :
                        r == "L" ? "LOSS" : "DRAW")));
            }
        }
        catch
        {
            Console.WriteLine("⚠️ Could not determine recent form");
        }
    }

}