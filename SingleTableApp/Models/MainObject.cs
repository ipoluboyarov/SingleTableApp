namespace SingleTableApp.Models
{
    public class MainObject
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public int TypeId { get; set; }
        public string Value { get; set; }
        public int Weight { get; set; }
        public bool HasChildrens { get; set; }
        public string DueCode { get; set; }
    }
}
