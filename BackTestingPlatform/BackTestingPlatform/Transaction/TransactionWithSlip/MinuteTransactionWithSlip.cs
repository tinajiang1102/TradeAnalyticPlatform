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
    public static class MinuteTransactionWithSlip
    {
        public static DateTime computeMinutePositions(Dictionary<string, MinuteSignal> signal, Dictionary<string, List<KLine>> data, ref SortedDictionary<DateTime, Dictionary<string, MinutePositions>> positions,DateTime now,double slipPoint=0.003)
        {
            if (signal==null || signal.Count==0)
            {
                return now.AddMinutes(1);
            }
            Dictionary<string, MinutePositions> positionShot = new Dictionary<string, MinutePositions>();
            Dictionary<string,MinutePositions> positionLast = (positions.Count==0?null:positions[positions.Keys.Last()]);
            if (positionLast!=null)
            {
                positionShot = new Dictionary<string, MinutePositions>(positionLast);
            }

            foreach (var signal0 in signal.Values)
            {
                if (signal0.volume!=0)
                {
                    now = (signal0.time > now) ? signal0.time : now;
                    MinutePositions position0 = new MinutePositions();
                    position0.minuteIndex = TimeListUtility.MinuteToIndex(signal0.time);
                    position0.code = signal0.code;
                    if (positionLast!=null && positionLast.ContainsKey(position0.code))
                    {
                        position0.volume = positionLast[position0.code].volume + signal0.volume;
                        position0.transactionCost = positionLast[position0.code].transactionCost + Math.Abs(signal0.volume * slipPoint * signal0.price);

                    }
                    else
                    {
                        position0.volume = signal0.volume;
                        position0.transactionCost = Math.Abs(signal0.volume * slipPoint * signal0.price);
                    }
                    position0.time = now;
                    position0.currentPrice = signal0.price;
                    if (positionShot.ContainsKey(position0.code))
                    {
                        positionShot[position0.code] = position0;
                    }
                    else
                    {
                        positionShot.Add(signal0.code, position0);
                    }
                }
            }
            positions.Add(now, positionShot);
            return now.AddMinutes(1);
        }
    }
}
