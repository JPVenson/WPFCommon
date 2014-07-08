namespace DataAccess.Helper
{
    public class QueryParameter : IQueryParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}