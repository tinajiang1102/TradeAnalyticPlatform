﻿using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.DataAccess
{
    public abstract class SequentialDataRepository<T> where T : Sequential , new()
    {
        //int CacheDataUpdateCycleDayLength = 1;    //CacheData有效期天数
        //bool CacheDataShouldPutToMemCache = true;   //
        const string PATH_KEY = "CacheData.Path.Sequential";

        /// <summary>
        /// 由DataTable中的行向实体类的转换函数的默认实现。
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public virtual T toEntityFromCsv(DataRow row)
        {
            return DataTableUtils.CreateItemFromRow<T>(row);    
        }

        /// <summary>
        ///  尝试从Wind获取数据
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public abstract List<T> fetchFromWind(string code, DateTime date);

        public abstract List<T> fetchFromDefaultMssql(string code, DateTime date);
      
        /// <summary>
        ///  尝试从本地csv文件获取数据
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public List<T> fetchFromLocalCsv(string code, DateTime date, string tag = null)
        {
            if (tag == null) tag = typeof(T).ToString();
            var filePath = FileUtils.GetCacheDataFilePath(PATH_KEY, tag, code, date.ToString("yyyyMMdd"));
            DataTable dt = CsvFileUtils.ReadFromCsvFile(filePath);
            if (dt == null) return null;
            return dt.AsEnumerable().Select(toEntityFromCsv).ToList();
        }

        /// <summary>
        /// 先后尝试从本地csv文件，Wind获取数据。
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public virtual List<T> fetchFromLocalCsvOrWind(string code, DateTime date, string tag = null)
        {
            return _fetchFromManySouresAndSave(code, date, tag, true, true, false,false);
        }

        /// <summary>
        /// 先后尝试从本地csv文件，Wind获取数据。若无本地csv，则保存到CacheData文件夹。
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public List<T> fetchFromLocalCsvOrWindAndSave(string code, DateTime date, string tag = null)
        {
            return _fetchFromManySouresAndSave(code, date, tag, true, true, false, true);
        }
        /// <summary>
        /// 先后尝试从本地csv文件，默认MSSQL数据库源获取数据。
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public List<T> fetchFromLocalCsvOrMssql(string code, DateTime date, string tag = null)
        {
            return _fetchFromManySouresAndSave(code, date, tag, true, false, true, false);
        }

        /// <summary>
        /// 先后尝试从本地csv文件，默认MSSQL数据库源获取数据。若无本地csv，则保存到CacheData文件夹。
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public List<T> fetchFromLocalCsvOrMssqlAndSave(string code, DateTime date, string tag = null)
        {
            return _fetchFromManySouresAndSave(code, date, tag, true, false, true, true);          
        }

        private List<T> _fetchFromManySouresAndSave(string code, DateTime date, string tag,bool tryCsv,bool tryWind,bool tryMssql0,bool saveToCsv)
        {
            if (tag == null) tag = typeof(T).ToString();
            List<T> result = null;
            bool csvHasData = false;
            
            if (tryCsv)
            {
                //尝试从csv获取
                try
                {
                    result = fetchFromLocalCsv(code, date, tag);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                if (result != null) csvHasData = true;
            }           
            if (result == null && tryWind)            
            {
                //尝试从Wind获取
                try
                {
                    result = fetchFromWind(code, date);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            if (result == null && tryMssql0)
            {               
                try
                {
                    //尝试从默认MSSQL获取
                    result = fetchFromDefaultMssql(code, date);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }
            if (!csvHasData && result != null && saveToCsv)
            {
                //如果数据不是从csv获取的，可保存至本地，存为csv文件
                saveToLocalCsvFile(result, code, date, tag);
            }
           
            return result;
        }

        /// <summary>
        /// 将数据以csv文件的形式保存到CacheData文件夹下的预定路径
        /// </summary>
        /// <param name="data"></param>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <param name="tag"></param>
        public void saveToLocalCsvFile(IList<T> data, string code, DateTime date, string tag = null)
        {
            if (tag == null) tag = typeof(T).ToString();
            if (data==null ||　data.Count==0)
            {
                Console.WriteLine("Nothing to save!");
                return;
            }
            var dt = DataTableUtils.ToDataTable(data);
            var path = FileUtils.GetCacheDataFilePath(PATH_KEY, tag, code, date.ToString("yyyyMMdd"));
            CsvFileUtils.WriteToCsvFile(path, dt);
            var s = (File.Exists(path)) ? "old file has been overwritten." : "";
            Console.WriteLine("{0} saved! {1}", path,s);
        }
    }
}
