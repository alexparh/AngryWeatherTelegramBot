using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using AngryWeatherBot.QuickType;
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
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnInlineQuery += BotOnInlineQueryReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            Bot.StartReceiving(Array.Empty<UpdateType>());
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();
            Bot.StopReceiving();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message == null || message.Type != MessageType.Text) return;

            switch (message.Text.Split(' ').First())
            {
                // send inline keyboard
                case "/inline":
                    await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                    await Task.Delay(500); // simulate longer running task

                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new [] // first row
                        {
                            InlineKeyboardButton.WithCallbackData("Auto White Widow"),
                            InlineKeyboardButton.WithCallbackData("Crtical Kush"),
                        },
                        new [] // second row
                        {
                            InlineKeyboardButton.WithCallbackData("Kabul"),
                            InlineKeyboardButton.WithCallbackData("Auto Gagarin"),
                        }
                    });

                    await Bot.SendTextMessageAsync(
                        message.Chat.Id,
                        "Выбирай шишкарик",
                        replyMarkup: inlineKeyboard);
                    break;

                // send custom keyboard
                case "/keyboard":
                    ReplyKeyboardMarkup ReplyKeyboard = new[]
                    {
                        new[] { "Auto White Widow", "Crtical Kush" },
                        new[] { "Kabul", "Auto Gagarin" },
                    };

                    await Bot.SendTextMessageAsync(
                        message.Chat.Id,
                        "Выбирай шишкарик",
                        replyMarkup: ReplyKeyboard);
                    break;

                // send a photo
                case "/photo":
                    await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                    const string file = @"Files/tux.png";

                    var fileName = file.Split(Path.DirectorySeparatorChar).Last();

                    using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        await Bot.SendPhotoAsync(
                            message.Chat.Id,
                            fileStream,
                            "Nice Picture");
                    }
                    break;

                // request location or contact
                case "/request":
                    var RequestReplyKeyboard = new ReplyKeyboardMarkup(new[]
                    {
                        KeyboardButton.WithRequestLocation("Location"),
                        KeyboardButton.WithRequestContact("Contact"),
                    });

                    await Bot.SendTextMessageAsync(
                        message.Chat.Id,
                        "Who or Where are you?",
                        replyMarkup: RequestReplyKeyboard);
                    break;

                case "/weather":

                    string url = "http://api.openweathermap.org/data/2.5/weather?q=Kyiv,ua&appid=228538c118dcbd5e9d7a79ed4b760927&units=metric";
                    string jsonString = await PostCallAPI(url);
                    var pwdResetRequest = PwdResetRequest.FromJson(jsonString);

                    await Bot.SendTextMessageAsync(
                          message.Chat.Id,
                          //"\U0001F30D" + " Погода сейчас " + "\n" +
                          "\U0001f31e" + " Погода сейчас " + "\U0001f31a" + "\n" +
                          "\n" +
                          "\U0001f321" + " Температура: " + pwdResetRequest.Main.Temp.ToString() + " °C" + "\n" +
                          "\U0001f32a" + " Скорость ветра: " + pwdResetRequest.Wind.Speed.ToString() + " м/с \n" +
                          "\U0001f329" + " Шо по тучам?: " + pwdResetRequest.Weather[0].Description
                           );
                    break;

                default:
                    const string usage = @"Usage: 
/inline   - send inline keyboard
/keyboard - send custom keyboard
/request  - request location or contact 
/weather - send weather";

                    await Bot.SendTextMessageAsync(
                        message.Chat.Id,
                        usage,
                        replyMarkup: new ReplyKeyboardRemove());
                    break;
            }
        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;

            await Bot.AnswerCallbackQueryAsync(
                callbackQuery.Id,
                $"Received {callbackQuery.Data}");

            await Bot.SendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                $"Received {callbackQuery.Data}");
        }

        private static async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            Console.WriteLine($"Received inline query from: {inlineQueryEventArgs.InlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                new InlineQueryResultLocation(
                    id: "1",
                    latitude: 40.7058316f,
                    longitude: -74.2581888f,
                    title: "New York")   // displayed result
                    {
                        InputMessageContent = new InputLocationMessageContent(
                            latitude: 40.7058316f,
                            longitude: -74.2581888f)    // message if result is selected
                    },

                new InlineQueryResultLocation(
                    id: "2",
                    latitude: 13.1449577f,
                    longitude: 52.507629f,
                    title: "Berlin") // displayed result
                    {
                        InputMessageContent = new InputLocationMessageContent(
                            latitude: 13.1449577f,
                            longitude: 52.507629f)   // message if result is selected
                    }
            };

            await Bot.AnswerInlineQueryAsync(
                inlineQueryEventArgs.InlineQuery.Id,
                results,
                isPersonal: true,
                cacheTime: 0);
        }

        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message);
        }

        public static async Task<string> PostCallAPI(string url)
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
    }
}