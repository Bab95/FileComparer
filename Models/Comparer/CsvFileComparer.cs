using FileComparer;
using FileComparer.Models.Comparer;
using FileComparer.Models.Sorting;

public class CsvFileComparer : ChunkedFileComparer
{
    private readonly int chunkSize = Constants.ChunkSize;
    private string delimiter { get; set; }

    public CsvFileComparer(string filePath1, string filePath2) : base(filePath1, filePath2)
    {
        this.delimiter = ",";
    }

    public CsvFileComparer(string filePath1, string filePath2, string delimiter) : base(filePath1, filePath2)
    {
        this.delimiter = delimiter;
    }

    public CsvFileComparer(string filePath1, string filePath2, bool sort) : this(filePath1, filePath2)
    {
        this.SortingContext = new SortingContext(new CsvFileSortingStrategy(this.chunkSize, delimiter, 0, false));
    }

    public CsvFileComparer(string filePath1, string filePath2, string delimiter, bool shouldSort) : this(filePath1, filePath2, shouldSort)
    {
        this.delimiter = delimiter;
        this.SortingContext = new SortingContext(new CsvFileSortingStrategy(this.chunkSize, delimiter, 0, false));
    }

    public CsvFileComparer(string filePath1, 
        string filePath2, 
        string delimiter,
        bool shouldSort,
        bool shouldNormalize) : base(filePath1, filePath2)
    {
        this.delimiter = delimiter;
        this.SortingContext = new SortingContext(new CsvFileSortingStrategy(this.chunkSize, 
                                                    delimiter,
                                                    0, // for simplicity we are sorting based on first column, this can be extended to take sorting column number as input. 
                                                    shouldNormalize));
    }

    private void Sort()
    {
        if (this.SortingContext != null)
        {
            // NOTE: outpath change for sorted files.
            this.SortingContext.Sort(File1Path, File1Path);
            this.SortingContext.Sort(File2Path, File2Path);
        }
        else
        {
            Console.WriteLine("Sorting was not requested.");
        }
    }

    public override void Compare(object obj)
    {


        // similar to data comparer read data parse it normalize it and then comapre chunk by chunk.
        throw new NotImplementedException();
    }
}