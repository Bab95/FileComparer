
using CommandLine;
using FileComparer;
using FileComparer.Models;
using System.Net.Sockets;

public class Program
{
    public static void Main(string[] args)
    {
        Logger.LogInfo("=============Starting FileComparer==============");
        Logger.LogInfo("Developed by: A.K.");
        new FileComparerMain().AppMain(args).Wait();
        Logger.LogInfo("=============FileComparer Finished==============");
    }
}