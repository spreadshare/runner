using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using SpreadShare.Models.Exceptions;
using SpreadShare.Models.Poco;
using SpreadShare.SupportServices.SettingsServices;
using Xunit;
using Xunit.Abstractions;

namespace SpreadShare.Tests.Models.Poco
{
    public class AdministratorSettingsPocoTest : BaseTest
    {
        private IConfiguration _config;

        public AdministratorSettingsPocoTest(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile(Path.Join(Directory, "AdministratorSettingsPocoTest.json"))
                .Build();
        }

        private string Directory { get; } = Path.GetFullPath(@"Models/Poco/");

        [Fact]
        public void ConstructorHappyFlow()
        {
            var poco = _config
                .GetSection("HappyFlow")
                .Get<AdministratorSettingsPoco>(opt => opt.BindNonPublicProperties = true);
            Assert.NotNull(poco);
            Assert.False(string.IsNullOrEmpty(poco.AdminEmail));
            Assert.False(string.IsNullOrEmpty(poco.AdminPassword));
            Assert.NotEmpty(poco.Recipients);
            var unused = new AdministratorSettings(poco);
        }

        [Fact]
        public void ConstructorNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AdministratorSettings(null));
        }

        [Fact]
        public void ConstructorEmpty()
        {
            var poco = _config
                .GetSection("_")
                .Get<AdministratorSettingsPoco>(opt => opt.BindNonPublicProperties = true);
            Assert.Null(poco);
        }

        [Fact]
        public void AdminEmailInvalid()
        {
            var poco = _config
                .GetSection("AdminEmailInvalid")
                .Get<AdministratorSettingsPoco>(opt => opt.BindNonPublicProperties = true);
            Assert.Throws<InvalidEmailException>(() => new AdministratorSettings(poco));
        }

        [Fact]
        public void AdminEmailNonGmail()
        {
            var poco = _config
                .GetSection("AdminEmailNonGmail")
                .Get<AdministratorSettingsPoco>(opt => opt.BindNonPublicProperties = true);
            Assert.Throws<InvalidEmailException>(() => new AdministratorSettings(poco));
        }

        [Fact]
        public void AdminEmailMissing()
        {
            var poco = _config
                .GetSection("AdminEmailMissing")
                .Get<AdministratorSettingsPoco>(opt => opt.BindNonPublicProperties = true);
            Assert.Throws<InvalidConfigurationException>(() => new AdministratorSettings(poco));
        }

        [Fact]
        public void AdminPasswordMissing()
        {
            var poco = _config
                .GetSection("AdminPasswordMissing")
                .Get<AdministratorSettingsPoco>(opt => opt.BindNonPublicProperties = true);
            Assert.Throws<InvalidConfigurationException>(() => new AdministratorSettings(poco));
        }

        [Fact]
        public void RecipientsMissing()
        {
            var poco = _config
                .GetSection("RecipientsMissing")
                .Get<AdministratorSettingsPoco>(opt => opt.BindNonPublicProperties = true);
            var settings = new AdministratorSettings(poco);
            Assert.NotNull(settings.Recipients);
            Assert.Empty(settings.Recipients);
        }

        [Fact]
        public void RecipientsNotAllValid()
        {
            var poco = _config
                .GetSection("RecipientsNotAllValid")
                .Get<AdministratorSettingsPoco>(opt => opt.BindNonPublicProperties = true);
            Assert.Throws<InvalidEmailException>(() => new AdministratorSettings(poco));
        }
    }
}