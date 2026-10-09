public static class FizzBuzz
{
    public static string For(int number) => (number % 3, number % 5) switch
    {
        (0, 0) => "FizzBuzz",
        (0, _) => "Fizz",
        (_, 0) => "Buzz",
        _ => number.ToString(),
    };

    public static IEnumerable<string> Range(int count) =>
        Enumerable.Range(1, count).Select(For);
}
