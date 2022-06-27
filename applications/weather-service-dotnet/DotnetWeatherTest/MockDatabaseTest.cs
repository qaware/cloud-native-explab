using static DotnetWeatherTest.Utils;

namespace DotnetWeatherTest;

[TestClass]
public class MockDatabaseTest
{
    [TestMethod]
    public void DatabaseSeedTest()
    {
        var context = GetDatabaseContext();

        var cities = (from c in context.City orderby c.Name select c);
        Assert.AreEqual(5, cities.Count());
        Assert.IsTrue((from c in cities select c.Name).Contains("Munich"));
    }
}