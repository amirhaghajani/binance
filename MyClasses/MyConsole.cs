using System;
using System.IO;

public class MyConsole
{
    public static void WriteLine_Error(string message)
    {
        var currentColore = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(message);
        Console.ForegroundColor = currentColore;
        log("Error: "+message);
    }

    public static void WriteLine_Succ(string message)
    {
        var currentColore = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(message);
        Console.ForegroundColor = currentColore;
        log("Succ: " + message);
    }

    public static void WriteLine_Info(string message)
    {
        var currentColore = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine(message);
        Console.ForegroundColor = currentColore;
        log("Info: " + message);
    }

    internal static void WriteLine_Exception(string name, Exception ex)
    {
        var message ="\n\nerror on '"+ name +"' - " + ex.Message + (ex.InnerException!=null? " - "+ex.InnerException.Message :"")+"\n\n";
        WriteLine_Error(message);
        log("Exception: " + message);
    }

    private static void log(string message){
        string[] str = {DateTime.Now.ToString() , message.Replace("\n","")};
        File.AppendAllLines("00Log.txt",str);
    }
}