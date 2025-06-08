using BlazorApp.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Shared.Tests
{
    [TestClass]
    public class AuthUserTests
    {
        [TestMethod]
        public void Test_GetClaimValue_ReturnsCorrectValue()
        {
            // Arrange
            var authUser = new AuthUser
            {
                Claims = new[]
                {
                    new AuthClaim { Type = "given_name", Value = "John" },
                    new AuthClaim { Type = "family_name", Value = "Doe" },
                    new AuthClaim { Type = "emails", Value = "john.doe@example.com" }
                }
            };

            // Act & Assert
            Assert.AreEqual("John", authUser.GetClaimValue("given_name"));
            Assert.AreEqual("Doe", authUser.GetClaimValue("family_name"));
            Assert.AreEqual("john.doe@example.com", authUser.GetClaimValue("emails"));
            Assert.AreEqual("", authUser.GetClaimValue("nonexistent"));
        }

        [TestMethod]
        public void Test_FirstName_Property()
        {
            // Arrange
            var authUser = new AuthUser
            {
                Claims = new[]
                {
                    new AuthClaim { Type = "given_name", Value = "Jane" }
                }
            };

            // Act & Assert
            Assert.AreEqual("Jane", authUser.FirstName);
        }

        [TestMethod]
        public void Test_EmptyClaimsArray()
        {
            // Arrange
            var authUser = new AuthUser();

            // Act & Assert
            Assert.AreEqual("", authUser.FirstName);
            Assert.AreEqual("", authUser.LastName);
            Assert.AreEqual("", authUser.Email);
            Assert.AreEqual("", authUser.Name);
        }
    }
}