using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Moq;
using Newtonsoft.Json;
using RestSharp;
using Shouldly;
using Xunit;

namespace LightestNight.System.Api.Tests
{
    public class RestClientTests
    {
        private class TestClient : ApiClient
        {
            public const string MachineToken = "MACHINE_TOKEN";
            public const string Route = "/route";
            
            public TestClient(IRestClient restClient) : base(restClient)
            {
            }

            protected override Task<TokenData> GetMachineToken(CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new TokenData
                {
                    AccessToken = MachineToken
                });
            }

            protected override string ApiRoute => Route;
        }
        
        private readonly Mock<IRestClient> _restClientMock = new Mock<IRestClient>();
        private readonly TestClient _sut;

        public RestClientTests()
        {
            _sut = new TestClient(_restClientMock.Object);
        }

        [Fact]
        public async Task Should_Make_Request_With_Included_Token()
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            });
            
            var token = $"Bearer {Guid.NewGuid()}";
            var request = new RestRequest();
            request.AddHeader(HeaderNames.Authorization, token);
            
            // Act
            await _sut.MakeRequest(request, true, false, CancellationToken.None);
            
            // Assert
            _restClientMock.Verify(client => client.ExecuteTaskAsync(It.Is<IRestRequest>(req => req.Parameters.FirstOrDefault(param => param.Name == HeaderNames.Authorization && param.Type == ParameterType.HttpHeader && param.Value.ToString() == token) != default), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Make_Request_With_Machine_Token()
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            });
            
            // Act
            await _sut.MakeRequest(new RestRequest(), isApiRequest: false, cancellationToken: CancellationToken.None);
            
            // Assert
            _restClientMock.Verify(client => client.ExecuteTaskAsync(It.Is<IRestRequest>(req => req.Parameters.FirstOrDefault(param => param.Name == HeaderNames.Authorization && param.Type == ParameterType.HttpHeader && param.Value.ToString() == $"Bearer {TestClient.MachineToken}") != default), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Make_Second_Request_For_Machine_Token_If_First_Is_Unauthorized_And_Authorization_Was_Not_Provided()
        {
            // Arrange
            _restClientMock.SetupSequence(client => client.ExecuteTaskAsync(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RestResponse
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    ResponseStatus = ResponseStatus.Error
                })
                .ReturnsAsync(new RestResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseStatus = ResponseStatus.Completed
                });
            
            // Act
            await _sut.MakeRequest(new RestRequest(), isApiRequest: false, cancellationToken: CancellationToken.None);
            
            // Assert
            _restClientMock.Verify(client => client.ExecuteTaskAsync(It.Is<IRestRequest>(req => req.Parameters.FirstOrDefault(param => param.Name == HeaderNames.Authorization && param.Type == ParameterType.HttpHeader && param.Value.ToString() == $"Bearer {TestClient.MachineToken}") != default), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Should_Throw_UnauthorizedException_If_Authorization_Fails()
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse
            {
                StatusCode = HttpStatusCode.Unauthorized,
                ResponseStatus = ResponseStatus.Error
            });
            
            var token = $"Bearer {Guid.NewGuid()}";
            var request = new RestRequest();
            request.AddHeader(HeaderNames.Authorization, token);
            
            // Act & Assert
            await Should.ThrowAsync<UnauthorizedException>(() => _sut.MakeRequest(request, true, false, CancellationToken.None));
        }

        [Fact]
        public async Task Should_Throw_RestException_If_Request_Fails()
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse
            {
                StatusCode = HttpStatusCode.Locked,
                ResponseStatus = ResponseStatus.Error,
                Content = JsonConvert.SerializeObject(new { Foo = "Bar" })
            });
            
            var token = $"Bearer {Guid.NewGuid()}";
            var request = new RestRequest("/resource");
            request.AddHeader(HeaderNames.Authorization, token);
            
            // Act
            var exception = await Should.ThrowAsync<RestException>(() => _sut.MakeRequest(request, true, false, CancellationToken.None));
            
            // Assert
            exception.Message.ShouldContain(request.Resource);
            exception.Details.ShouldNotBeNull();
        }
        
        [Fact]
        public async Task Should_Make_Request_With_ApiRoute_Set()
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            });
            
            var token = $"Bearer {Guid.NewGuid()}";
            var request = new RestRequest();
            request.AddHeader(HeaderNames.Authorization, token);
            request.Resource = "/resource";

            // Act
            await _sut.MakeRequest(request, true, cancellationToken: CancellationToken.None);
            
            // Assert
            _restClientMock.Verify(client => client.ExecuteTaskAsync(It.Is<IRestRequest>(req => req.Resource.Contains(request.Resource) && req.Resource.Contains(TestClient.Route)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Throw_If_ApiRequest_Is_Requested_With_No_Resource()
        {
            // Act
            var exception = await Should.ThrowAsync<ArgumentNullException>(() => _sut.MakeRequest(new RestRequest()));
            
            // Assert
            exception.ParamName.ShouldBe(nameof(RestRequest.Resource));
        }
    }
}