namespace FileComparer.Models.Sorting
{
    public class SortingContext
    {
        private readonly ISortingStrategy sortingStrategy;

        public SortingContext(ISortingStrategy sortingStrategy)
        {
            this.sortingStrategy = sortingStrategy;
        }

        public void Sort(string inputFilePath, string outputFilePath)
        {
            sortingStrategy.Sort(inputFilePath, outputFilePath);
        }
    }
}
