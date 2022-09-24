using System;
using System.Data.SqlTypes;

namespace ShoonyaPe
{
    public class TransactionModel
    {
        public string transactionId { get; set; }
        public string Category { get; set; }
        public long Amount { get; set; }
        public long UserId { get; set; }
        public string label { get; set; }
        
        public SqlDateTime sqlDateTime { get; set; }
    }
}