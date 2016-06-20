using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SingleTableApp.Models
{
    public class FieldViewModel
    {
        public int Id { get; set; }

        [DisplayName("Наименование")]
        public string Name { get; set; }

        [UIHint("TypeView")]
        [DisplayName("Тип")]
        public MainObject Type { get; set; }

        [DisplayName("Значение")]
        public string Value { get; set; }

        [DisplayName("Порядок")]
        public int Weight { get; set; }
    }
}