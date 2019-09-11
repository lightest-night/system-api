# Lightest Night
## API

Gives the ability to make Authorized API Requests using a Rest Client. Utilizes RestSharp under the hood.

#### How To Use
##### Registration
* Asp.Net Standard/Core Dependency Injection
  * Use the provided `services.AddApiClientFactory()` method
  
* Other Containers
  * Register an instance of `ApiClientFactory` as the `IApiClientFactory` type as a Singleton

##### Usage
Generate a new instance of an Api Client using the `IApiClientFactory.Create<TClient>(string baseUrl = default)` method.

The `CoreClient` is provided to give out of the box API Request functionality. If more bespoke clients are required, use one from the following:

`MakeRequest<T>(IRestRequest request, bool authorizationProvided, bool isApiRequest, CancellationToken cancellationToken)`
Makes a REST Request using the specified parameters and returning an object containing type T

`MakeRequest(IRestRequest request, bool authorizationProvided, bool isApiRequest, CancellationToken cancellationToken)`
Makes a REST Request using the specified parameters and returning a generic object