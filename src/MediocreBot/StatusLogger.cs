using Discord;
using System;
using System.Threading.Tasks;

namespace MediocreBot
{
    public static class StatusLogger
    {
        public static Task Log(LogMessage message)
        {
            Log(message.Message);
            return Task.CompletedTask;
        }

        public static void Log(string message)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.ff")}]: {message}");
        }
    }
}
