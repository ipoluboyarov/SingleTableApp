using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SingleTableApp.Models;

namespace SingleTableApp.Controllers
{
    public class LibraryController : Controller
    {
        public ActionResult Create()
        {
            //Отбираем и записываем информацию о типах и дефолтном значении для kendo Grid.
            MainObjectContext db = new MainObjectContext();

            var types = from m in db.MainObjects
                where m.Id == m.TypeId
                orderby m.Value
                select m;

            ViewData["types"] = types;
            ViewData["defaultType"] = types.First();
            ViewBag.Title = "Создание справочника";

            return View();
        }

        [HttpPost]
        public JsonResult AddNewLibrary(string fields, string libraryName)
        {
            //TODO: обобщить с ObjectController.AddNewObject. Вынести в базовый класс
            MainObjectService ObjectService = new MainObjectService();

            string errorMsg = "";

            try
            {
                var fieldsJson = JObject.Parse(fields).SelectToken("myFields").ToString();
                var fieldsList = JsonConvert.DeserializeObject<List<FieldViewModel>>(fieldsJson);
                int idLib;

                //проверяем имя и поля справочника
                if (libraryName.HasValue() && VerifyFields(fieldsList)) //проверка успешно пройдена
                {
                    //Создаем и записываем справочник
                    MainObject Lib = new MainObject
                    {
                        ParentId = 0,
                        TypeId = 2,
                        Value = libraryName
                    };

                    idLib = ObjectService.CreateMainObject(ref Lib);

                    //Создаем и записываем поля справочника 
                    if (idLib > 0) //Если справочник создан
                    {
                        List<MainObject> listfields = new List<MainObject>();

                        foreach (var f in fieldsList)
                        {
                            MainObject m = new MainObject
                            {
                                ParentId = idLib,
                                Weight = f.Weight*100,
                                TypeId = f.Type.Id,
                                Value = f.Name
                            };

                            listfields.Add(m);
                        }
                        ObjectService.CreateMainObjectRange(listfields);
                    }
                    else
                    {
                        errorMsg = String.Format("Не удалось добавить новый справочник {0} в базу", libraryName);
                    }
                }
                else //Иначе формируем сообщение об ошибке
                {
                    errorMsg = "Ошибка 1: Неверно заполнены данные справочника";
                }
            }
            catch (Exception e)
            {
                errorMsg = String.Format("Не удалось добавить справочник {0} или его реквизиты в базу", libraryName);
                //TODO: Добавить в лог информацию по ошибке
            }

            return Json(new
            {
                errorMsg = errorMsg,
                redirectUrl = Url.Action("Index", "MainObject")
            });
        }

        private bool VerifyFields(List<FieldViewModel> fields)
        {
            //Различные проверки для полей справочника
            //Можно реализовать проверку заполнения порядка полей. 
            //(а можно не проверять, а использовать одинаковое значение порядка для горизонтального отображения)
            return !fields.Any(x => x.Name.HasValue() == false);
        }
    }
}