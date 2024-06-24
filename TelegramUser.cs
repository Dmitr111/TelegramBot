namespace TelegramBot
{
    public class TelegramUser
    {
        // Свойство для хранения ChatId
        public long ChatId { get; set; }

        // Свойство для хранения выбранного типа отходов
        public string? SelectedTypeWaste { get; set; }

        // Свойстводля отправления подсказок
        public bool IsSendingTips { get; set; }

        // Свойство для хранения последннего общения с пользоватеелм
        public DateTime LastMessageTime { get; set; }

        // Поле для хранения типов отходов для утилизации
        private string[] wastePoints = ["Пищевые отходы", "Стеклотара", "Металлолом", "Пластик", "Батарейки и аккумуляторы", "Текстиль", "Макулатура"];

        //Свойство для хранения в ключе типы отходов для утилизации, а в значение - количество посещений пункта приема мусора
        public Dictionary<string, int> NumberPointsVisited { get; set; } = new Dictionary<string, int>();

        // Конструктор с параметром ChatID
        public TelegramUser(long chatId)
        {
            ChatId = chatId;
            SelectedTypeWaste = null;
            IsSendingTips = true;
            LastMessageTime = DateTime.Now;

            for (int i = 0; i < wastePoints.Length; i++)
            {
                NumberPointsVisited.Add(wastePoints[i], 0);
            }
        }

        // Метод для изменения выбранного типа отходов
        public void ChangeSelectedTypeWaste(string selectedTypeWaste)
        {
            SelectedTypeWaste = selectedTypeWaste;
        }

        // Метод для изменения отправки подсказок
        public void ChangeIsSendingTips()
        {
            if (IsSendingTips)
            {
                IsSendingTips = false;
            }
            else
            {
                IsSendingTips = true;
            }
        }

        // Метод для увеличения количества запросов
        public void IncrementRequestCount(string key)
        {
            NumberPointsVisited[key] += 1;
        }
    }
}
