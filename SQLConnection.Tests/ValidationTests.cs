using System;
using Xunit;
using SQLConnection;

namespace SQLConnection.Tests
{
    public class ValidationTests
    {
        [Theory]
        [InlineData("alice@example.com", true)]
        [InlineData("invalid-email", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsValidEmail_ReturnsExpected(string input, bool expected)
        {
            var result = ValidationHelper.IsValidEmail(input);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("1234567", true)]
        [InlineData("123456789012345", true)]
        [InlineData("123456", false)]
        [InlineData("1234567890123456", false)]
        [InlineData("abc1234567", false)]
        [InlineData("", false)]
        public void IsValidMobile_ReturnsExpected(string input, bool expected)
        {
            var result = ValidationHelper.IsValidMobile(input);
            Assert.Equal(expected, result);
        }
    }
}
