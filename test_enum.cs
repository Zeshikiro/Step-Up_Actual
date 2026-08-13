using System;

class Program
{
    static void Main()
    {
        int diff = (7 + (DateTime.Today.DayOfWeek - DayOfWeek.Monday)) % 7;
        Console.WriteLine(diff);
    }
}
