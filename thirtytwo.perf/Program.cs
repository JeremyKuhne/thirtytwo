namespace thirtytwo.perf;

/// <summary>
///  Runs the selected benchmarks.
/// </summary>
internal static class Program
{
    private static void Main(string[] args)
    {
        BenchmarkDotNet.Running.BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}