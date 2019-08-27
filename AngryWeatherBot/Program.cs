using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using QuickType;
using System.Net.Http;
using Newtonsoft.Json;

namespace Telegram.Bot.Examples.Echo
{
    public static class Program
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("514772911:AAFsIoAI0BvtyQ3s-6JL4h-oS4EIYdCHpGM");

        public static void Main(string[] args)
        {
            var me = Bot.GetMeAsync().Result;
            Console.Title = me.Username;
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            Bot.StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            //message.Chat.Id

            if (message == null || message.Type != MessageType.Text) return;

            switch (message.Text.Split(' ').First())
            {

                case "/weather":

                    string url = "http://api.openweathermap.org/data/2.5/weather?q=Kyiv,ua&appid=228538c118dcbd5e9d7a79ed4b760927&units=metric";
                    string jsonString = await GetCallAPI(url);
                    var pwdResetRequest = PwdResetRequest.FromJson(jsonString);

                    string temperatureStr = Temperature(pwdResetRequest.Main.Temp);
                    string windStr = Wind(pwdResetRequest.Wind.Speed);

                    await Bot.SendTextMessageAsync(
                          message.Chat.Id,
                          //"\U0001F30D" + " Погода сейчас " + "\n" +
                          "\U0001f31e" + " Погода сейчас " + "\U0001f31a" + "\n" +
                          "\n" +
                          "\U0001f321" + " Температура: " + pwdResetRequest.Main.Temp.ToString() + " °C" + "\n" +
                          "   " + temperatureStr + "\n" +
                          "----------------------------------------------" + "\n" +
                          "\U0001f32a" + " Скорость ветра: " + pwdResetRequest.Wind.Speed.ToString() + " м/с \n" +
                          "   " + windStr + "\n" +
                          "----------------------------------------------" + "\n" +
                          "\U0001f329" + " Шо по тучам?: " + pwdResetRequest.Weather[0].Description
                           );
                    break;

                default:
                    const string usage = @"Usage: 
/weather - send weather";

                    await Bot.SendTextMessageAsync(
                        message.Chat.Id,
                        usage,
                        replyMarkup: new ReplyKeyboardRemove());
                    break;
            }
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message);
        }

        public static async Task<string> GetCallAPI(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {

                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response != null)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        return jsonString;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }

        public static string Temperature(double temp)
        {
            string temperatureStr;

            if (temp > 30)
                temperatureStr = "Ну и жара, я б в такую погоду дома под кондиционером сидел и пивко пил";
            else if (temp > 20)
                temperatureStr = "Погода как раз погулять! Надевай сандали с носками (но можно и без, если ты не глэк)";
            else if (temp > 10)
                temperatureStr = "Прохладно...Надевай кофту (не хипстерские свитшот и худи)";
            else if (temp > 0)
                temperatureStr = "Надеюсь ты уже купил теплую обувь??? Или как модник-бомж гоняешь в летних кроссовках?";
            else if (temp > -10)      //Пора раскатывать подкаты, мамин ты модник...
                temperatureStr = "Если ты без шапки, сразу звоню твоей маме.";
            else if (temp > -20)
                temperatureStr = "Подштанники это не зашквар)";
            else if (temp > -25)
                temperatureStr = "Не ну можно даже шарф надеть";
            else
                temperatureStr = "Думаю врятле стоит выходить на улицу, если ты не белый ходок";
            return temperatureStr;
        }

        public static string Wind(long wind)
        {
            if (wind == 0)
                return "Ветра нема";
            else if (wind == 1)
                return "Специалисты говорят что ветер тихий";
            else if (wind < 3)
                return "Специалисты говорят что ветер легкий";
            else if (wind < 5)
                return "Специалисты говорят что ветер слабый";
            else if (wind < 7)
                return "Специалисты говорят что ветер умеренный";
            else if (wind < 9)
                return "Специалисты говорят что ветер Свежий";
            else if (wind < 12)
                return "Специалисты говорят что ветер Сильный";
            else if (wind < 15)
                return "Специалисты говорят что ветер крепкий";
            else if (wind < 18)
                return "Специалисты говорят что ветер очень крепкий";
            else if (wind < 21)
                return "Специалисты говорят что сейчас шторм";
            else if (wind < 25)
                return "Специалисты говорят что сейчас сильный шторм";
            else if (wind < 29)
                return "Специалисты говорят что сейчас жестокий шторм";
            else if (wind > 29)
                return "Специалисты говорят что сейчас ураган";
            else
                return "Ветер эрор";
        }
    }
}