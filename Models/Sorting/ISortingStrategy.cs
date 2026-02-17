namespace FileComparer.Models.Sorting
{
    public interface ISortingStrategy
    {
        void Sort(string inputFilePath, string outputFilePath);
    }
}
