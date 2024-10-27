using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using уп;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private avtorizatia form;

        [TestInitialize]
        public void Setup()
        {
            // Инициализация формы перед каждым тестом
            form = new avtorizatia();
        }

        [TestMethod]
        public void Test_InvalidPassword_ShouldFailLogin()
        {
            // Arrange
            string login = "login1"; // менеджер
            string invalidPassword = "wrongpass";

            // Act
            bool result = CallValidateUserMethod(login, invalidPassword);

            // Assert
            Assert.IsFalse(result, "Login should fail with incorrect password.");
        }

        [TestMethod]
        public void Test_InvalidLogin_ShouldFailLogin()
        {
            // Arrange
            string invalidLogin = "invalidLogin";
            string password = "pass1";

            // Act
            bool result = CallValidateUserMethod(invalidLogin, password);

            // Assert
            Assert.IsFalse(result, "Login should fail with incorrect login.");
        }

        [TestMethod]
        public void Test_ValidUserLogin_ManagerRole()
        {
            // Arrange
            string login = "login1"; // менеджер
            string password = "pass1";

            // Act
            bool result = CallValidateUserMethod(login, password);
            string userRole = GetUserRole();

            // Assert
            Assert.IsTrue(result, "User should be able to log in.");
            Assert.AreEqual("Менеджер", userRole, "Role should be Manager.");
        }

        [TestMethod]
        public void Test_BlockUser_AfterMultipleFailedAttempts()
        {
            // Arrange
            string login = "login1"; // менеджер
            string wrongPassword = "wrongpass";

            // Act
            SimulateFailedLoginAttempts(login, wrongPassword, 3);
            bool isBlocked = IsUserBlocked();

            // Assert
            Assert.IsTrue(isBlocked, "User should be blocked after multiple failed login attempts.");
        }

        [TestMethod]
        public void Test_ValidUserLogin_AutoMechanicRole()
        {
            // Arrange
            string login = "login2"; // автомеханик
            string password = "pass2";

            // Act
            bool result = CallValidateUserMethod(login, password);
            string userRole = GetUserRole();

            // Assert
            Assert.IsTrue(result, "User should be able to log in.");
            Assert.AreEqual("Автомеханик", userRole, "Role should be AutoMechanic.");
        }

        [TestMethod]
        public void Test_UnblockUser_AfterBlock()
        {
            // Arrange
            string login = "login1"; // менеджер
            string wrongPassword = "wrongpass";

            // Act
            SimulateFailedLoginAttempts(login, wrongPassword, 3);
            UnblockUser();

            // Assert
            Assert.IsFalse(IsUserBlocked(), "User should be unblocked after UnblockUser is called.");
        }

        private bool CallValidateUserMethod(string login, string password)
        {
            MethodInfo validateUserMethod = typeof(avtorizatia).GetMethod("ValidateUser", BindingFlags.NonPublic | BindingFlags.Instance);
            return (bool)validateUserMethod.Invoke(form, new object[] { login, password });
        }

        private string GetUserRole()
        {
            FieldInfo roleField = typeof(avtorizatia).GetField("UserRole", BindingFlags.NonPublic | BindingFlags.Instance);
            return roleField.GetValue(form)?.ToString();
        }

        private void SimulateFailedLoginAttempts(string login, string password, int attempts)
        {
            for (int i = 0; i < attempts; i++)
            {
                CallValidateUserMethod(login, password);
            }
        }

        private bool IsUserBlocked()
        {
            FieldInfo blockField = typeof(avtorizatia).GetField("isBlocked", BindingFlags.NonPublic | BindingFlags.Instance);
            return (bool)blockField.GetValue(form);
        }

        private void UnblockUser()
        {
            MethodInfo unblockUserMethod = typeof(avtorizatia).GetMethod("UnblockUser", BindingFlags.NonPublic | BindingFlags.Instance);
            unblockUserMethod.Invoke(form, null);
        }
    }
}
