﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Positions
{
    public class BasicPositions
    {
        public string code { get; set; }
        public DateTime time { get; set; }
        public double volume { get; set; }
        public double price { get; set; }
        public List<TransactionRecord> record { get; set; }
        public double totalVolume { get; set; }
        public double transactionCost { get; set; }
        public double totalCost { get; set; }
    }
}
