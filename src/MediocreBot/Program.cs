using System.Threading.Tasks;

namespace MediocreBot
{
    public class Program
    {
        private static async Task Main()
        {
            await new MediocreBot().ConnectAsync();
            await Task.Delay(-1);
        }
    }
}
