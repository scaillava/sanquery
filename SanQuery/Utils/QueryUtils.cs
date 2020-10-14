using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Newtonsoft.Json.Linq;
using SanQuery.Models;
using SanQuery.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace SanQuery.Utils
{
    public static class QueryUtils
    {

        public static async Task<List<dynamic>> ConsultGenerator(RequestModel requestModel)
        {
            try
            {
                List<dynamic> dynamics = new List<dynamic>();
                var queryModels = await DAL.getQueryModels();
                var queryModel = queryModels.Find(x => x.modelName == requestModel.model);

                if (queryModel != null)
                {
                    List<PropertyInfo> propertyInfos = queryModel.modelType.GetProperties().ToList();
                    string columns = await GetColumns(queryModel.modelType, requestModel.format);
                    SqlCommand sql = new SqlCommand();
                    SqlConnection con = new SqlConnection();
                    SqlDataAdapter da = new SqlDataAdapter();
                    DataTable dt = new DataTable();
                    try
                    {
                        con.ConnectionString = @"Server=localhost\SQLEXPRESS;Database=drivers;Trusted_Connection=True";
                        con.Open();
                        sql.Connection = con;
                        sql.CommandType = CommandType.Text;
                        sql.CommandText = "SELECT " + columns + " FROM " + queryModel.queryFrom;
                        await GetWhere(sql, queryModel.modelType, Convert.ToString(requestModel.filter));
                        //await GetWhere(sql, queryModel.modelType, requestModel.filter);
                        da.SelectCommand = sql;

                        da.Fill(dt);
                        JArray jArray = new JArray();
                        foreach (DataRow dr in dt.Rows)
                        {
                            dynamics.Add(await GetDynamicValues(dr, queryModel.modelType, requestModel.format));
                        }

                        List<PropertyInfo> notGroupedProp = new List<PropertyInfo>();
                        List<PropertyInfo> groupedProp = new List<PropertyInfo>();
                        foreach (var prop in propertyInfos)
                        {
                            if ((requestModel.format == null) || requestModel.format.Contains(prop.Name))
                            {
                                if (!prop.PropertyType.IsGenericType)
                                {
                                    notGroupedProp.Add(prop);
                                }
                                else
                                {
                                    groupedProp.Add(prop);
                                }
                            }

                        }
                        
                        dynamics = await GetValuesGrupped(dynamics, notGroupedProp, groupedProp);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        dt.Dispose();
                        da.Dispose();
                        sql.Dispose();
                        con.Dispose();
                    }
                }

                return await Task.FromResult(dynamics);

                //var q = from t in Assembly.GetExecutingAssembly().GetTypes()
                //        where t.IsClass && t.Namespace == nspace
                //        select t;
                //q.ToList().ForEach(t => Console.WriteLine(t.Name));
                //string query = "select "++ from { 1}"
                //    string.Format
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        private static async Task<List<dynamic>> GetValuesGrupped(List<dynamic> dynamics, List<PropertyInfo> notGroupedProp, List<PropertyInfo> groupedProp)
        {
            try
            {
                Dictionary<dynamic, dynamic> dictionary = new Dictionary<dynamic, dynamic>();
                foreach (var dyn in dynamics)
                {
                    dynamic extendObject = new ExpandoObject();
                    dynamic extendObjectLists = new ExpandoObject();
                    foreach (var ngp in notGroupedProp)
                    {
                        ((IDictionary<string, object>)extendObject)[ngp.Name] = ((IDictionary<string, object>)dyn)[ngp.Name];
                        ((IDictionary<string, object>)extendObjectLists)[ngp.Name] = ((IDictionary<string, object>)dyn)[ngp.Name];
                    }

                    dynamic exist = null;
                    foreach (var dkey in dictionary.Keys)
                    {
                        bool partialExist = true;
                        foreach (PropertyInfo prop in notGroupedProp)
                        {
                            if (!((((IDictionary<string, object>)dkey)[prop.Name]).Equals(((IDictionary<string, object>)dyn)[prop.Name])))
                                partialExist = false;
                        }
                        if (partialExist)
                            exist = dkey;
                    }
                    if (exist != null)
                    {

                        foreach (var ngp in groupedProp)
                        {
                            if (!((IDictionary<string, object>)dyn).ContainsKey(ngp.Name))
                            {
                                ((IDictionary<string, object>)dictionary[exist])[ngp.Name] = new List<dynamic>();
                            }
                                ((List<dynamic>)((IDictionary<string, object>)dictionary[exist])[ngp.Name]).Add(((IDictionary<string, object>)dyn)[ngp.Name]);
                        }

                    }
                    else
                    {

                        foreach (var ngp in groupedProp)
                        {
                            if (!((IDictionary<string, object>)extendObjectLists).ContainsKey(ngp.Name))
                            {
                                ((IDictionary<string, object>)extendObjectLists)[ngp.Name] = new List<dynamic>();
                            }
                                ((List<dynamic>)((IDictionary<string, object>)extendObjectLists)[ngp.Name]).Add(((IDictionary<string, object>)dyn)[ngp.Name]);
                        }
                        dictionary.Add(extendObject, extendObjectLists);
                    }


                }

                return await Task.FromResult(dictionary.Values.ToList());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

//        if(value is IList && value.GetType().IsGenericType) {
//}
        private static async Task<JObject> GetValues(DataRow dr, Type type, string[] format)
        {
            try
            {
                JObject jObject = new JObject();
                foreach (PropertyInfo prop in type.GetProperties())
                {
                    if ((format == null) || format.Contains(prop.Name))
                    {

                        if (prop.PropertyType.IsGenericType)
                        {
                            jObject[prop.Name] = await GetValues(dr, prop.PropertyType.GetGenericArguments()[0], null);
                        }
                        else
                        {
                            if (prop.PropertyType == typeof(int))
                                jObject[prop.Name] = int.Parse(dr[prop.Name].ToString());
                            else if (prop.PropertyType == typeof(string))
                                jObject[prop.Name] = dr[prop.Name].ToString();
                            else if (prop.PropertyType == typeof(DateTime))
                                jObject[prop.Name] = int.Parse(dr[prop.Name].ToString());
                            else if (prop.PropertyType == typeof(float))
                                jObject[prop.Name] = float.Parse(dr[prop.Name].ToString());
                            else if (prop.PropertyType == typeof(decimal))
                                jObject[prop.Name] = decimal.Parse(dr[prop.Name].ToString());
                            else
                                jObject[prop.Name] = await GetValues(dr, prop.PropertyType, null);
                        }
                    }
                }
                return await Task.FromResult(jObject);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        private static async Task<dynamic> GetDynamicValues(DataRow dr, Type type, string[] format)
        {
            try
            {
                dynamic expando = new ExpandoObject();
                foreach (PropertyInfo prop in type.GetProperties())
                {
                    if ((format == null) || format.Contains(prop.Name))
                    {

                        if (prop.PropertyType.IsGenericType)
                        {
                            ((IDictionary<string, object>)expando)[prop.Name] = await GetDynamicValues(dr, prop.PropertyType.GetGenericArguments()[0], null);
                        }
                        else
                        {
                            if (prop.PropertyType == typeof(int))
                                ((IDictionary<string, object>)expando)[prop.Name] = int.Parse(dr[prop.Name].ToString());
                            else if (prop.PropertyType == typeof(string))
                                ((IDictionary<string, object>)expando)[prop.Name] = dr[prop.Name].ToString();
                            else if (prop.PropertyType == typeof(DateTime))
                                ((IDictionary<string, object>)expando)[prop.Name] = DateTime.Parse(dr[prop.Name].ToString());
                            else if (prop.PropertyType == typeof(float))
                                ((IDictionary<string, object>)expando)[prop.Name] = float.Parse(dr[prop.Name].ToString());
                            else if (prop.PropertyType == typeof(decimal))
                                ((IDictionary<string, object>)expando)[prop.Name] = decimal.Parse(dr[prop.Name].ToString());
                            else
                                ((IDictionary<string, object>)expando)[prop.Name] = await GetDynamicValues(dr, prop.PropertyType, null);
                        }
                    }
                }
                return await Task.FromResult(expando);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private static async Task GetWhere(SqlCommand sql, Type type, string filterString)
        {
            try
            {

                if (filterString != null)
                {
                    JObject filter = JObject.Parse(filterString);
                    sql.CommandText += " WHERE ";
                    bool and = false;
                    foreach (var prop in type.GetProperties())
                    {
                        if (filter.ContainsKey(prop.Name))
                        {
                            //SqlParameter sqlParameter = new SqlParameter("@"+prop.Name,);
                            if (filter[prop.Name]["eq"] != null)
                            {
                                if (and)
                                    sql.CommandText += " and ";
                                else
                                    and = true;
                                string parameterName = "@" + prop.Name + "eq";
                                sql.CommandText += (await GetDBSchema(type, prop.Name)) + "." + prop.Name + " = " + parameterName;
                                await AddParameter(sql, parameterName, filter[prop.Name]["eq"]);

                            }
                            if (filter[prop.Name]["min"] != null)
                            {
                                if (and)
                                    sql.CommandText += " and ";
                                else
                                    and = true;
                                string parameterName = "@" + prop.Name + "min";
                                sql.CommandText += (await GetDBSchema(type, prop.Name)) + "." + prop.Name + " > " + parameterName;
                                await AddParameter(sql, parameterName, filter[prop.Name]["min"]);

                            }
                            if (filter[prop.Name]["mineq"] != null)
                            {
                                if (and)
                                    sql.CommandText += " and ";
                                else
                                    and = true;
                                string parameterName = "@" + prop.Name + "mineq";
                                sql.CommandText += (await GetDBSchema(type, prop.Name)) + "." + prop.Name + " >= " + parameterName;
                                await AddParameter(sql, parameterName, filter[prop.Name]["mineq"]);

                            }
                            if (filter[prop.Name]["max"] != null)
                            {
                                if (and)
                                    sql.CommandText += " and ";
                                else
                                    and = true;
                                string parameterName = "@" + prop.Name + "max";
                                sql.CommandText += (await GetDBSchema(type, prop.Name)) + "." + prop.Name + " < " + parameterName;
                                await AddParameter(sql, parameterName, filter[prop.Name]["max"]);

                            }
                            if (filter[prop.Name]["maxeq"] != null)
                            {
                                if (and)
                                    sql.CommandText += " and ";
                                else
                                    and = true;
                                string parameterName = "@" + prop.Name + "maxeq";
                                sql.CommandText += (await GetDBSchema(type, prop.Name)) + "." + prop.Name + " <= " + parameterName;
                                await AddParameter(sql, parameterName, filter[prop.Name]["maxeq"]);

                            }
                            if (filter[prop.Name]["lk"] != null)
                            {
                                if (and)
                                    sql.CommandText += " and ";
                                else
                                    and = true;
                                string parameterName = "@" + prop.Name + "lk";
                                sql.CommandText += (await GetDBSchema(type, prop.Name)) + "." + prop.Name + " like " + parameterName;
                                await AddParameter(sql, parameterName, filter[prop.Name]["lk"]);
                            }

                        }

                    }
                }

                await Task.FromResult(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static async Task GetWhere(SqlCommand sql, Type type, dynamic filter)
        {
            try
            {

                if (filter != null)
                {
                    
                    sql.CommandText += " WHERE ";
                    bool and = false;
                    foreach (var prop in type.GetProperties())
                    {
                        if (filter.ContainsKey(prop.Name))
                        {
                            //SqlParameter sqlParameter = new SqlParameter("@"+prop.Name,);
                            if (filter[prop.Name]["eq"] != null)
                            {
                                if (and)
                                    sql.CommandText += " and ";
                                else
                                    and = true;
                                string parameterName = "@" + prop.Name + "eq";
                                sql.CommandText += (await GetDBSchema(type, prop.Name)) + "." + prop.Name + " = " + parameterName;
                                await AddParameter(sql, parameterName, filter[prop.Name]["eq"]);

                            }
                            if (filter[prop.Name]["min"] != null)
                            {
                                if (and)
                                    sql.CommandText += " and ";
                                else
                                    and = true;
                                string parameterName = "@" + prop.Name + "min";
                                sql.CommandText += (await GetDBSchema(type, prop.Name)) + "." + prop.Name + " > " + parameterName;
                                await AddParameter(sql, parameterName, filter[prop.Name]["min"]);

                            }
                            if (filter[prop.Name]["mineq"] != null)
                            {
                                if (and)
                                    sql.CommandText += " and ";
                                else
                                    and = true;
                                string parameterName = "@" + prop.Name + "mineq";
                                sql.CommandText += (await GetDBSchema(type, prop.Name)) + "." + prop.Name + " >= " + parameterName;
                                await AddParameter(sql, parameterName, filter[prop.Name]["mineq"]);

                            }
                            if (filter[prop.Name]["max"] != null)
                            {
                                if (and)
                                    sql.CommandText += " and ";
                                else
                                    and = true;
                                string parameterName = "@" + prop.Name + "max";
                                sql.CommandText += (await GetDBSchema(type, prop.Name)) + "." + prop.Name + " < " + parameterName;
                                await AddParameter(sql, parameterName, filter[prop.Name]["max"]);

                            }
                            if (filter[prop.Name]["maxeq"] != null)
                            {
                                if (and)
                                    sql.CommandText += " and ";
                                else
                                    and = true;
                                string parameterName = "@" + prop.Name + "maxeq";
                                sql.CommandText += (await GetDBSchema(type, prop.Name)) + "." + prop.Name + " <= " + parameterName;
                                await AddParameter(sql, parameterName, filter[prop.Name]["maxeq"]);

                            }
                            if (filter[prop.Name]["lk"] != null)
                            {
                                if (and)
                                    sql.CommandText += " and ";
                                else
                                    and = true;
                                string parameterName = "@" + prop.Name + "lk";
                                sql.CommandText += (await GetDBSchema(type, prop.Name)) + "." + prop.Name + " like " + parameterName;
                                await AddParameter(sql, parameterName, filter[prop.Name]["lk"]);
                            }

                        }

                    }
                }

                await Task.FromResult(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static async Task AddParameter(SqlCommand sql, string parameterName, JToken jToken)
        {
            SqlParameter sqlParameter = null;
            switch (jToken.Type)
            {
                case JTokenType.String:
                    sqlParameter = new SqlParameter(parameterName, jToken.ToString());
                    break;
                case JTokenType.Integer:
                    sqlParameter = new SqlParameter(parameterName, int.Parse(jToken.ToString()));
                    break;
                case JTokenType.Date:
                    sqlParameter = new SqlParameter(parameterName, DateTime.Parse(jToken.ToString()));
                    break;
                case JTokenType.Float:
                    sqlParameter = new SqlParameter(parameterName, float.Parse(jToken.ToString()));
                    break;

            }
            await Task.FromResult(sql.Parameters.Add(sqlParameter));
        }

        public static async Task<string> GetColumns(Type type, string[] format)
        {
            try
            {
                Type[] allowedType = new Type[]
                {
                    typeof(int),typeof(string),typeof(DateTime),typeof(float),typeof(decimal)
                };
                string result = string.Empty;
                foreach (PropertyInfo prop in type.GetProperties())
                {
                    if ((format == null) || format.Contains(prop.Name))
                    {
                        if (prop.PropertyType.IsGenericType)
                        {
                            result += await GetColumns(prop.PropertyType.GetGenericArguments()[0], null) + ",";

                        }
                        else if (allowedType.Contains(prop.PropertyType))
                        {
                            result += (await GetDBSchema(type, prop.Name)) + "." + prop.Name + ",";
                        }
                        else
                        {
                            result += await GetColumns(prop.PropertyType, null) + ",";
                        }
                    }

                }
                return await Task.FromResult(result.Substring(0, result.Length - 1));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static async Task<string> GetDBSchema(Type parentType, string type)
        {
            try
            {
                string schema = string.Empty;
                object[] attrs = parentType.GetProperty(type).GetCustomAttributes(true);
                //object[] attrs = type.GetCustomAttributes(true);
                var attr = attrs.ToList().Find(x => x.GetType() == typeof(DBSchema));

                //schema = ((attr == null ? null : (DBSchema)attr).schema);
                if (attr == null)
                {
                    attrs = parentType.GetCustomAttributes(true);
                    attr = attrs.ToList().Find(x => x.GetType() == typeof(DBSchema));
                    if (attr != null)
                        schema = ((DBSchema)attr).schema;
                }
                else
                {
                    schema = ((DBSchema)attr).schema;
                }
                return await Task.FromResult(schema);
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }
    }
}