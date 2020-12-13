using System;
using System.Threading;
public class MyThread
{
    public static void StartWithThread(System.Threading.ThreadStart s, int timeout_second, string message)
    {
        var performed = false;
        while (!performed)
        {
            var tr = new System.Threading.Thread(s);

            try
            {
                tr.Start();

                var co = 1;

                while (co < timeout_second + 1)
                {
                    System.Threading.Thread.Sleep(1000);

                    if (co > 1) Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.WriteLine($"{message} {co} second");


                    if (tr.ThreadState == ThreadState.Stopped)
                    {
                        performed = true;
                        break;
                    }
                    co++;
                }

                if (!performed)
                {
                    tr.Interrupt();
                    MyConsole.WriteLine_Error($"{message} not ended on {timeout_second} seconds - restarting on 10 seconds -------\n");
                    MyThread.GenerateConsoleWaiting(10);
                }

                if (performed)
                {
                    MyConsole.WriteLine_Succ(message + $" Ended in {co} seconds !!!");
                }
            }
            catch (Exception ex)
            {
                MyConsole.WriteLine_Exception("error in executing " + message , ex);
                throw new Exception(ex.Message);
            }

        }
    }

    internal static void GenerateConsoleWaiting(double timeout_Second)
    {
        if (timeout_Second == 0) timeout_Second = 30;
        var co = 1;

        while (co < timeout_Second + 1)
        {
            System.Threading.Thread.Sleep(1000);

            if (co > 1) Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.WriteLine($"waiting - {co} second");

            co++;
        }
    }
}