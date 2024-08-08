using System.Net;
using Microsoft.Extensions.DependencyInjection;
using MyLab.OidcFinisher;

namespace IntegrationTests
{
    public partial class OidcFinisherBehavior 
    {
        [Fact]
        public async Task ShouldAutoAccept()
        {
            //Arrange
            var proxy = _fxt.StartWithProxy
            (
                srv =>
                {
                    srv.Configure(_optionsHandler);
                    srv.Configure<OidcFinisherOptions>(opt => opt.AutoAccept = true);
                    TuneApiClients(srv, null);
                    AddXUnit(srv);
                }).ApiClient;

            //Act
            var finishResult = await proxy.FinishAsync(TestAuthCode, TestAuthState, TestCodeVerifier);

            //Assert
            Assert.NotNull(finishResult);
            Assert.Equal(TestAccessToken, finishResult.AccessToken);
            Assert.Equal(TestRefreshToken, finishResult.RefreshToken);
            Assert.Equal(TestIdToken, finishResult.IdToken);
            Assert.Equal(TestExpiresIn, finishResult.ExpiresIn);
        }

        [Fact]
        public async Task ShouldAccept()
        {
            //Arrange
            var client = _fxt.Start
            (
                srv =>
                {
                    srv.Configure(_optionsHandler);
                    TuneApiClients(srv, "accept");
                    AddXUnit(srv);
                }).ApiClient;

            //Act
            var finishResultCallDetails = await client.Call(api => api.FinishAsync(TestAuthCode, TestAuthState, TestCodeVerifier));

            //Assert
            await finishResultCallDetails.ThrowIfUnexpectedStatusCode();
            Assert.NotNull(finishResultCallDetails.ResponseContent);
            Assert.Equal(TestAccessToken ,finishResultCallDetails.ResponseContent.AccessToken);
            Assert.Equal(TestRefreshToken, finishResultCallDetails.ResponseContent.RefreshToken);
            Assert.Equal(TestIdToken,finishResultCallDetails.ResponseContent.IdToken);
            Assert.Equal(TestExpiresIn, finishResultCallDetails.ResponseContent.ExpiresIn);

            Assert.Contains
            (
                finishResultCallDetails.ResponseMessage.Headers,
                h =>
                    h.Key == "X-Foo" &&
                    h.Value.Any(v => v == "bar") &&
                    h.Value.Count() == 1
            );
        }


        [Fact]
        public async Task ShouldReject()
        {
            //Arrange
            var client = _fxt.Start
            (
                srv =>
                {
                    srv.Configure(_optionsHandler);
                    TuneApiClients(srv, "reject");
                    AddXUnit(srv);
                }).ApiClient;

            //Act
            var finishResultCallDetails = await client.Call(api => api.FinishAsync(TestAuthCode, TestAuthState, TestCodeVerifier));

            var contentStream = await finishResultCallDetails.ResponseMessage.Content.ReadAsStreamAsync();
            contentStream.Seek(0, SeekOrigin.Begin);
            var contentReader = new StreamReader(contentStream);
            var stringContent = await contentReader.ReadToEndAsync();

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, finishResultCallDetails.StatusCode);
            Assert.Equal("test reason", stringContent);
        }
    }
}