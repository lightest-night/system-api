using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using Shouldly;
using Xunit;

namespace LightestNight.System.Api.Tests
{
    public class GenericApiClientTests
    {
        private class TestClient : ApiClient
        {
            public const string MachineToken = "MACHINE_TOKEN";
            
            public TestClient(string baseUrl) : base(baseUrl)
            {
            }

            public override Task<TokenData> GetMachineToken(CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new TokenData
                {
                    AccessToken = MachineToken
                });
            }
        }

        private readonly Mock<IRestClient> _restClientMock = new Mock<IRestClient>();
        private readonly TestClient _sut;

        public GenericApiClientTests()
        {
            _sut = new TestClient("http://example.com");
            var restClientField = typeof(TestClient).BaseType?.GetField("_restClient", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
            
            if (restClientField != null)
                restClientField.SetValue(_sut, _restClientMock.Object);
        }

        [Fact]
        public async Task Should_Make_Request_With_Included_Token()
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync<TestObject>(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse<TestObject>
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            });
            
            var token = $"{Guid.NewGuid()}";
            var request = new ApiRequest("/resource")
            {
                Authorization = new Authorization(AuthorizationType.Bearer, token)
            };
            
            // Act
            await _sut.MakeApiRequest<TestObject>(request, CancellationToken.None);
            
            // Assert
            _restClientMock.Verify(client => client.ExecuteTaskAsync<TestObject>(It.Is<IRestRequest>(req => req.Parameters.FirstOrDefault(param => param.Name == HeaderNames.Authorization && param.Type == ParameterType.HttpHeader && param.Value.ToString() == $"Bearer {token}") != default), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Make_Request_With_Machine_Token()
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync<TestObject>(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse<TestObject>
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            });

            var request = new ApiRequest("resource");
            
            // Act
            await _sut.MakeApiRequest<TestObject>(request, CancellationToken.None);
            
            // Assert
            _restClientMock.Verify(client => client.ExecuteTaskAsync<TestObject>(It.Is<IRestRequest>(req => req.Parameters.FirstOrDefault(param => param.Name == HeaderNames.Authorization && param.Type == ParameterType.HttpHeader && param.Value.ToString() == $"Bearer {TestClient.MachineToken}") != default), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Make_Second_Request_For_Machine_Token_If_First_Is_Unauthorized_And_Authorization_Was_Not_Provided()
        {
            // Arrange
            _restClientMock.SetupSequence(client => client.ExecuteTaskAsync<TestObject>(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse<TestObject>
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ResponseStatus = ResponseStatus.Error
                })
                .ReturnsAsync(new RestResponse<TestObject>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseStatus = ResponseStatus.Completed
                });

            var request = new ApiRequest("resource");
            
            // Act
            await _sut.MakeApiRequest<TestObject>(request, CancellationToken.None);
            
            // Assert
            _restClientMock.Verify(client => client.ExecuteTaskAsync<TestObject>(It.Is<IRestRequest>(req => req.Parameters.FirstOrDefault(param => param.Name == HeaderNames.Authorization && param.Type == ParameterType.HttpHeader && param.Value.ToString() == $"Bearer {TestClient.MachineToken}") != default), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Should_Throw_UnauthorizedException_If_Authorization_Fails()
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync<TestObject>(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse<TestObject>
            {
                StatusCode = HttpStatusCode.Unauthorized,
                ResponseStatus = ResponseStatus.Error
            });
            
            var token = $"{Guid.NewGuid()}";
            var request = new ApiRequest("resource")
            {
                Authorization = new Authorization(AuthorizationType.Bearer, token)
            };
            
            // Act & Assert
            await Should.ThrowAsync<UnauthorizedException>(() => _sut.MakeApiRequest<TestObject>(request, CancellationToken.None));
        }

        [Fact]
        public async Task Should_Throw_RestException_If_Request_Fails()
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync<TestObject>(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse<TestObject>
            {
                StatusCode = HttpStatusCode.Locked,
                ResponseStatus = ResponseStatus.Error,
                Content = JsonConvert.SerializeObject(new { Foo = "Bar" })
            });
            _restClientMock.Setup(client => client.BuildUri(It.IsAny<IRestRequest>())).Returns(new Uri("https://localhost/resource"));
            
            var token = $"{Guid.NewGuid()}";
            var request = new ApiRequest("resource")
            {
                Authorization = new Authorization(AuthorizationType.Bearer, token)
            };
            
            // Act
            var exception = await Should.ThrowAsync<RestException>(() => _sut.MakeApiRequest<TestObject>(request, CancellationToken.None));
            
            // Assert
            exception.Message.ShouldContain(request.Resource);
            exception.Content.ShouldNotBeNull();
        }
        
        [Fact]
        public async Task Should_Make_Request_With_ApiRoute_Set()
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync<TestObject>(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse<TestObject>
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            });
            
            var token = $"{Guid.NewGuid()}";
            var request = new ApiRequest("resource")
            {
                Authorization = new Authorization(AuthorizationType.Bearer, token),
                Edge = "/edge"
            };

            // Act
            await _sut.MakeApiRequest<TestObject>(request, CancellationToken.None);
            
            // Assert
            _restClientMock.Verify(client => client.ExecuteTaskAsync<TestObject>(It.Is<IRestRequest>(req => req.Resource.Contains(request.Resource) && req.Resource.Contains(request.Edge)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Throw_If_ApiRequest_Is_Requested_With_No_Resource()
        {
            // Act
            var exception = await Should.ThrowAsync<ArgumentNullException>(() => _sut.MakeApiRequest<TestObject>(new ApiRequest(string.Empty)));
            
            // Assert
            exception.ParamName.ShouldBe(nameof(RestRequest.Resource));
        }
    }
}