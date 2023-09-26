
using CommandLine;
using FileComparer;
using FileComparer.Models;
using System.Net.Sockets;

public class Program
{
    public static void Main(string[] args)
    {
        new FileComparerMain().AppMain(args).Wait();
    }
}