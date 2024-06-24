namespace TelegramBot.Tests
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestTelegramUser_Constructor()
        {
            // Arrange && Act
            var user = new TelegramUser(123);

            // Assert
            Assert.AreEqual(user.ChatId, 123);
            Assert.AreEqual(user.SelectedTypeWaste, null);
            Assert.AreEqual(user.IsSendingTips, true);
            Assert.AreEqual(user.NumberPointsVisited.Sum(pair => pair.Value), 0);
        }

        [TestMethod]
        public void TestTelegramUser_ChangeSelectedTypeWasteMethod()
        {
            // Arrange
            var user = new TelegramUser(123);

            // Act
            var actual = "Пластик";
            user.ChangeSelectedTypeWaste(actual);

            // Assert
            Assert.AreEqual(user.ChatId, 123);
            Assert.AreEqual(user.SelectedTypeWaste, actual);
            Assert.AreEqual(user.IsSendingTips, true);
            Assert.AreEqual(user.NumberPointsVisited.Sum(pair => pair.Value), 0);
        }

        [TestMethod]
        public void TestTelegramUser_ChangeIsSendingTipsMethod()
        {
            // Arrange
            var user = new TelegramUser(123);

            // Act
            user.ChangeIsSendingTips();
            user.ChangeIsSendingTips();

            // Assert
            Assert.AreEqual(user.ChatId, 123);
            Assert.AreEqual(user.SelectedTypeWaste, null);
            Assert.AreEqual(user.IsSendingTips, true);
            Assert.AreEqual(user.NumberPointsVisited.Sum(pair => pair.Value), 0);
        }
        [TestMethod]
        public void TestTelegramUser_IncrementRequestCountMethod()
        {
            // Arrange
            var user = new TelegramUser(123);

            // Act
            var actual = "Металлолом";
            user.IncrementRequestCount(actual);

            // Assert
            Assert.AreEqual(user.ChatId, 123);
            Assert.AreEqual(user.SelectedTypeWaste, null);
            Assert.AreEqual(user.IsSendingTips, true);
            Assert.AreEqual(user.NumberPointsVisited.Sum(pair => pair.Value), 1);
        }
    }
}