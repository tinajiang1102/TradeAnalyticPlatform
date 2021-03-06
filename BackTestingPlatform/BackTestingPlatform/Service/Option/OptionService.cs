﻿using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BackTestingPlatform.Service.Option
{
    /// <summary>
    /// Option的Tick，Minute等数据的相关服务
    /// </summary>
    public class OptionService
    {
        OptionMinuteDataRepository optionMinuteDataRepository = Platforms.container.Resolve<OptionMinuteDataRepository>();

        public List<OptionMinuteData> loadOptionMinuteData(string optionCode,DateTime date)
        {
            List<OptionMinuteData> optionData;
            var filePath= FileUtils.GetCacheDataFilePath(OptionMinuteDataRepository.PATH_KEY,"MinuteDataFromWind",optionCode,date.ToString("yyyyMMdd"));
            //若本地文件存在，则从本地读取否者先从万德或者数据库中读取
            if (File.Exists(filePath))
            {
                optionData = optionMinuteDataRepository.fetchAllFromLocalCsvFile(filePath);   
            }
            else
            {
                optionData = optionMinuteDataRepository.fetchMinuteDataFromWind(optionCode, date);
                if (optionData!=null)
                {
                    optionMinuteDataRepository.saveToLocalFile(optionData, filePath);
                }
            }
            return optionData==null? null :optionData;
        }

    }
}
