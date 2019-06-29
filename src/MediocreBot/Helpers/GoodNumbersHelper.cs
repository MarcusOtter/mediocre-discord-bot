using System;
using System.Linq;

namespace MediocreBot
{
    public static class GoodNumbersHelper
    {
        private static int[] _goodNumbers = new int[]
        {
            123, 1234, 12345, 1337, 69, 420, 111, 222, 333, 444, 555, 666, 777, 888, 999, 1111, 2222, 3333, 4444, 5555, 6666, 7777, 8888, 9999
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

        public static int LowestGoodNumber => _goodNumbers.OrderBy(x => x).FirstOrDefault();

        public static int GetRandomGoodNumber(int maxNumber = 0)
        {
            var availableNumbers = _goodNumbers.Where(x => x <= maxNumber).ToArray();
            if (availableNumbers.Length == 0) { return 0; }

            var random = new Random();
            return availableNumbers[random.Next(0, availableNumbers.Length)];
        }

        public static TimeSpan GetRandomGoodTime()
        {
            var random = new Random();
            return _goodTimes[random.Next(0, _goodTimes.Length)];
        }
    }
}
