using System;

class Program
{
    static void Main()
    {
        string lastDate = "invalid";
        if (DateTime.TryParseExact(lastDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime lastLoginDateObj))
        {
            Console.WriteLine("Parsed");
        }
        else
        {
            Console.WriteLine("Failed");
        }

        if (lastLoginDateObj < DateTime.Now)
        {
            Console.WriteLine("Works!");
        }
    }
}
