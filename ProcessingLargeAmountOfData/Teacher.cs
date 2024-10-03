namespace ProcessingLargeAmountOfData
{
    public class Teacher : Person
    {
        public int Classroom {  get; private set; }

        public Teacher(string lastName, string firstName, int classroom) : base(lastName, firstName) 
        {
            Classroom = classroom;
        }

        public Teacher(string[] fields) : base(fields[0], fields[1])
        {
            Classroom = int.Parse(fields[2]);
        }
    }
}