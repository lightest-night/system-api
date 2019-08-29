using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;
using Shouldly;
using Xunit;

namespace LightestNight.System.Api.Tests
{
    public class ApiClientFactoryTests
    {
        private class TestApiClient : ApiClient
        {
            public override Task<TokenData> GetMachineToken(CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }
        }

        private readonly IApiClientFactory _sut;
        private readonly FieldInfo _restClientField = typeof(ApiClient).GetField("_restClient", BindingFlags.Instance | BindingFlags.NonPublic);

        public ApiClientFactoryTests()
        {
            _sut = new ApiClientFactory();
        }

        [Fact]
        public void Should_Create_New_Instance_Of_Client_With_CoreClient()
        {
            // Act
            var result = _sut.Create<CoreClient>();
            
            // Assert
            result.ShouldBeOfType(typeof(CoreClient));
        }
        
        [Fact]
        public void Should_Create_New_Instance_Of_Client_With_TestApiClient()
        {
            // Act
            var result = _sut.Create<TestApiClient>();
            
            // Assert
            result.ShouldBeOfType(typeof(TestApiClient));
        }

        [Fact]
        public void Should_Create_New_Instance_Of_Client_With_No_BaseUrl_Set()
        {
            // Act
            var result = _sut.Create<TestApiClient>();
            
            // Assert
            var restClient = _restClientField?.GetValue(result) as IRestClient;
            restClient.ShouldNotBeNull();
            restClient.BaseUrl.ShouldBeNull();
        }

        [Fact]
        public void Should_Create_New_Instance_Of_Client_With_BaseUrl_Set()
        {
            // Arrange
            const string baseUrl = "https://www.example.com";
            
            // Act
            var result = _sut.Create<TestApiClient>(baseUrl);
            
            // Assert
            var restClient = _restClientField?.GetValue(result) as IRestClient;
            restClient.ShouldNotBeNull();
            restClient.BaseUrl.ShouldBe(new Uri(baseUrl));
        }

        [Theory]
        [InlineData("example.com")]
        [InlineData("resource")]
        [InlineData("resource/1")]
        public void Should_Throw_Exception_If_BaseUrl_Is_Not_Absolute(string baseUrl)
        {
            // Act
            var exception = Should.Throw<UriFormatException>(() => _sut.Create<TestApiClient>(baseUrl));
            
            // Assert
            exception.Message.ShouldBe($"{baseUrl} is not a valid Absolute URI");
        }
    }
}