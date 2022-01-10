namespace Smallprogram.BlazorWasmClient.Service
{


    public class CallApiService
    {
        private readonly IHttpClientFactory httpClientFactory;

        public CallApiService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }
        public async Task<string> CallApi()
        {

            using var client = httpClientFactory.CreateClient("Smallprogram.Api1");

            using var request = new HttpRequestMessage(HttpMethod.Get, "api/secret");

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }


}
