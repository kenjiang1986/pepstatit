using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Job.DTO;
using MySql.Data.MySqlClient;

namespace Job.Helper
{
    public class SqlHelper
    {
        /// <summary>
        /// 通过Castle查询Sql语句
        /// </summary>
        /// <typeparam name="T">返回结果集的类型</typeparam>
        /// <param name="spname">存储过程名</param>
        /// <param name="paras">存储过程参数</param>
        /// <returns>返回指定类型的结果集</returns>
        public static IList<T> CastleExecuteSql<T>(string Sql, IDictionary<string, object> paras)
        {
            Type type = typeof(T);

            IDbConnection con = new SqlConnection(ConfigurationManager.AppSettings["DbConnection"]);
               
            IDbCommand cmd = con.CreateCommand();
            cmd.CommandText = Sql;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = con;
            if (paras != null)
            {
                ParamesDTO pa = null;
                foreach (string key in paras.Keys)
                {
                    #region 添加参数
                    IDbDataParameter pra = cmd.CreateParameter();
                    pra.ParameterName = key;
                    if (paras[key] is ParamesDTO)
                    {
                        pa = (ParamesDTO)paras[key];
                        pra.Value = pa.Value;
                        pra.Direction = pa.Direction;
                        pra.Size = pa.Size;
                    }
                    else
                    {
                        pra.Value = paras[key];
                    }
                    #endregion
                    cmd.Parameters.Add(pra);
                }
            }

            IList<T> result = new List<T>();
            bool conIsOpen = true;
            if (con.State == ConnectionState.Closed)
            {
                conIsOpen = false;
                con.Open();
            }
            IDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                #region 运用反射创建对象
                object obj = type.Assembly.CreateInstance(type.Name);
                ConstructorInfo constructure = type.GetConstructor(new Type[] { });
                obj = constructure.Invoke(new Object[] { });
                PropertyInfo[] pros = type.GetProperties();
                string tempColumnName = string.Empty;
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    tempColumnName = dr.GetName(i);
                    var columnInfo = pros.Where(p => p.Name == tempColumnName);
                    if (columnInfo != null && columnInfo.Count() > 0)
                    {
                        Type tp = columnInfo.First().PropertyType;
                        object g = dr[tempColumnName];
                        if (!Convert.IsDBNull(dr[tempColumnName]))
                        {

                            if (tp.Equals(typeof(Guid)))
                            {
                                g = Guid.Parse(g.ToString());
                            }
                            else if (tp.IsGenericType && tp.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                var innerType = tp.GetGenericArguments().First();
                                var nullAbleType = typeof(Nullable<>).MakeGenericType(innerType).GetConstructor(new Type[] { innerType });
                                var value = nullAbleType.Invoke(new object[] { g });
                                columnInfo.First().SetValue(obj, value, null);
                            }
                            else
                            {
                                columnInfo.First().SetValue(obj, Convert.ChangeType(g, tp), new object[] { });
                            }
                        }
                    }
                }
                #endregion
                result.Add((T)obj);
            }
            dr.Close();
            if (!conIsOpen)
            {
                con.Close();
            }
            return result;
        }

        /// <summary>
        /// 通过Castle调用存储过程
        /// </summary>
        /// <typeparam name="T">返回结果集的类型</typeparam>
        /// <param name="spname">存储过程名</param>
        /// <param name="paras">存储过程参数</param>
        /// <param name="dicOutPar">带输出参数的值</param>
        /// <returns>返回指定类型的结果集</returns>
        public static IList<T> CallProcedure<T>(string spname, IDictionary<string, object> paras, IDbConnection con)
        {
            Type type = typeof(T);
            //IDbConnection con = new SqlConnection(dbCon);
            IDbCommand cmd = con.CreateCommand();
            cmd.CommandText = spname;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Connection = con;
            if (paras != null)
            {
                ParamesDTO pa = null;
                foreach (string key in paras.Keys)
                {
                    #region 添加参数
                    IDbDataParameter pra = cmd.CreateParameter();
                    pra.ParameterName = key;
                    if (paras[key] is ParamesDTO)
                    {
                        pa = (ParamesDTO)paras[key];
                        pra.Value = pa.Value;
                        pra.Direction = pa.Direction;
                        pra.Size = pa.Size;
                    }
                    else
                    {
                        pra.Value = paras[key];
                    }
                    #endregion
                    cmd.Parameters.Add(pra);
                    //    LogManager.GetLogger("LogExceptionAttribute")
                    //.Error(string.Format("Name:{0},Key:{1},Value:{2}", spname, key, pa.Value));


                }
            }

            IList<T> result = new List<T>();
            bool conIsOpen = true;
            if (con.State == ConnectionState.Closed)
            {
                conIsOpen = false;
                con.Open();
            }
            IDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                #region 运用反射创建对象
                object obj = type.Assembly.CreateInstance(type.Name);
                ConstructorInfo constructure = type.GetConstructor(new Type[] { });
                obj = constructure.Invoke(new Object[] { });
                PropertyInfo[] pros = type.GetProperties();
                string tempColumnName = string.Empty;
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    tempColumnName = dr.GetName(i);
                    var columnInfo = pros.Where(p => p.Name == tempColumnName);

                    if (columnInfo != null && columnInfo.Count() > 0)
                    {
                        Type tp = columnInfo.First().PropertyType;
                        object g = dr[tempColumnName];
                        if (!Convert.IsDBNull(dr[tempColumnName]))
                        {
                            if (tp.Equals(typeof(Guid)))
                            {
                                g = Guid.Parse(g.ToString());
                            }
                            else if (tp.IsGenericType && tp.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                var innerType = tp.GetGenericArguments().First();
                                var nullAbleType = typeof(Nullable<>).MakeGenericType(innerType).GetConstructor(new Type[] { innerType });
                                var value = nullAbleType.Invoke(new object[] { g });
                                columnInfo.First().SetValue(obj, value, null);
                            }
                            else
                            {
                                columnInfo.First().SetValue(obj, Convert.ChangeType(g, tp), new object[] { });
                            }
                        }
                    }
                }
                #endregion
                result.Add((T)obj);
            }
            dr.Close();
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }

            #region 获取参数返回的值

            //foreach (IDbDataParameter dp in cmd.Parameters)
            //{
            //    if (dp.Direction == ParameterDirection.Output || dp.Direction == ParameterDirection.InputOutput || dp.Direction == ParameterDirection.ReturnValue)
            //    {
            //        dicOutPar.Add(dp.ParameterName, dp.Value);
            //    }
            //}

            #endregion
            return result;
        }


        public static IList<T> CallMySqlProcedure<T>(string spname, IDictionary<string, object> paras, MySqlConnection con)
        {

            con.Open();

            MySqlCommand cmd = new MySqlCommand(spname, con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            if (paras != null)
            {
                ParamesDTO pa = null;
                foreach (string key in paras.Keys)
                {
                    #region 添加参数
                  
                    if (paras[key] is ParamesDTO)
                    {
                        pa = (ParamesDTO)paras[key];
                        cmd.Parameters.Add(key, MySqlDbType.VarChar);
                        cmd.Parameters[key].Value = pa.Value;
                    }
                 
                    #endregion
                }
            }

           

            IDataReader dr = cmd.ExecuteReader();
            IList<T> result = new List<T>();
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }

            while (dr.Read())
            {
                #region 运用反射创建对象
                Type type = typeof(T);
                object obj = type.Assembly.CreateInstance(type.Name);
                ConstructorInfo constructure = type.GetConstructor(new Type[] { });
                obj = constructure.Invoke(new Object[] { });
                PropertyInfo[] pros = type.GetProperties();
                string tempColumnName = string.Empty;
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    tempColumnName = dr.GetName(i);
                    var columnInfo = pros.Where(p => p.Name == tempColumnName);

                    if (columnInfo != null && columnInfo.Count() > 0)
                    {
                        Type tp = columnInfo.First().PropertyType;
                        object g = dr[tempColumnName];
                        if (!Convert.IsDBNull(dr[tempColumnName]))
                        {
                            if (tp.Equals(typeof(Guid)))
                            {
                                g = Guid.Parse(g.ToString());
                            }
                            else if (tp.IsGenericType && tp.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                var innerType = tp.GetGenericArguments().First();
                                var nullAbleType = typeof(Nullable<>).MakeGenericType(innerType).GetConstructor(new Type[] { innerType });
                                var value = nullAbleType.Invoke(new object[] { g });
                                columnInfo.First().SetValue(obj, value, null);
                            }
                            else
                            {
                                columnInfo.First().SetValue(obj, Convert.ChangeType(g, tp), new object[] { });
                            }
                        }
                    }
                }
                #endregion
                result.Add((T)obj);
            }
            dr.Close();
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }

            return result;
        }
      
    }
}
