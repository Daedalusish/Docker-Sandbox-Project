using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

// Starts the API in memory and calls it over HTTP, against the real database.
public class ApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Returns_the_sequence()
    {
        string[] expected = ["1", "2", "Fizz", "4", "Buzz"];

        var words = await factory.CreateClient().GetFromJsonAsync<string[]>("/fizzbuzz/5");

        Assert.Equal(expected, words);
    }

    [Fact]
    public async Task Rejects_counts_out_of_range()
    {
        var response = await factory.CreateClient().GetAsync("/fizzbuzz/0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Remembers_what_it_was_asked()
    {
        var client = factory.CreateClient();
        // Random, so an entry left over from an earlier run can't make the test pass
        var count = Random.Shared.Next(1, 101);

        await client.GetAsync($"/fizzbuzz/{count}");
        var history = await client.GetFromJsonAsync<int[]>("/history") ?? [];

        Assert.Contains(count, history);
    }
}
