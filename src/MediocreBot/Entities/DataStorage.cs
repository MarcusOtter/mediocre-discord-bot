using System.IO;
using Newtonsoft.Json;

namespace MediocreBot.Entities
{
    public class DataStorage
    {
        // Todo: set bot configuration (for changing prefix, unless that should be separated from config)

        public BotConfiguration GetBotConfiguration()
        {
            if (!File.Exists("BotConfiguration.json"))
            {
                throw new FileNotFoundException("Your BotConfiguration.json file is missing. Create it and try again.");
            }

            var botConfigJson = File.ReadAllText("BotConfiguration.json");
            return JsonConvert.DeserializeObject<BotConfiguration>(botConfigJson);
        }
    }
}
