using System.Net;
using System.Text.RegularExpressions;
using DotnetWeather;

namespace DotnetHello.Test;

[TestClass]
public class UnitTest
{
    [TestMethod]
    public async Task TestSum()
    {
        var webAppFactory = new WebApplicationFactory<Program>();
        var httpClient = webAppFactory.CreateDefaultClient();

        var response = await httpClient.GetAsync("sum?n1=0&n2=111");
        var stringResult = await response.Content.ReadAsStringAsync();
        
        Assert.AreEqual(111, Int32.Parse(stringResult));
    }

    [TestMethod]
    public async Task TestRedirect()
    {
        var webAppFactory = new WebApplicationFactory<Program>();
        var httpClient = webAppFactory.CreateDefaultClient();

        var response = await httpClient.GetAsync("");

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);

        var redirectedUrl = response.Headers.Location;
        var newResponse = await httpClient.GetAsync(redirectedUrl);
        var result = await newResponse.Content.ReadAsStringAsync();

        Regex regex = new Regex(@"\<title\>.*\</title\>", RegexOptions.IgnoreCase);
        Match match = regex.Match(result);

        Assert.AreEqual("<title>Hello World</title>", match.Value);
    }
}