using ProcessingLargeAmountOfData;

class Program
{
    public static void Main()
    {
        // Починається робота програми
        var schoolsearch = new SchoolSearch("list.txt", "teachers.txt");

        schoolsearch.Run();
    }
}