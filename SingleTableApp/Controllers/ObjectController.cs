using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using SingleTableApp.Models;

namespace SingleTableApp.Controllers
{
    public class ObjectController : Controller
    {
        private readonly MainObjectContext db = new MainObjectContext();


        public ActionResult Index()
        {
            var types = db.MainObjects.Where(t => t.Id != t.TypeId
                                                  && t.ParentId == 0)
                .OrderBy(t => t.Value)
                .ToList();

            TempData["types"] = types; //классы объекта для DropDownList
            ViewBag.Title = "Дерево объектов";
            return View();
        }

        public ActionResult Edit(string due)
        {
            var items = from data in db.MainObjects
                join lib in db.MainObjects on data.TypeId equals lib.Id
                join type in db.MainObjects on lib.TypeId equals type.Id
                where data.DueCode == due
                orderby lib.Weight
                select
                    new FieldViewModel
                    {
                        Id = data.Id,
                        Name = lib.Value,
                        Weight = lib.Weight,
                        Type = type,
                        Value = data.Value
                    };

            return PartialView(items.ToList());
        }

        [HttpPost]
        public ActionResult Edit(IList<FieldViewModel> fields)
        {
            var errorMsg = "";

            try
            {
                var ObjectService = new MainObjectService();

                ObjectService.EditObjects(fields.ToDictionary(x => x.Id, x => x.Value));
            }
            catch (Exception e)
            {
                //TODO: Добавить в лог информацию по ошибке и объекте
                return View("Error");
            }

            return View("Index");
        }

        public ActionResult Create(int id, bool isChildren)
        {
            var tmpData = TempData["types"];
            TempData["types"] = tmpData; //Записываем типы обратно...

            ViewData["types"] = tmpData;
            ViewData["parentId"] = id; //id объекта на котором нажата кнопка создания нового объекта
            ViewData["isChildren"] = isChildren; //создается подчиненный объект или на позиции

            return PartialView();
        }

        public JsonResult KendoTree_GetObjects(int? id)
        {
            var objects = from obj in db.MainObjects
                join lib in db.MainObjects on obj.TypeId equals lib.Id
                where lib.ParentId == 0 && lib.TypeId != lib.Id && obj.ParentId == (id ?? 1)
                orderby obj.Weight
                select new {id = obj.Id, value = obj.Value, due = obj.DueCode, hasChildren = obj.HasChildrens};

            return Json(objects, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Fields(int Id)
        {
            var listfields = new List<FieldViewModel>();

            var fields = db.MainObjects.Where(f => f.ParentId == Id).OrderBy(f => f.Weight).ToList();

            var types = db.MainObjects.Where(x => x.Id == x.TypeId).ToList();

            foreach (var f in fields)
            {
                var field = new FieldViewModel
                {
                    Id = f.Id,
                    Name = f.Value,
                    Weight = f.Weight
                };

                field.Type = types.FirstOrDefault(x => x.Id == f.TypeId);

                listfields.Add(field);
            }

            return Json(listfields, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddNewObject(string objectName, string fields, int libraryId, int parentId, bool isChildren)
        {
            //TODO: Обобщить с LibraryController.AddNewLibrary. Вынести в базовый класс
            var errorMsg = "";
            var objectId = 0;

            try
            {
                var ObjectService = new MainObjectService();

                //Создаем и записываем объект в базу
                var obj = new MainObject
                {
                    TypeId = libraryId,
                    Value = objectName
                };

                obj = ObjectService.CreateObject(obj, parentId, isChildren);

                var dueCode = obj.DueCode;
                objectId = obj.Id;
                if (objectId > 0)
                {
                    var mainObjects = new List<MainObject>();

                    //десериализуем реквизиты объекта
                    var fieldsList = JsonConvert.DeserializeObject<IEnumerable<KeyValuePair<int, string>>>(fields);

                    //Создаем и добавляем реквизиты объекта в list
                    foreach (var field in fieldsList)
                    {
                        var fld = new MainObject
                        {
                            ParentId = objectId,
                            TypeId = field.Key,
                            Value = field.Value,
                            DueCode = dueCode
                            //При необходимости реализовать возможность изменения order объектов
                        };
                        mainObjects.Add(fld);
                    }

                    //Записываем реквизиты в базу
                    ObjectService.CreateMainObjectRange(mainObjects);
                }
                else
                {
                    errorMsg = string.Format("Не удалось добавить новый объект {0} в базу", objectName);
                }
            }
            catch (Exception e)
            {
                errorMsg = string.Format("Не удалось добавить объект {0} или его реквизиты в базу", objectName);
                //TODO: Добавить в лог информацию по ошибке
            }

            return Json(new {errorMsg, objectId});
        }

        [HttpPost]
        public JsonResult DeleteObject(int id)
        {
            var errorMsg = "";

            try
            {
                var ObjectService = new MainObjectService();

                ObjectService.DeleteObject(id);
            }
            catch (Exception e)
            {
                errorMsg = string.Format("Не удалось удалить объект ID: {0}", id);
                //TODO: Добавить в лог информацию по ошибке
            }

            return Json(new {errorMsg});
        }
    }
}