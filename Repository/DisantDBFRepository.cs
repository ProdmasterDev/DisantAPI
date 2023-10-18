using System.Collections;
using System.Data;
using System.Reflection;

namespace DisantAPI.Repository
{
    public class DisantDBFRepository
    {
        private readonly DBF.DBF db;
        public DisantDBFRepository(string connectionString)
        {
            db = new DBF.DBF(connectionString);
        }
        private IList GetObjectList(Type type, IEnumerable<DataRow> queryResult)
        {
            Type genericListType = typeof(List<>);
            Type concreteListType = genericListType.MakeGenericType(type);
            IList list = Activator.CreateInstance(concreteListType) as IList;
            foreach (var row in queryResult)
            {
                if (row == null) continue;
                var obj = Activator.CreateInstance(type);
                foreach (var prop in type.GetProperties())
                {
                    if (!row.Table.Columns.Contains(prop.Name)) continue;
                    var curVal = row[prop.Name];
                    if (curVal == null) continue;
                    var propValue = curVal.ToString()!.Trim();
                    switch (Type.GetTypeCode(prop.PropertyType))
                    {
                        case TypeCode.String:
                            prop.SetValue(obj, propValue, null);
                            break;
                        default:
                            try
                            {
                                var typeVal = prop.GetValue(obj);
                                var typeOfProp = prop.PropertyType;
                                var tryParseMethod = typeOfProp.GetMethod("TryParse",
                                    BindingFlags.Static | BindingFlags.Public,
                                    null,
                                    new Type[] { typeof(string), typeOfProp.MakeByRefType() },
                                    null);
                                object[] objects = { curVal.ToString()!, null };
                                if (tryParseMethod != null)
                                {
                                    tryParseMethod?.Invoke(null, objects);
                                    prop.SetValue(obj, objects[1], null);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            break;
                    }
                }
                list.Add(obj);
            }
            return list;
        }
        public async Task<DataTable> Execute(string query)
        {
            return await db.Execute(query);
        }
    }
}
