using CommandLine;

public class Options
{
    [Option('i', "file1", Required = true, HelpText = "File1 file path.")]
    public string File1InputPath { get; set; }

    [Option('j', "file2", Required = true, HelpText = "File2 file path.")]
    public string File2InputPath { get; set; }

    public string OperationName => this.GetType().Name.Substring(0, this.GetType().Name.IndexOf("Options"));

    public string OperationDescription => ((VerbAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(VerbAttribute))).HelpText;
}


[Verb("GetDifferenceIndexes", HelpText = "Prints difference indexes along with line number. This operation is slow and may consume more space.")]
public class GetDifferenceIndexOption : Options
{
    [Option('o', "outpath", Required = true, HelpText = "Result Outpath")]
    public string OutPath { get; set; }
}

[Verb("GetDifferentLines", HelpText = "Prints the different lines either on console or output path.")]
public class GetDifferentLinesOption : Options
{
    [Option('o', "outpath", Required = false, HelpText = "Result Outpath")]
    public string OutPath { get; set; }

    [Option('p', "PrintLines", HelpText = "Print Difference Lines", Required = false)]
    public bool printLines { get; set; } = false;
}

[Verb("GetFilesParity", HelpText ="Tells if two files are same or not and prints number of differences.")]
public class GetFileParityOption : Options
{
    [Option('d', "PrintDiff", HelpText="Prints 5 different lines. (This is just to analyze pattern in differences)", Required = false, Default = false)]
    public bool PrintTopDiffs { get; set; }
}