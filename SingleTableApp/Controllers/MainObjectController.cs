using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using SingleTableApp.Models;


namespace SingleTableApp.Controllers
{
    public class MainObjectController : Controller
    {
        MainObjectContext db = new MainObjectContext();
        List<MainObject> typesList = new List<MainObject>();

        public ActionResult Index()
        {
            IEnumerable<MainObject> mainObjects = from m in db.MainObjects
                select m;
            ViewBag.Title = "Главная";

            return View(mainObjects);
        }

        [HttpGet]
        public ActionResult CreateLibrary()
        {
            return Redirect("/Library/Create");
        }

        public ActionResult Delete(int id)
        {
            MainObjectService mainObjectService = new MainObjectService();

            mainObjectService.DeleteMainObject(id);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public void FillDB(int parentId, int parentTypeId)
        {
            typesList = db.MainObjects.Where(t => t.Id != t.TypeId
                                                  && t.ParentId == 0)
                .OrderBy(t => t.Value)
                .ToList();

            FillDBAddObject(parentId, parentTypeId);
        }

        [HttpPost]
        public void FillDBAddObject(int parentId, int parentTypeId)
        {
            //Определяем Id типа создаваемого объекта
            MainObject typeObject = FillDBGenerateType(parentTypeId);

            for (int i = 0; i < 10; i++)
            {
                //Если есть еще что создавать на нижнем уровне, делаем рекурсию...
                if (parentTypeId != 1 && parentTypeId != 18) //сюда id Детали
                {
                    //Создаем объект
                    int id = FillDdCreateObject(parentId, typeObject);

                    FillDBAddObject(id, typeObject.Id);
                }
                else
                {
                    FillDdCreateObject(parentId, typeObject);
                }

                //Создаем только два объекта 1го уровня(с подчиненными объектами) 
                if (parentTypeId == 0 && i == 1)
                {
                    break;
                }
            }
        }

        private int FillDdCreateObject(int parentId, MainObject typeObject)
        {
            ObjectController objectController = new ObjectController();
            JavaScriptSerializer jss = new JavaScriptSerializer();
            Random rand = new Random();

            //получаем атрибуты создаваемого объекта
            var fieldsJson = objectController.Fields(typeObject.Id);
            dynamic fields = fieldsJson.Data;

            int objectCount = db.MainObjects.Count();

            //формируем лист ключ-значение полей(ид атрибута,значение поля) 
            List<KeyValuePair<int, string>> kvPairs = new List<KeyValuePair<int, string>>();

            foreach (var field in fields)
            {   
                kvPairs.Add(new KeyValuePair<int, string>(field.Id,
                    (field.Type.Value == "number")
                        ? (Convert.ToDouble(rand.Next(10000)) / 100).ToString().Replace(",", ".")
                        : field.Name + (objectCount + 1)));
            }

            //создаем объект
            dynamic idJson = objectController.AddNewObject(typeObject.Value + (objectCount + 1), jss.Serialize(kvPairs),
                typeObject.Id, parentId, true);
            var id = idJson.Data.objectId;

            return id;
        }

        private MainObject FillDBGenerateType(int parentTypeId)
        {
            int typeId = 0;

            switch (parentTypeId)
            {
                case 0:
                    typeId = 4; //Комплекс
                    break;

                case 4: //Если это Комплекс 
                    typeId = 9; //Узел
                    break;

                case 9: //Если это Узел
                    typeId = 13; //Установка
                    break;

                case 13: //Если это Установка
                    typeId = 18; //Оборудование
                    break;

                case 18: //Если это Оборудование
                    typeId = 22; //Деталь
                    break;

                case 22: //Если это Деталь
                    typeId = 22; //Деталь
                    break;

                default:
                    typeId = 1;
                    break;
            }

            return typesList.SingleOrDefault(x => x.Id == typeId);
        }
    }
}