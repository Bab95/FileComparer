using CommandLine;
using CommandLine.Text;

public abstract class CompareOptionsBase
{
    [Option('i', "file1", Required = true, HelpText = "File1 path")]
    public string File1InputPath { get; set; }

    [Option('j', "file2", Required = true, HelpText = "File2 path")]
    public string File2InputPath { get; set; }

    [Option('o', "outpath", Required = false, HelpText = "Output path")]
    public string OutPath { get; set; }

    [Option('n', "PrintNoOfDiff", Required = false, HelpText = "Number of differences to print")]
    public int PrintNoOfDiffs { get; set; } = 5;

    public string OperationName =>
        this.GetType().Name.Replace("Options", "");

    public string? OperationDescription =>
        (Attribute.GetCustomAttribute(
            this.GetType(),
            typeof(VerbAttribute)) as VerbAttribute)?.HelpText;
}

public abstract class DataCompareOptionsBase : CompareOptionsBase
{
    [Option('s', "sort", Required = false, HelpText = "Sort before compare")]
    public bool Sort { get; set; }

    [Option('n', "NoOfDifferences", Required = false, HelpText = "Number of differences to print")]
    public int NoOfDifferences { get; set; }
}

public abstract class CsvCompareOptionsBase : DataCompareOptionsBase
{
    [Option("delimiter", Required = false, Default = ",",
        HelpText = "CSV delimiter")]
    public string Delimiter { get; set; } = ",";

    [Option("normalize", Required = false, HelpText = "Normalize before compare")]
    public bool Normalize { get; set; } = false;

    [Option("sortColumn", Required = false, Default = 1,
        HelpText = "CSV sort column index (1-based)")]
    public int SortColumn { get; set; }

    [Option("ignoreHeader", Required = false,
        HelpText = "Ignore header row")]
    public bool IgnoreHeader { get; set; }
}

public abstract class ExcelCompareOptionsBase : DataCompareOptionsBase
{
    [Option("sheet", Required = false,
        HelpText = "Sheet name")]
    public string? SheetName { get; set; }

    [Option("sheetIndex", Required = false,
        HelpText = "Sheet index")]
    public int SheetIndex { get; set; }
}

public abstract class PdfCompareOptionsBase : CompareOptionsBase
{
    [Option("mode", Required = false, Default = "text",
        HelpText = "Comparison mode: text|visual|metadata")]
    public string Mode { get; set; }

    [Option("ignoreWhitespace", Required = false)]
    public bool IgnoreWhitespace { get; set; }
}

[Verb("CompareData", HelpText = "Compare Data(Raw text) files")]
public class CompareDataFileOptions : DataCompareOptionsBase
{
}

[Verb("CompareCsv", HelpText = "Compare CSV files")]
public class CompareCsvOptions : CsvCompareOptionsBase
{
}

[Verb("CompareExcel", HelpText = "Compare Excel files (Not Implemented)")]
public class CompareExcelOptions : ExcelCompareOptionsBase
{
}

[Verb("ComparePdf", HelpText = "Compare PDF files (Not Implemented)")]
public class ComparePdfOptions : PdfCompareOptionsBase
{
}
