﻿using BackTestingPlatform.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Utilities
{
    public class TradeDaysUtils
    {
        private static List<DateTime> _tradeDays;
        private static List<DateTime> getTradeDays()
        {
            if (_tradeDays == null)
            {
                _tradeDays = Caches.get<List<DateTime>>("TradeDays");
            }
            return _tradeDays;
        }
        private static DateTime getTradeDay(int index)
        {
            if (index >= 0 && index < getTradeDays().Count)
            {
                return getTradeDays()[index];
            }
            return DateTime.MinValue;
        }

        /// <summary>
        /// get TradeDay List by a range [firstDate,lastDate]
        /// </summary>
        /// <param name="firstDate"></param>
        /// <param name="lastDate"></param>
        /// <returns></returns>
        public static List<DateTime> getTradeDays(DateTime firstDate, DateTime lastDate)
        {
            int x1 = Math.Abs(getTradeDays().BinarySearch(firstDate));
            int x2 = getTradeDays().BinarySearch(lastDate);
            x2 = x2 < 0 ? -x2 - 1 : x2;
            return getTradeDays().GetRange(x1, x2 - x1 + 1);
        }

        public static List<DateTime> getTradeDays(int firstDate, int lastDate)
        {
            return getTradeDays(
                Kit.ToDateTime(firstDate, 0), 
                Kit.ToDateTime(lastDate, 235959));
        }



        /// <summary>
        /// 判断当前日期是否为交易日
        /// </summary>
        /// <param name="today">当前日</param>
        /// <returns></returns>
        public static bool isTradeDay(DateTime today)
        {
            return getTradeDays().BinarySearch(today) >= 0;
        }
        /// <summary>
        /// 给出上一交易日,即比当前天早的交易日中最晚的一个
        /// </summary>
        /// <param name="today">当前日</param>
        /// <returns>返回前一交易日</returns>
        public static DateTime Previous(DateTime today)
        {
            int index = getTradeDays().BinarySearch(today);
            return getTradeDay(Math.Abs(index) - 1);
        }

        /// <summary>
        /// 给出下一交易日,即比当前天晚的交易日中最早的一个
        /// </summary>
        /// <param name="today">当前日</param>
        /// <returns>下一交易日</returns>
        public static DateTime Next(DateTime today)
        {
            int index = getTradeDays().BinarySearch(today);
            if (index < 0) index = -index - 1;
            return getTradeDay(index + 1);
        }

        /// <summary>
        /// 给出当前日期最近的交易日。如果今日是交易日返回今日，否者返回下一个最近的交易日。
        /// </summary>
        /// <param name="today">当前日期</param>
        /// <returns>交易日</returns>
        public static DateTime NextOrCurrent(DateTime today)
        {
            int index = getTradeDays().BinarySearch(today);            
            return getTradeDay(Math.Abs(index));
        }

        /// <summary>
        ///获取间隔的交易日计数,计数包含day1,day2。
        /// 例如，Jan-1,Jan-2,Jan-3不是交易日，Jan-4，Jan-5是交易日,则
        /// GetSpanOfTradeDays(Jan-4,Jan-5)=2
        /// GetSpanOfTradeDays(Jan-3,Jan-5)=2
        /// GetSpanOfTradeDays(Jan-1,Jan-3)=0
        /// </summary>
        /// <param name="day1">开始日期</param>
        /// <param name="day2">结束日期</param>
        /// <returns>间隔的交易日天数</returns>
        public static int GetSpanOfTradeDays(DateTime day1, DateTime day2)
        {
            int x1 = getTradeDays().BinarySearch(day1);
            int x2 = getTradeDays().BinarySearch(day2);
            if (x1 < 0) x1 = -x1;
            if (x2 < 0) x2 = -x2 - 1;
            return x2 - x1 + 1;
        }



        /// <summary>
        /// 判断当日是本月第几周。
        /// </summary>
        /// <param name="day">日期</param>
        /// <returns>第几周</returns>
        public int getWeekOfMonth(DateTime dt)
        {           
            int weekNum = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNum;
        }

    }
}
