using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Moq;
using Newtonsoft.Json;
using RestSharp;
using Shouldly;
using Xunit;

namespace LightestNight.System.Api.Tests
{
    public class ApiClientTests
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

        public ApiClientTests()
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
            _restClientMock.Setup(client => client.ExecuteTaskAsync(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            });
            
            var token = $"{Guid.NewGuid()}";
            var request = new ApiRequest("resource")
            {
                Authorization = new Authorization(AuthorizationType.Bearer, token)
            };
            
            // Act
            await _sut.MakeApiRequest(request, CancellationToken.None);
            
            // Assert
            _restClientMock.Verify(client => client.ExecuteTaskAsync(It.Is<IRestRequest>(req => req.Parameters.FirstOrDefault(param => param.Name == HeaderNames.Authorization && param.Type == ParameterType.HttpHeader && param.Value.ToString() == $"Bearer {token}") != default), It.IsAny<CancellationToken>()), Times.Once);
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
            
            var request = new ApiRequest("resource");
            
            // Act
            await _sut.MakeApiRequest(request, CancellationToken.None);
            
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
            
            var request = new ApiRequest("resource");
            
            // Act
            await _sut.MakeApiRequest(request, CancellationToken.None);
            
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
            
            var token = $"{Guid.NewGuid()}";
            var request = new ApiRequest("resource")
            {
                Authorization = new Authorization(AuthorizationType.Bearer, token)
            };
            
            // Act & Assert
            await Should.ThrowAsync<UnauthorizedException>(() => _sut.MakeApiRequest(request, CancellationToken.None));
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
            _restClientMock.Setup(client => client.BuildUri(It.IsAny<IRestRequest>())).Returns(new Uri("https://localhost/resource"));
            
            var token = $"{Guid.NewGuid()}";
            var request = new ApiRequest("/resource")
            {
                Authorization = new Authorization(AuthorizationType.Bearer, token)
            };
            
            // Act
            var exception = await Should.ThrowAsync<RestException>(() => _sut.MakeApiRequest(request, CancellationToken.None));
            
            // Assert
            exception.Message.ShouldContain(request.Resource);
            exception.Content.ShouldNotBeNull();
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
            
            var token = $"{Guid.NewGuid()}";
            var request = new ApiRequest("resource")
            {
                Authorization = new Authorization(AuthorizationType.Bearer, token),
                Edge = "edge"
            };

            // Act
            await _sut.MakeApiRequest(request, CancellationToken.None);
            
            // Assert
            _restClientMock.Verify(client => client.ExecuteTaskAsync(It.Is<IRestRequest>(req => req.Resource.Contains(request.Resource) && req.Resource.Contains(request.Edge)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Throw_If_ApiRequest_Is_Requested_With_No_Resource()
        {
            // Act
            var exception = await Should.ThrowAsync<ArgumentNullException>(() => _sut.MakeApiRequest(new ApiRequest(null)));
            
            // Assert
            exception.ParamName.ShouldBe(nameof(RestRequest.Resource));
        }

        [Theory]
        [InlineData(true, false, DataFormat.Json)]
        [InlineData(false, true, DataFormat.Xml)]
        [InlineData(false, false, DataFormat.None)]
        public async Task Should_Use_Correct_DataFormat(bool useJson, bool useXml, DataFormat expected)
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            });

            var request = new ApiRequest("resource")
            {
                UseJson = useJson,
                UseXml = useXml
            };
            
            // Act
            await _sut.MakeApiRequest(request, CancellationToken.None);
            
            // Assert
            _restClientMock.Verify(client => client.ExecuteTaskAsync(It.Is<IRestRequest>(req => req.RequestFormat == expected), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData("dElEtE", Method.DELETE)]
        [InlineData("GeT", Method.GET)]
        [InlineData("HEAD", Method.HEAD)]
        [InlineData("options", Method.OPTIONS)]
        [InlineData("paTCh", Method.PATCH)]
        [InlineData("posT", Method.POST)]
        [InlineData("PUt", Method.PUT)]
        public async Task Should_Use_Correct_HttpMethod_Verb(string httpMethod, Method expected)
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            });

            var request = new ApiRequest("resource")
            {
                HttpMethod = httpMethod
            };
            
            // Act
            await _sut.MakeApiRequest(request, CancellationToken.None);
            
            // Assert
            _restClientMock.Verify(client => client.ExecuteTaskAsync(It.Is<IRestRequest>(req => req.Method == expected), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Throw_If_HttpMethod_Verb_Is_Not_Supported()
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            });

            var request = new ApiRequest("resource")
            {
                HttpMethod = "trace"
            };
            
            // Act
            var exception = await Should.ThrowAsync<NotSupportedException>(() => _sut.MakeApiRequest(request, CancellationToken.None));
            
