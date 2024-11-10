using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

const decimal FlagdownFee = 4.50m;

Banner("Singapore Taxi & Ride-Hailing Fare Estimator",
    "Flagdown fee + time-of-day surcharge on the metered fare");

Section("Rate card");
Row("Flagdown fee", Money(FlagdownFee), labelWidth: 38);
Row("Midnight       00:00-05:59, daily", "x1.50", labelWidth: 38);
Row("Morning peak   06:00-09:29, Mon-Fri*", "x1.25", labelWidth: 38);
Row("Evening peak   17:00-23:59, daily", "x1.25", labelWidth: 38);
Row("Weekend peak   10:00-13:59, Sat/Sun/PH", "x1.25", labelWidth: 38);
Row("All other times", "x1.00", labelWidth: 38);
Console.WriteLine("  * Excludes public holidays.");

do
{
    Section("Trip details");
    decimal meteredFare = ReadDecimal("Metered fare excluding flagdown, S$", 0m, 1000m);
    bool isWeekday = ReadChoice("Day of travel",
        "Weekday (Mon-Fri, not a public holiday)",
        "Saturday, Sunday or public holiday") == 1;
    TimeOnly boarding = ReadTime("Boarding time, 24h (e.g. 08:30, or just 8)");

    var (period, multiplier) = (isWeekday, boarding.Hour, boarding.Minute) switch
    {
        (_, >= 0 and <= 5, _) => ("Midnight surcharge", 1.50m),
        (true, >= 6 and <= 8, _) => ("Morning peak", 1.25m),
        (true, 9, < 30) => ("Morning peak", 1.25m),
        (false, >= 10 and <= 13, _) => ("Weekend/PH peak", 1.25m),
        (_, >= 17, _) => ("Evening peak", 1.25m),
        _ => ("Off-peak", 1.00m)
    };

    decimal surcharge = meteredFare * (multiplier - 1m);
    decimal total = FlagdownFee + meteredFare + surcharge;

    Section("Fare breakdown");
    Row("Boarding time", $"{boarding:HH:mm} {(isWeekday ? "weekday" : "weekend/PH")}");
    Row("Pricing period", period, multiplier > 1m ? ConsoleColor.Yellow : null);
    Row("Rate applied to meter", $"x{multiplier:0.00}");
    Console.WriteLine("  " + new string('-', 48));
    Row("Flagdown fee", Money(FlagdownFee));
    Row("Metered fare", Money(meteredFare));
    Row("Time-of-day surcharge", Money(surcharge));
    Console.WriteLine("  " + new string('=', 48));
    Row("ESTIMATED TOTAL", Money(total), ConsoleColor.Green);
    Console.WriteLine();
    Console.WriteLine("  Estimate only. Flagdown ($4.40-$5.20) and rates differ by operator. Booking fees, ERP,");
    Console.WriteLine("  city-area ($3) and airport ($6-$8) surcharges are not included.");
}
while (AskYesNo("Estimate another fare?"));

Console.WriteLine();
Console.WriteLine("  Thank you for using the fare estimator. Safe travels!");

static void Banner(string title, string subtitle)
{
    string line = new('=', Math.Max(title.Length, subtitle.Length) + 6);
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(line);
    Console.WriteLine($"   {title}");
    Console.WriteLine($"   {subtitle}");
    Console.WriteLine(line);
    Console.ResetColor();
}

static void Section(string title)
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"--- {title} ".PadRight(56, '-'));
    Console.ResetColor();
}

static void Row(string label, string value, ConsoleColor? color = null, int labelWidth = 28, int valueWidth = 20)
{
    Console.Write($"  {label.PadRight(labelWidth)}");
    if (color is { } c)
    {
        Console.ForegroundColor = c;
    }
    Console.WriteLine(value.PadLeft(valueWidth));
    Console.ResetColor();
}

static string Money(decimal value) => "$" + value.ToString("N2", CultureInfo.InvariantCulture);

static void ShowError(string message)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"    ! {message}");
    Console.ResetColor();
}

static string ReadLine(string prompt)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write($"  > {prompt}: ");
    Console.ResetColor();
    string? line = Console.ReadLine();
    if (line is null)
    {
        Console.WriteLine();
        Environment.Exit(0);
    }
    return line.Trim();
}

static decimal ReadDecimal(string label, decimal min, decimal max)
{
    while (true)
    {
        string input = ReadLine($"{label} [{min:N2} - {max:N2}]").TrimStart('$');
        if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value)
            && value >= min && value <= max)
        {
            return value;
        }
        ShowError($"Please enter a number between {min:N2} and {max:N2}.");
    }
}

static int ReadInt(string label, int min, int max)
{
    while (true)
    {
        if (int.TryParse(ReadLine(label), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
            && value >= min && value <= max)
        {
            return value;
        }
        ShowError($"Please enter a whole number between {min} and {max}.");
    }
}

static int ReadChoice(string label, params string[] options)
{
    Console.WriteLine($"  {label}:");
    for (int i = 0; i < options.Length; i++)
    {
        Console.WriteLine($"    {i + 1}) {options[i]}");
    }
    return ReadInt($"Select 1-{options.Length}", 1, options.Length);
}

static TimeOnly ReadTime(string label)
{
    while (true)
    {
        string input = ReadLine(label);
        if (TimeOnly.TryParseExact(input, ["H:mm", "HH:mm"], CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly time))
        {
            return time;
        }
        if (int.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out int hour) && hour is >= 0 and <= 23)
        {
            return new TimeOnly(hour, 0);
        }
        ShowError("Please enter a time such as 08:30, or an hour from 0 to 23.");
    }
}

static bool AskYesNo(string label)
{
    Console.WriteLine();
    while (true)
    {
        switch (ReadLine($"{label} (y/n)").ToLowerInvariant())
        {
            case "y" or "yes": return true;
            case "n" or "no": return false;
            default: ShowError("Please answer y or n."); break;
        }
    }
}
