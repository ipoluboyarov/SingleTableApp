using System.Data.Entity;

namespace SingleTableApp.Models
{
    public class MainObjectDbInit : DropCreateDatabaseAlways<MainObjectContext>
    {
        protected override void Seed(MainObjectContext db)
        {
            //db.MainObjects.Add(new MainObject { Id = 1, ParentId = 1, TypeId = 2, Value = "Root", Weight = 100, DueCode = "0." });
            //db.MainObjects.Add(new MainObject { Id = 2, ParentId = 0, TypeId = 2, Value = "text", Weight = 100 });
            //db.MainObjects.Add(new MainObject { Id = 3, ParentId = 0, TypeId = 3, Value = "number", Weight = 200 });
            
            base.Seed(db);
        }
    }
}