            // Assert
            exception.Message.ShouldContain($"Http Method {request.HttpMethod} is not supported in {typeof(Method)}.");
        }

        [Fact]
        public async Task Should_Include_QueryParams()
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            });

            const string paramName = "name";
            const string paramValue = "value";
            var request = new ApiRequest("resource")
            {
                QueryParams = new Dictionary<string, string>
                {
                    {paramName, paramValue}
                }
            };
            
            // Act
            await _sut.MakeApiRequest(request, CancellationToken.None);
            
            // Assert
            _restClientMock.Verify(client => client.ExecuteTaskAsync(It.Is<IRestRequest>(req => req.Parameters.Any(param => param.Name == paramName && param.Value.ToString() == paramValue && param.Type == ParameterType.QueryString)), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task Should_Include_HeaderParams()
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            });

            const string paramName = "name";
            const string paramValue = "value";
            var request = new ApiRequest("resource")
            {
                Headers = new Dictionary<string, string>
                {
                    {paramName, paramValue}
                }
            };
            
            // Act
            await _sut.MakeApiRequest(request, CancellationToken.None);
            
            // Assert
            _restClientMock.Verify(client => client.ExecuteTaskAsync(It.Is<IRestRequest>(req => req.Parameters.Any(param => param.Name == paramName && param.Value.ToString() == paramValue && param.Type == ParameterType.HttpHeader)), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Make_Request_Using_GET_Verb_Ignoring_Given_Verb()
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            });

            var request = new ApiRequest("resource")
            {
                HttpMethod = HttpMethods.Post
            };
            
            // Act
            await _sut.Get(request, CancellationToken.None);
            
            // Assert
            _restClientMock.Verify(client => client.ExecuteTaskAsync(It.Is<IRestRequest>(req => req.Method == Method.GET), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task Should_Make_Request_Using_DELETE_Verb_Ignoring_Given_Verb()
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            });

            var request = new ApiRequest("resource")
            {
                HttpMethod = HttpMethods.Post
            };
            
            // Act
            await _sut.Delete(request, CancellationToken.None);
            
            // Assert
            _restClientMock.Verify(client => client.ExecuteTaskAsync(It.Is<IRestRequest>(req => req.Method == Method.DELETE), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task Should_Make_Request_Using_PATCH_Verb_Ignoring_Given_Verb()
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            });

            var request = new ApiRequest("resource")
            {
                HttpMethod = HttpMethods.Post
            };
            
            // Act
            await _sut.Patch(request, CancellationToken.None);
            
            // Assert
            _restClientMock.Verify(client => client.ExecuteTaskAsync(It.Is<IRestRequest>(req => req.Method == Method.PATCH), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task Should_Make_Request_Using_POST_Verb_Ignoring_Given_Verb()
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            });

            var request = new ApiRequest("resource")
            {
                HttpMethod = HttpMethods.Get
            };
            
            // Act
            await _sut.Post(request, CancellationToken.None);
            
            // Assert
            _restClientMock.Verify(client => client.ExecuteTaskAsync(It.Is<IRestRequest>(req => req.Method == Method.POST), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_Make_Request_Using_PUT_Verb_Ignoring_Given_Verb()
        {
            // Arrange
            _restClientMock.Setup(client => client.ExecuteTaskAsync(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            });

            var request = new ApiRequest("resource")
            {
                HttpMethod = HttpMethods.Get
            };
            
            // Act
            await _sut.Put(request, CancellationToken.None);
            
            // Assert
            _restClientMock.Verify(client => client.ExecuteTaskAsync(It.Is<IRestRequest>(req => req.Method == Method.PUT), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
            public async Task Should_Make_Request_Using_GET_Verb_Ignoring_Given_Verb_Accessing_Generic_Client()
            {
                // Arrange
                _restClientMock.Setup(client => client.ExecuteTaskAsync<TestObject>(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse<TestObject>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseStatus = ResponseStatus.Completed
                });

                var request = new ApiRequest("resource")
                {
                    HttpMethod = HttpMethods.Post
                };
                
                // Act
                await _sut.Get<TestObject>(request, CancellationToken.None);
                
                // Assert
                _restClientMock.Verify(client => client.ExecuteTaskAsync<TestObject>(It.Is<IRestRequest>(req => req.Method == Method.GET), It.IsAny<CancellationToken>()), Times.Once);
            }
            
            [Fact]
            public async Task Should_Make_Request_Using_PATCH_Verb_Ignoring_Given_Verb_Accessing_Generic_Client()
            {
                // Arrange
                _restClientMock.Setup(client => client.ExecuteTaskAsync<TestObject>(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse<TestObject>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseStatus = ResponseStatus.Completed
                });

                var request = new ApiRequest("resource")
                {
                    HttpMethod = HttpMethods.Post
                };
                
                // Act
                await _sut.Patch<TestObject>(request, CancellationToken.None);
                
                // Assert
                _restClientMock.Verify(client => client.ExecuteTaskAsync<TestObject>(It.Is<IRestRequest>(req => req.Method == Method.PATCH), It.IsAny<CancellationToken>()), Times.Once);
            }
            
            [Fact]
            public async Task Should_Make_Request_Using_POST_Verb_Ignoring_Given_Verb_Accessing_Generic_Client()
            {
                // Arrange
                _restClientMock.Setup(client => client.ExecuteTaskAsync<TestObject>(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse<TestObject>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseStatus = ResponseStatus.Completed
                });

                var request = new ApiRequest("resource")
                {
                    HttpMethod = HttpMethods.Get
                };
                
                // Act
                await _sut.Post<TestObject>(request, CancellationToken.None);
                
                // Assert
                _restClientMock.Verify(client => client.ExecuteTaskAsync<TestObject>(It.Is<IRestRequest>(req => req.Method == Method.POST), It.IsAny<CancellationToken>()), Times.Once);
            }

            [Fact]
            public async Task Should_Make_Request_Using_PUT_Verb_Ignoring_Given_Verb_Accessing_Generic_Client()
            {
                // Arrange
                _restClientMock.Setup(client => client.ExecuteTaskAsync<TestObject>(It.IsAny<IRestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse<TestObject>
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseStatus = ResponseStatus.Completed
                });

                var request = new ApiRequest("resource")
                {
                    HttpMethod = HttpMethods.Get
                };
                
                // Act
                await _sut.Put<TestObject>(request, CancellationToken.None);
                
                // Assert
                _restClientMock.Verify(client => client.ExecuteTaskAsync<TestObject>(It.Is<IRestRequest>(req => req.Method == Method.PUT), It.IsAny<CancellationToken>()), Times.Once);
            }
    }
}