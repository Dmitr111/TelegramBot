using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    public class Bot
    {
        private static bool running = true;

        private static Dictionary<string, string> recommendations = new Dictionary<string, string>();

        private static Dictionary<string, int> scores = new Dictionary<string, int>();

        static void Main()
        {
            ITelegramBotClient botClient = new TelegramBotClient("7076546636:AAEWOOEy-Xb6TFR6lQ1flNSPXrenNZ253pw");

            Console.WriteLine("Запущен бот: " + botClient.GetMeAsync().Result.FirstName);

            var cancellationToken = new CancellationTokenSource().Token;

            // Разрешаем получать все виды апдейтов
            var receiverOptions = new ReceiverOptions { AllowedUpdates = { }, };

            botClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions, cancellationToken);

            // Добавление в хэш-таблицу советов
            recommendations.Add("Пищевые отходы", "Используйте пищевые отходы для компостирования и создания питательного почвенного удобрения для ваших растений и сада.");
            recommendations.Add("Стеклотара", "Помните, что стекло можно переработать бесконечное количество раз, поэтому не выбрасывайте его – сдайте на переработку.");
            recommendations.Add("Металлолом", "Сдавайте металлолом на переработку – это позволит сэкономить природные ресурсы и снизить загрязнение окружающей среды.");
            recommendations.Add("Пластик", "Попробуйте уменьшить использование одноразовых пластиковых изделий и предпочитайте продукцию, упакованную в более устойчивые материалы.");
            recommendations.Add("Батарейки и аккумуляторы", "Правильно утилизируйте старые батарейки и аккумуляторы, сдав их в специальные пункты приема.");
            recommendations.Add("Текстиль", "Подумайте о возможности переплавить старую одежду или использовать ее для создания новых предметов, чтобы уменьшить количество текстильных отходов.");
            recommendations.Add("Макулатура", "Отдавайте старые газеты, журналы и книги на переработку для производства новой макулатуры и бумаги.");

            const int maxScore = 55;

            // Добавление в хэш-таблицу баллов
            scores.Add("Пищевые отходы", maxScore - 34);
            scores.Add("Стеклотара", maxScore - 3);
            scores.Add("Металлолом", maxScore - 2);
            scores.Add("Пластик", maxScore - 45);
            scores.Add("Батарейки и аккумуляторы", maxScore - 5);
            scores.Add("Текстиль", maxScore - 4);
            scores.Add("Макулатура", maxScore - 39);

            Console.WriteLine("Нажмите 'Q', чтобы остановить бота.");

            // Создаем таймер, который будет вызывать функцию CheckTelegrammBot с интервалом 5 дней (432000000 милисекунды)
            var timer = new Timer(CheckTelegrammBot, botClient, 0, 432000000);

            // В бесконечном цикле ждем нажатия клавиши
            while (running)
            {
                var key = Console.ReadKey(true);

                // Если нажата клавиша Q, останавливаем бот
                if (key.Key == ConsoleKey.Q)
                {
                    running = false;
                }
            }

            Console.WriteLine("Остановлен бот: " + botClient.GetMeAsync().Result.FirstName); ;

            // Функция проверки пользователей в Telegram
            static void CheckTelegrammBot(object state)
            {
                var botClient = (ITelegramBotClient)state;

                if (running)
                {
                    // Чтение списка пользователей из файла
                    string fileContent = System.IO.File.ReadAllText(filePath);
                    List<TelegramUser> loadedUsers = JsonConvert.DeserializeObject<List<TelegramUser>>(fileContent);

                    foreach (var user in loadedUsers)
                    {
                        var currentTime = DateTime.Now;
                        var elapsedTime = currentTime - user.LastMessageTime;

                        // Прошло более 5 дней с момента последнего сообщения
                        if (elapsedTime.TotalDays >= 5)
                        {
                            botClient.SendTextMessageAsync(user.ChatId, "Ты не писал нам более 5 дней. Что-то случилось?");
                            Console.WriteLine($"Напоминание отправлено пользователю: {user.ChatId}");

                            // Обновляем время последнего сообщения у пользователя
                            user.LastMessageTime = currentTime;
                        }

                        // Сохраняем обновленный список пользователей в файл
                        string json = JsonConvert.SerializeObject(loadedUsers, Formatting.Indented);
                        System.IO.File.WriteAllText(filePath, json);
                    }
                }
            }
        }

        private static readonly string staticApiKey = "ec36e5d5-86d7-4cd8-9a64-fde9a1fb5923";
        private static readonly string organizationSearchApiKey = "cc30b60b-0acf-48e3-9e06-ae7090771183";
        private static readonly string filePath = "users.json";

        private static double latitude;
        private static double longitude;
        private static bool isSendingСoordinates = false;

        // Создание списка пользователей
        private static string fileContent = System.IO.File.ReadAllText(filePath);
        private static List<TelegramUser> users = JsonConvert.DeserializeObject<List<TelegramUser>>(fileContent);
        private static TelegramUser? user;

        // Клавиатура для выбора типа отходов
        private readonly static InlineKeyboardMarkup inlineSelectedTypeWasteKeyBoard = new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>()
        {
            new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("♻️Пищевые отходы🍔", "button4"),
            },
            new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("♻️Стеклотара\U0001fa9f", "button5"),
            },
            new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("♻️Металлолом🔩", "button6"),
            },
            new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("♻️Пластик🥤", "button7"),
            },
            new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("♻️Батарейки🔋", "button8"),
            },
            new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("♻️Текстиль\U0001f9f5", "button9"),
            },
            new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("♻️Макулатура📜", "button10"),
            },
        });

        // Клавиатура для стартового меню
        private readonly static InlineKeyboardMarkup inlineStartKeyboard = new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>()
        {
            new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("⚙️Профиль", "button1"),
                InlineKeyboardButton.WithCallbackData("❓Помощь", "button3"),
            },
            new InlineKeyboardButton[]
            {
                InlineKeyboardButton.WithCallbackData("♻️Сортировка отходов", "button2"),
            },
        });

        // Функция проверки наличия пользователей в файле
        private static void CheckUsers(long userId)
        {
            // Проверяем, есть ли список пользователей
            if (users == null)
            {
                user = new TelegramUser(userId);
                users = [user];
            }
            else
            {
                // Проверяем, существует ли пользователь в списке
                var userExisting = users.FirstOrDefault(u => u.ChatId == userId);

                if (userExisting == null)
                {
                    // Добавляем нового пользователя в список
                    user = new TelegramUser(userId);

                    users.Add(user);
                }
                else
                {
                    user = userExisting;
                }
            }

            // Сохраняем время последнего сообщения
            user.LastMessageTime = DateTime.Now;

            // Сохраняем обновленный список пользователей в файл
            string json = JsonConvert.SerializeObject(users, Formatting.Indented);
            System.IO.File.WriteAllText(filePath, json);
        }

        // Метод обновления обработчика для обновлений в Telegram боте
        public static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        {
                            CheckUsers(update.Message.Chat.Id);

                            switch (update.Message.Type)
                            {
                                // Текстовый тип
                                case MessageType.Text:
                                    {
                                        // Вывод данных о пользователи в консоль
                                        Console.WriteLine($"{update.Message.Chat.Username ?? null: unknown}     |     {update.Message.Text}");

                                        // тут обрабатываем команду /start
                                        if (update.Message.Text.ToLower().Contains("start"))
                                        {
                                            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Привет, Я - Recycle Slon Perm, вот что я умею", replyMarkup: inlineStartKeyboard);

                                            return;
                                        }

                                        // тут обрабатываем команду /help
                                        if (update.Message.Text.ToLower().Contains("help"))
                                        {
                                            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "\n/help - вывести список команд\n/start - начать/перезапустить бота\n/data - вывести профиль и выбранные настройки" +
                                                "\nЕсли у вас возникили вопросы, то @Vov41kend и @Luuckyyy всегда готовы вам помочь. Также будем рады получить обратную связь.");

                                            return;
                                        }

                                        // тут обрабатываем команду /data
                                        if (update.Message.Text.ToLower().Contains("data"))
                                        {
                                            await DisplayUserData(update.Message.Chat.Id, botClient);

                                            return;
                                        }

                                        // тут обрабатываем команду /sorting
                                        if (update.Message.Text.ToLower().Contains("sorting"))
                                        {
                                            if (user.SelectedTypeWaste == null)
                                            {
                                                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Выберите для начала тип отходов", replyMarkup: inlineSelectedTypeWasteKeyBoard);
                                            }
                                            else
                                            {
                                                await SendMessageWithGeoRequest(botClient, update.Message.Chat.Id);
                                            }

                                            return;
                                        }

                                        // Проверка на рассылку и отправку координат
                                        if (update.Message.Text.ToCharArray()[update.Message.Text.ToCharArray().Length - 1] != '$' && !isSendingСoordinates)
                                        {
                                            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "К сожалению, я не знаю такой команды. Узнать список команд /help");
                                        }

                                        if (isSendingСoordinates)
                                        {
                                            // Создаем регулярное выражение для проверки шаблона координат
                                            Regex pattern = new Regex(@"^(-?\d+(?:\.\d+)?), (-?\d+(?:\.\d+)?)$");

                                            // Получаем введенную строку сообщения
                                            Match match = pattern.Match(update.Message.Text.ToString());

                                            // Проверяем, соответствует ли введенная строка заданному шаблону
                                            if (match.Success)
                                            {
                                                // Разбиваем строку на координаты
                                                var location = update.Message.Text.ToString().Split(',');

                                                // Если пользователь не выбрал тип отходов, отправляем ему сообщение о выборе типа отходов
                                                if (user.SelectedTypeWaste == null)
                                                {
                                                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Выберите для начала тип отходов", replyMarkup: inlineSelectedTypeWasteKeyBoard);
                                                }
                                                // Если тип отходов уже выбран, извлекаем координаты и отправляем карту пользователю
                                                else
                                                {
                                                    latitude = double.Parse(location[0].Replace('.', ','));
                                                    longitude = double.Parse(location[1].Replace('.', ','));

                                                    await SendNearestPointToUser(latitude, longitude, update.Message.Chat.Id, botClient);

                                                    isSendingСoordinates = false;
                                                }
                                            }
                                            else
                                            {
                                                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Формат координат не соответствует шаблону (52.5200, 13.4050). Попробуйте еще раз");
                                            }
                                        }

                                        // Рассылка
                                        if (update.Message.Chat.Id == 1178725484 && update.Message.Text.ToCharArray()[update.Message.Text.ToCharArray().Length - 1] == '$' && update.Message.Text.ToCharArray()[0] == '$')
                                        {
                                            // Чтение списка пользователей из файла
                                            string fileContent = System.IO.File.ReadAllText(filePath);
                                            List<TelegramUser> loadedUsers = JsonConvert.DeserializeObject<List<TelegramUser>>(fileContent);

                                            var massage = update.Message.Text.ToString().Substring(1, update.Message.Text.ToString().Length - 1);

                                            foreach (var user in loadedUsers)
                                            {
                                                if (user.ChatId != 1178725484)
                                                {
                                                    await botClient.SendTextMessageAsync(user.ChatId, massage);
                                                    Console.WriteLine($"Рассылочное сообщение успешно отправлено пользователю с Chat ID: {user.ChatId}");
                                                }
                                                else
                                                {
                                                    await botClient.SendTextMessageAsync(user.ChatId, "Рассылка отправлена.");
                                                }
                                            }
                                        }

                                        return;
                                    }
                                // Геолокация
                                case MessageType.Location:
                                    {
                                        Console.WriteLine($"{update.Message.Chat.Username ?? null: unknown}     |     {update.Message.Location.Latitude}, {update.Message.Location.Longitude}");

                                        // Если пользователь не выбрал тип отходов, отправляем ему сообщение о выборе типа отходов
                                        if (user.SelectedTypeWaste == null)
                                        {
                                            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Выберите для начала тип отходов", replyMarkup: inlineSelectedTypeWasteKeyBoard);
                                        }
                                        // Если тип отходов уже выбран, извлекаем координаты и отправляем карту пользователю
                                        else
                                        {
                                            latitude = update.Message.Location.Latitude;
                                            longitude = update.Message.Location.Longitude;

                                            await SendNearestPointToUser(latitude, longitude, update.Message.Chat.Id, botClient);
                                        }

                                        return;
                                    }
                                default:
                                    {
                                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, "К сожалению, я могу распознавать только текстовый тип сообщения или вашу геоклокацию😞", replyMarkup: inlineStartKeyboard);

                                        return;
                                    }
                            }
                        }

                    case UpdateType.CallbackQuery:
                        {
                            CheckUsers(update.CallbackQuery.Message.Chat.Id);

                            switch (update.CallbackQuery.Data)
                            {
                                // Профиль
                                case "button1":
                                    {
                                        await DisplayUserData(update.CallbackQuery.Message.Chat.Id, botClient);

                                        break;
                                    }
                                // Выбор типа отходоы
                                case "button2":
                                    {
                                        await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Выберите тип отходов: ", replyMarkup: inlineSelectedTypeWasteKeyBoard);

                                        break;
                                    }
                                // Помощь
                                case "button3":
                                    {
                                        await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "\n/help - вывести список доступных команд.\n/start - начать/перезапустить бота." +
                                            "\n/start - получить метками пунктов утилизации отходов.\n/data - вывести профиль и выбранные настройки.\n" +
                                            "Если что у вас всегда есть @Vov41kend и @Luuckyyy - 2 человека готовых вам помочь. Будем рады получить обратную связь или помочь вам с возникшей проблемой");

                                        break;
                                    }
                                case "button4":
                                    {
                                        user.ChangeSelectedTypeWaste("Пищевые отходы");

                                        await SendMessageWithGeoRequest(botClient, update.CallbackQuery.Message.Chat.Id);

                                        break;
                                    }
                                case "button5":
                                    {
                                        user.ChangeSelectedTypeWaste("Стеклотара");

                                        await SendMessageWithGeoRequest(botClient, update.CallbackQuery.Message.Chat.Id);

                                        break;
                                    }
                                case "button6":
                                    {
                                        user.ChangeSelectedTypeWaste("Металлолом");

                                        await SendMessageWithGeoRequest(botClient, update.CallbackQuery.Message.Chat.Id);

                                        break;
                                    }
                                case "button7":
                                    {
                                        user.ChangeSelectedTypeWaste("Пластик"); 

                                        await SendMessageWithGeoRequest(botClient, update.CallbackQuery.Message.Chat.Id);

                                        break;
                                    }
                                case "button8":
                                    {
                                        user.ChangeSelectedTypeWaste("Батарейки и аккумуляторы");

                                        await SendMessageWithGeoRequest(botClient, update.CallbackQuery.Message.Chat.Id);

                                        break;
                                    }
                                case "button9":
                                    {
                                        user.ChangeSelectedTypeWaste("Текстиль");

                                        await SendMessageWithGeoRequest(botClient, update.CallbackQuery.Message.Chat.Id);

                                        break;
                                    }
                                case "button10":
                                    {
                                        user.ChangeSelectedTypeWaste("Макулатура");

                                        await SendMessageWithGeoRequest(botClient, update.CallbackQuery.Message.Chat.Id);

                                        break;
                                    }
                                // Отключение отправки советов
                                case "button11":
                                    {
                                        user.ChangeIsSendingTips();

                                        await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Отправка советов отключена.", replyMarkup: inlineStartKeyboard);

                                        break;
                                    }
                                // Включение отправки советов
                                case "button12":
                                    {
                                        user.ChangeIsSendingTips();

                                        await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Отправка советов включена.", replyMarkup: inlineStartKeyboard);

                                        break;
                                    }
                                case "button13":
                                    {
                                        // Отправляем сообщение с кнопкой "Отправить геопозицию"
                                        var replyKeyboard = new ReplyKeyboardMarkup(new KeyboardButton[][] { [KeyboardButton.WithRequestLocation("Отправить геопозицию")] });
                                        replyKeyboard.ResizeKeyboard = true;

                                        string messageText = "Вы можете отправить геопозицию при помощи клавиатуры. Или написать их в соответствии с шаблоном. " + 
                                            "Например: 52.5200, 13.4050\nЕсли у вас возникли проблемы, то <a href=\"https://yandex.ru/support/maps/concept/place.html\">нажав сюда вы перейдете в справочную службу Яндекс</a>";

                                        await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, messageText, parseMode: ParseMode.Html, replyMarkup: replyKeyboard);

                                        isSendingСoordinates = true;

                                        break;
                                    }

                            }

                            // Сохраняем обновленный список пользователей в файл
                            string json = JsonConvert.SerializeObject(users, Formatting.Indented);
                            System.IO.File.WriteAllText(filePath, json);

                            Console.WriteLine($"{update.CallbackQuery.Message.Chat.Username ?? null: unknown}     |     {update.CallbackQuery.Data}");

                            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);

                            return;
                        }
                }
            }
            catch (Exception exeption)
            {
                Console.WriteLine($"Ошибка: {exeption}");
            }
        }

        // Метод, который отправляет карту Yandex пользователю c ближайщим пунктом утилизации отходов
        static async Task SendNearestPointToUser(double latitude, double longitude, long chatId, ITelegramBotClient botClient)
        {
            // Получаем список адресов и их координат
            List<(string, string, string, string[])> addressCoordinateMatrix = await GetAddressesFromYandexAPI();

            // Находим ближайшую точку
            var (nearestLatitude, nearestLongitude) = FindNearestPoint(latitude, longitude, addressCoordinateMatrix);

            // Формируем URL для Yandex Static API
            var apiUrl = $"https://static-maps.yandex.ru/v1?apikey={staticApiKey}&bbox={longitude.ToString().Replace(",", ".")},{latitude.ToString().Replace(",", ".")}~" +
                $"{nearestLatitude.ToString().Replace(",", ".")},{nearestLongitude.ToString().Replace(",", ".")}&lang=ru_RU&size=450,450" +
                $"&pt={longitude.ToString().Replace(",", ".")},{latitude.ToString().Replace(",", ".")},pmwtm1~{nearestLatitude.ToString().Replace(",", ".")},{nearestLongitude.ToString().Replace(",", ".")},pmwtm2";

            string yandexMapLink = $"https://yandex.ru/maps/?rtext={latitude.ToString().Replace(",", ".")},{longitude.ToString().Replace(",", ".")}~{nearestLongitude.ToString().Replace(",", ".")}," +
                $"{nearestLatitude.ToString().Replace(",", ".")}";

            var msg = $"<a href=\"{yandexMapLink}\">Нажмите сюда, чтобы перейти на яндекс карты</a>";

            // Создание экземпляра HttpClient для выполнения HTTP запросов.
            HttpClient httpClient = new HttpClient();

            //Получение ответа от Yandex Static API.
            HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

            // Проверяем, успешен ли запрос
            if (response.IsSuccessStatusCode)
            {
                // Получение байтового массива изображения из ответа.
                byte[] imageData = await response.Content.ReadAsByteArrayAsync();

                // Создание потока MemoryStream для изображения.
                using var stream = new MemoryStream(imageData);

                // Создание объекта InputOnlineFile для передачи изображения в Telegram API.
                var inputOnlineFile = new InputOnlineFile(stream);
                await botClient.SendPhotoAsync(chatId, inputOnlineFile, msg, parseMode: ParseMode.Html);
            }
            // Выводим ошибку, если запрос не был успешным
            else
            {
                Console.WriteLine("Не удалось отправить фото. Код состояния: " + response.StatusCode);
            }
        }

        // Метод для вывода настроек пользователя
        static async Task DisplayUserData(long chatId, ITelegramBotClient botClient)
        {
            StringBuilder messageBuilder = new StringBuilder();

            // Вывод пользователей
            if (user.NumberPointsVisited.Count == 0)
            {
                await botClient.SendTextMessageAsync(chatId, $"Пока что вы не сделали ни одного запроса в боте😢");
            }
            else
            {
                // Вычисляем сумму баллов для каждого пользователя
                var totalScores = users.OrderByDescending(p => p.NumberPointsVisited.Sum(pair => pair.Value * scores[pair.Key])).Select(p => p.NumberPointsVisited.Sum(pair => pair.Value * scores[pair.Key])).ToList();

                // Сортируем пользователей по сумме баллов
                var sortedUsers = users.OrderByDescending(p => p.NumberPointsVisited.Sum(pair => pair.Value * scores[pair.Key])).ToList();

                var index = sortedUsers.IndexOf(user);

                var msg = new StringBuilder();

                // Определение сообщения в зависимости от места пользователя по количеству запросов
                if (index <= 2) { msg.Append("Поздравляю,"); }

                if (index > 2 && index <= 9) { msg.Append("Неплохо,"); }

                if (index > 10) { msg.Append("Пока что,"); }

                await botClient.SendTextMessageAsync(chatId, $"{msg} вы занимаете {index+1} место среди всех пользователей.\nКоличество ваших баллов = {totalScores[index]}");

                messageBuilder.AppendLine($"Ваша история посещения центров сбора мусора:");

                foreach (var item in user.NumberPointsVisited)
                {
                    if (item.Value != 0)
                    {
                        messageBuilder.AppendLine($"{item.Key}: {item.Value}");
                    }
                }
            }

            await botClient.SendTextMessageAsync(chatId, messageBuilder.ToString().TrimEnd('\n'));
            messageBuilder.Clear();

            // Вывод выбранного типа отходов
            if (user.SelectedTypeWaste == null)
            {
                messageBuilder.AppendLine("Текущий тип отходов не выбран");
            }
            else
            {
                messageBuilder.AppendLine($"Ваши настройки: \nТекущий тип отходов - {user.SelectedTypeWaste}");
            }

            // Вывод информации о настройках отправки советов
            if (user.IsSendingTips == true)
            {
                messageBuilder.AppendLine($"Включена отправка советов.");
                var inlineKeyboard = new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>()
                {
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("📴Отключить её", "button11"),
                    },
                });

                await botClient.SendTextMessageAsync(chatId, messageBuilder.ToString().TrimEnd('\n'), replyMarkup: inlineKeyboard);

                await botClient.SendTextMessageAsync(chatId, "Если хотите поменять настройки нажмите /start");
            }
            else
            {
                messageBuilder.AppendLine($"Отключена отправка советов.");
                var inlineKeyboard = new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>()
                {
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("🔛Включить её", "button12"),
                    },
                });

                await botClient.SendTextMessageAsync(chatId, messageBuilder.ToString().TrimEnd('\n'), replyMarkup: inlineKeyboard);

                await botClient.SendTextMessageAsync(chatId, "Если хотите поменять настройки нажмите /start");
            }
        }

        // Метод для отправки Yandex карты пользователю с отмеченными на ней метками центров утилизации отходов
        static async Task SendMessageWithGeoRequest(ITelegramBotClient botClient, long chatId)
        {
            await botClient.SendTextMessageAsync(chatId, $"Выбранный тип отходов: {user.SelectedTypeWaste}");

            // Получение матрицы адресов и координат
            List<(string, string, string, string[])> addressCoordinateMatrix = await GetAddressesFromYandexAPI();

            // Метод для отправки карты
            await BuildMapWithWasteRecyclingCenters(botClient, chatId, addressCoordinateMatrix);

            // Создание инлайн клавиатуры для помощи и возврата назад
            var inlineKeyboard = new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>()
            {
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("❓Помощь", "button3"),
                    InlineKeyboardButton.WithCallbackData("↩️Назад", "button2"),
                },
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("📴Отключить отправку советов", "button11"),
                },
            });

            // Если включена отправка советов, отправка рекомендации с использованием инлайн клавиатуры
            if (user.IsSendingTips == true)
            {
                await botClient.SendTextMessageAsync(chatId, recommendations[user.SelectedTypeWaste], replyMarkup: inlineKeyboard);
            }

            // Создание инлайн клавиатуры для отправки координат
            var inlineGeoCoordinatesKeyBoard = new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>() { new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Отправить координаты", "button13"), }, });

            await botClient.SendTextMessageAsync(chatId, "Могу также построить маршрут до ближайшего центра утилизации.", replyMarkup: inlineGeoCoordinatesKeyBoard);

            user.IncrementRequestCount(user.SelectedTypeWaste);
        }

        // Метод для нахождения координат ближайщего к пользователю пункта утилизации отходов
        static (double, double) FindNearestPoint(double longitude, double latitude, List<(string, string, string, string[])> addressCoordinateMatrix)
        {
            double nearestDistance = double.MaxValue;
            double nearestLatitude = 0;
            double nearestLongitude = 0;    

            // Перебор всех элементов в матрице адресов и координат
            foreach (var item in addressCoordinateMatrix)
            {
                // Получение широты и долготы точки из матрицы, замена точки на запятую
                var pointLatitude = double.Parse(item.Item4[0].Replace(".", ","));
                var pointLongitude = double.Parse(item.Item4[1].Replace(".", ","));

                // Расчет расстояния между текущей точкой и заданными координатами
                var distance = CalculateDistance(longitude, latitude, pointLongitude, pointLatitude);

                // Сравнение расстояния с текущим ближайшим расстоянием и обновление значений, если текущее расстояние меньше
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestLatitude = pointLatitude;
                    nearestLongitude = pointLongitude;
                }
            }

            // Возврат координат ближайшей точки
            return (nearestLatitude, nearestLongitude);
        }

        // Метод для вычисления расстояния между двумя точками в км
        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371d; // Радиус Земли в км

            // Разницы в широте и долготе, переведенные в радианы
            var dLat = DegToRad(lat2 - lat1);
            var dLon = DegToRad(lon2 - lon1);

            // Формула для вычисления расстояния между двумя точками на поверхности Земли (формула Гаверсинуса)
            var a =
                Math.Sin(dLat / 2d) * Math.Sin(dLat / 2d) +
                Math.Cos(DegToRad(lat1)) * Math.Cos(DegToRad(lat2)) *
                Math.Sin(dLon / 2d) * Math.Sin(dLon / 2d);
            var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));

            // Расстояние между двумя точками в км
            var d = R * c;

            return d;
        }

        // Метод для перевода в радианы
        static double DegToRad(double deg)
        {
            return deg * (Math.PI / 180d);
        }

        // Метод для извлечения имени пункта, его телефона, адреса и координат из JSON-ответа
        private static async Task<List<(string, string, string, string[])>> GetAddressesFromYandexAPI()
        {
            // Создаем URL для запроса к API Yandex картам
            string organizationSearchApiUrl = $"https://search-maps.yandex.ru/v1/?text=пункты приёма {user.SelectedTypeWaste}, " +
                $"Пермь&lang=ru_RU&apikey={organizationSearchApiKey}&results=10";

            // Создание экземпляра HttpClient для выполнения HTTP запросов
            HttpClient httpClient = new HttpClient();

            // Получение ответа от Yandex Static API
            HttpResponseMessage response = await httpClient.GetAsync(organizationSearchApiUrl);

            // Проверяем, успешен ли запрос
            if (response.IsSuccessStatusCode)
            {
                // Читаем содержимое ответа
                string responseData = await response.Content.ReadAsStringAsync();
                JObject jsonResponse = JObject.Parse(responseData);

                // Извлекаем имя пункта, его телефон, адрес и координаты из JSON-ответа и создаем список кортежей
                List<(string, string, string, string[])> result = jsonResponse["features"]
                    .Select(f => ((string)f["properties"]["name"] ?? "",string.Join(", ", ((JObject)f["properties"]["CompanyMetaData"])["Phones"]?
                    .Select(p => (string)p["formatted"]).DefaultIfEmpty(string.Empty).ToArray() ?? []),
                    (string)f["properties"]["CompanyMetaData"]?["address"] ?? "", ((JArray)f["geometry"]["coordinates"])?.Select(c => (string)c).ToArray() ?? [])).ToList();

                // Возращаем список кортежей
                return result;
            }
            // Иначе выводим ошибку и бросаем исключение
            else
            {
                Console.WriteLine("Не удалось получить данные. Код состояния: " + response.StatusCode);
                throw new Exception("Не удалось получить данные. Код состояния: " + response.StatusCode);
            }
        }

        // Метод для отправки карты с метками центров утилизации пользователю
        private static async Task BuildMapWithWasteRecyclingCenters(ITelegramBotClient client, long chatId, List<(string, string, string, string[])> addressCoordinateMatrix)
        {
            double MinLatitude = double.MaxValue;
            double MaxLatitude = double.MinValue;
            double MinLongitude = double.MaxValue;
            double MaxLongitude = double.MinValue;

            string? coordinatesLat = null;
            string? coordinatesLong = null;

            // Проходим по списку кортежей с адресами и координатами
            foreach (var item in addressCoordinateMatrix)
            {
                // Извлекаем широту и долготу из массива координат
                double latitude = double.Parse(item.Item4[0].Replace(".", ","));
                double longitude = double.Parse(item.Item4[1].Replace(".", ","));

                // Сохраняем координаты в переменные
                coordinatesLat = item.Item4[0].ToString();
                coordinatesLong = item.Item4[1].ToString();

                // Обновляем минимальные и максимальные значения широты и долготы
                MaxLatitude = Math.Max(MaxLatitude, latitude);
                MaxLongitude = Math.Max(MaxLongitude, longitude);
                MinLatitude = Math.Min(MinLatitude, latitude);
                MinLongitude = Math.Min(MinLongitude, longitude);
            }

            StringBuilder messageBuilder = new StringBuilder();

            // Проходим по списку кортежей с адресами и координатами и формируем сообщение
            int j = 1;
            foreach (var item in addressCoordinateMatrix)
            {
                // Формируем ссылку на Yandex карты для адреса
                string yandexMapLink = $"https://yandex.ru/maps/?text={item.Item3}";

                // Формируем строку с телефонами (если есть)
                string phoneNumbers = string.IsNullOrEmpty(item.Item2) ? "отсутствует" : item.Item2;

                // Добавляем информацию об организации в сообщениея
                messageBuilder.AppendLine($"{j}. {item.Item1}\n<a href=\"{yandexMapLink}\">{item.Item3}</a>\nТелефон(ы): {phoneNumbers}");
                j++;
            }

            // Формируем Static Api запрос в Yandex карты
            string staticMapUrl = $"https://static-maps.yandex.ru/v1?apikey={staticApiKey}" +
                $"&bbox={MaxLatitude.ToString().Replace(",", ".")},{MaxLongitude.ToString().Replace(",", ".")}~" +
                $"{MinLatitude.ToString().Replace(",", ".")},{MinLongitude.ToString().Replace(",", ".")}" +
                $"&lang=ru_RU&size=450,450&z=9&pt={coordinatesLat},{coordinatesLong},pmwtm1~";

            // Добавляем метки в на карту
            int z = 0;
            foreach (var item in addressCoordinateMatrix)
            {
                staticMapUrl += $"{item.Item4[0]},{item.Item4[1]},pm2rdm";
                if (z < addressCoordinateMatrix.Count - 1)
                {
                    staticMapUrl += "~";
                }
                z++;
            }

            // Создание экземпляра HttpClient для выполнения HTTP запросов
            HttpClient httpClient = new HttpClient();

            // Получение ответа от Yandex Static API
            HttpResponseMessage response = await httpClient.GetAsync(staticMapUrl);

            // Проверяем, успешен ли запрос
            if (response.IsSuccessStatusCode)
            {
                // Получение байтового массива изображения из ответа.
                byte[] imageData = await response.Content.ReadAsByteArrayAsync();

                // Формируем сообщение
                string message = messageBuilder.ToString().TrimEnd('\n');

                await client.SendTextMessageAsync(chatId, "Найденные центры утилизации");

                // Создание потока MemoryStream для изображения.
                using var stream = new MemoryStream(imageData);

                // Создание объекта InputOnlineFile для передачи изображения в Telegram API.
                var inputOnlineFile = new InputOnlineFile(stream);
                await client.SendPhotoAsync(chatId, inputOnlineFile, message, parseMode: ParseMode.Html);
            }
            // Выводим ошибку, если запрос не был успешным
            else
            {
                Console.WriteLine("Не удалось отправить фото. Код состояния: " + response.StatusCode);
            }
        }

        // Метод для обработки исключений
        private static Task ErrorHandler(ITelegramBotClient client, Exception error, CancellationToken token)
        {
            // Определяем сообщение об ошибке в зависимости от типа исключения
            var ErrorMessage = error switch
            {
                // Если это исключение ApiRequestException, то создаем сообщение в формате "Telegram API Error: [ErrorCode] [Message]"
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                // В противном случае, просто выводим текст ошибки
                _ => error.ToString()
            };

            // Выводим сообщение об ошибке в консоль
            Console.WriteLine(ErrorMessage);

            // Возвращаем задачу, которая была выполнена
            return Task.CompletedTask;
        }
    }
}