using Microsoft.AspNetCore.Mvc.Testing;

namespace MiniApiTest;

[TestClass]
public class UnitTest1
{
    //Need to reference JwtAuthApi project to get "program" class
    private static WebApplicationFactory<Program> app;

    [ClassInitialize]
    public static void Init(TestContext textContext)
    {
        app = new WebApplicationFactory<Program>();
    }

    [TestMethod]
    public async Task Get_Home_ReturnHelloWorld_IndividualSetting()
    {
        await using var application = new WebApplicationFactory<Program>();
        using var client = application.CreateClient();

        var response = await client.GetStringAsync("/");

        Assert.AreEqual("Hello World!", response);
    }
    [TestMethod]
    public async Task Get_Home_ReturnHelloWorld()
    {
        var client = app.CreateClient();
        var response = await client.GetStringAsync("/");

        Assert.AreEqual("Hello World!", response);
    }
    
    [TestMethod]
    public async Task Get_Guid_ReturnGuid()
    {
        var client = app.CreateClient();
        var resp = await client.GetStringAsync("/guid");
        Assert.IsTrue(resp.StartsWith("SUT:"));
        //Assert.IsTrue(Guid.TryParse(resp.Substring(5), out _));
    }
    
}
