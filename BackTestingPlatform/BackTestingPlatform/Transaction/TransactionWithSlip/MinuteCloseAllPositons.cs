﻿using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Signal;
using BackTestingPlatform.Utilities.TimeList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BackTestingPlatform.Transaction.TransactionWithSlip
{
    public class MinuteCloseAllPositonsWithSlip
    {
        /// <summary>
        /// 清空当前所有持仓，对所有的持仓生成平仓信号（等量反向）
        /// </summary>
        /// <param name="data"></param>
        /// <param name="positions"></param>
        /// <param name="myAccount"></param>
        /// <param name="now"></param>
        public static DateTime closeAllPositions(Dictionary<string, List<KLine>> data, ref SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>> positions, ref BasicAccount myAccount, DateTime now, double slipPoint)
        {
            Dictionary<string, MinuteSignal> signal = new Dictionary<string, MinuteSignal>();

            //查询当前持仓情况
            Dictionary<string, PositionsWithDetail> positionShot = new Dictionary<string, PositionsWithDetail>();
            Dictionary<string, PositionsWithDetail> positionLast = (positions.Count == 0 ? null : positions[positions.Keys.Last()]);
            //检查最新持仓，若无持仓，直接返回
            if (positionLast == null)
            {
                return now.AddMinutes(1); ;
            }
            else
                positionShot = new Dictionary<string, PositionsWithDetail>(positionLast);

            //生成清仓信号
            foreach (var position0 in positionShot.Values)
            {
                //若当前品种持仓量为0（仅用于记录历史持仓）
                if (position0.volume == 0)
                    continue;
                //对所有的持仓，生成现价等量反向的交易信号
                int index = TimeListUtility.MinuteToIndex(now);
                MinuteSignal nowSignal = new MinuteSignal() { code = position0.code, volume = - position0.volume,
                    time = now, tradingVarieties = position0.tradingVarieties, price = data[position0.code][index].close, minuteIndex = index };
                signal.Add(nowSignal.code, nowSignal);
            }
            //将清仓信号传给成交判断
            DateTime next = MinuteTransactionWithSlip3.computeMinuteClosePositions(signal, data, ref positions, ref myAccount, slipPoint: slipPoint, now: now);
            return next;
        }

    }
}
