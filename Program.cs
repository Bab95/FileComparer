
using CommandLine;
using FileComparer;
using FileComparer.Models;
using System.Net.Sockets;

public class Program
{
    public static void Main(string[] args)
    {
        //string file1 = @"C:\Users\Babnish\Downloads\large\bible-test1";
        //string file2 = @"C:\Users\Babnish\Downloads\large\bible-test2";
        //new SequentialFileComparer(file1, file2).Compare();
        new FileComparerMain().AppMain(args).Wait();
    }
}