using System;

namespace MediocreBot.Helpers
{
    public static class GoodNumbersHelper
    {
        private static int[] _goodNumbers = new int[]
        {
            123, 1234, 1337, 69, 420, 111, 222, 333, 444, 555, 666, 777, 888, 999
        };

        private static TimeSpan[] _goodTimes = new TimeSpan[]
        {
            new TimeSpan(01, 23, 0),
            new TimeSpan(12, 34, 0),
            new TimeSpan(13, 37, 0),
            new TimeSpan(11, 11, 0),
            new TimeSpan(22, 22, 0),
            new TimeSpan(20, 48, 0)
        };

        public static int GetRandomGoodNumber()
        {
            var random = new Random();
            return _goodNumbers[random.Next(0, _goodNumbers.Length)];
        }

        public static TimeSpan GetRandomGoodTime()
        {
            var random = new Random();
            return _goodTimes[random.Next(0, _goodTimes.Length)];
        }
    }
}
