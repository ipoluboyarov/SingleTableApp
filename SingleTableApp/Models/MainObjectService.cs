using System;
using System.Collections.Generic;
using System.Linq;
using EntityFramework.Extensions;

namespace SingleTableApp.Models
{
    public class MainObjectService
    {
        MainObjectContext db = new MainObjectContext();

        public int CreateMainObject(ref MainObject obj)
        {
            db.MainObjects.Add(obj);
            db.SaveChanges();
            return obj.Id;
        }

        public void CreateMainObjectRange(IEnumerable<MainObject> objects)
        {
            db.MainObjects.AddRange(objects);

            db.SaveChanges();
        }

        public void DeleteMainObject(int id)
        {
            var deletedItem = db.MainObjects.Delete(o => o.Id == id);
        }

        public void DeleteObject(int id)
        {
            MainObject obj = db.MainObjects.SingleOrDefault(x => x.Id == id);

            if (obj.DueCode.Length > 2)
            {
                var deletedItem = db.MainObjects.Delete(o => o.DueCode.Contains(obj.DueCode));

                //если это был единственный объект, то родительскому меняем флаг
                var parentObj = db.MainObjects.SingleOrDefault(x => x.Id == obj.ParentId);

                if (!db.MainObjects.Any(x => x.ParentId == parentObj.Id && x.DueCode != parentObj.DueCode))
                    //можно обойтись что содержит, но не равен DueCode, но лучше проверять и id
                {
                    parentObj.HasChildrens = false;
                    db.SaveChanges();
                }
            }
        }


        public void EditObjects(Dictionary<int, string> values)
        {
            //TODO: Использовать LINQ 
            foreach (var value in values)
            {
                var obj = db.MainObjects.SingleOrDefault(x => x.Id == value.Key);
                obj.Value = value.Value;
            }
            db.SaveChanges();
        }


        public MainObject CreateObject(MainObject obj, int parentId, bool isChildren)
        {
            // получаем родительский(или на позиции которого создается новый) объект
            MainObject parentObject = db.MainObjects.Single(x => x.Id == parentId);

            //записываем позицию нового объекта
            obj.Weight = GeneratePosition(parentObject, isChildren);

            //получаем родительский элемент
            parentObject = (isChildren)
                ? parentObject
                : db.MainObjects.SingleOrDefault(x => x.Id == parentObject.ParentId);

            //записываем id родительского объекта
            obj.ParentId = parentObject.Id;

            //добавляем объект в БД
            this.CreateMainObject(ref obj);

            //записываем DUE код нового объекта
            obj.DueCode = String.Format("{0}{1}.", parentObject.DueCode, obj.Id.ToString("X").ToUpper());

            //устанавливаем родительскому флаг наличия детей
            parentObject.HasChildrens = true;

            //модифицируем объекты БД
            db.MainObjects.Attach(obj);
            db.Entry(obj).Property(p => p.DueCode).IsModified = true;

            db.MainObjects.Attach(parentObject);
            db.Entry(parentObject).Property(p => p.HasChildrens).IsModified = true;

            db.SaveChanges();

            return obj;
        }

        private int GeneratePosition(MainObject parentObject, bool isChildren)
        {
            //Функция возвращает позицию для нового объекта.
            // Если создается подчиненный, то создается позиция в конце списка
            // Если новый элемент вставляется в позицию, то расчитываем для него середину между предыдущим и текущим объектом
            //      В качестве увеличения количества позиций можно использовать дробные числа в order
            //      Для приведения поля ордер в нормальный вид, возможно создать "периодическое" задание для установки значений 100,200,300...n+100

            //Возможно еще сделать различными вариантами, например односвязным списком(каждый объект хранит в поле order, id предыдущего)
            // + решается проблема с ограниченностью объектов в списке. 
            // - проигрываем в более ресурсоемкой сортировке объектов

            //Возможно присвоение позиции вынести в триггер БД

            //TODO: Протестировать и обработать ситуации с нулевыми элементами
            int returnVal;

            if (isChildren)
            {
                var maxOrder = db.MainObjects.Where(m => m.ParentId == parentObject.Id).Max(x => x.Weight);

                returnVal = (int) (maxOrder/100)*100 + 100;
            }
            else
            {
                var prevObjects =
                    db.MainObjects.Where(m => m.ParentId == parentObject.ParentId && m.Weight < parentObject.Weight);

                var prevOrder = (prevObjects.Any()) ? prevObjects.Max(x => x.Weight) : 0;

                returnVal = (int) (prevOrder + (parentObject.Weight - prevOrder)/2);

                //если по какой-то причине не получилось вычислить ордер, то вставляем элемент в начало(в 100 позицию)
                returnVal = (returnVal == 0) ? 100 : returnVal;
            }

            return returnVal;
        }
    }
}