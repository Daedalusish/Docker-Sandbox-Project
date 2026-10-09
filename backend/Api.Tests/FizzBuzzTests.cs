public class FizzBuzzTests
{
    [Theory]
    [InlineData(1, "1")]
    [InlineData(3, "Fizz")]
    [InlineData(5, "Buzz")]
    [InlineData(15, "FizzBuzz")]
    public void Returns_the_right_word(int number, string expected) =>
        Assert.Equal(expected, FizzBuzz.For(number));
}
