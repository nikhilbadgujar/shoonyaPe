using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ShoonyaPe.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private List<TaxExemptResponseModel> _TaxRulesList = new List<TaxExemptResponseModel>();
        private List<CategoryTaxModel> _categoryTax = new List<CategoryTaxModel>();
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly string _sqlConnectionString;
        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
            _TaxRulesList.Add(new TaxExemptResponseModel()
            {
                section = "80DDB – Medical Expenditure",
                link = "https://cleartax.in/s/80c-80-deductions#Section80DDB",
                maxAmountExempt = "80000"
            });
            _TaxRulesList.Add(new TaxExemptResponseModel()
            {
                section = "80D – Medical Insurance",
                link = "https://cleartax.in/s/80c-80-deductions#Section80D",
                maxAmountExempt = "100000"
            });
            _TaxRulesList.Add(new TaxExemptResponseModel()
            {
                section = "80E – Interest on Education Loan",
                link = "https://cleartax.in/s/80c-80-deductions#Section80E",
                maxAmountExempt = "No Limit"
            });
            _categoryTax.Add(new CategoryTaxModel()
            {
                category = "Medical",
                section = "80DDB – Medical Expenditure"
            });
            _categoryTax.Add(new CategoryTaxModel()
            {
                category = "Insurance",
                section = "80D – Medical Insurance"
            });
            _categoryTax.Add(new CategoryTaxModel()
            {
                category = "EducationLoan",
                section = "80E – Interest on Education Loan"
            });
            _sqlConnectionString = "Server=tcp:shooyna.database.windows.net,1433;Initial Catalog=shoonyaPay;Persist Security Info=False;User ID=ayushbansal323;Password=Pass#aaga123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        }

        [HttpGet]
        public async Task<IEnumerable<ExpenseResponseModel>> GetTransactionSummary( int userId)
        {
            var LastYear = DateTime.Now.AddMonths(-6);
            var StartDate = new DateTime(LastYear.Year, LastYear.Month, 1);
            var SqlQuery = $"Select * from Transactions where UserId={userId} and TRANSACTIONDATE >={StartDate}";
            
            List<TransactionModel> TransactionModelsList = new List<TransactionModel>();
            var sqlConnection = new SqlConnection(_sqlConnectionString);
            using (sqlConnection)
            {
                sqlConnection.Open();
                var com = new SqlCommand(SqlQuery, sqlConnection);
                var reader = await com.ExecuteReaderAsync();
                
                while (reader.Read())
                {
                    TransactionModelsList.Add(new TransactionModel()
                    {
                        transactionId = reader.GetString(0),
                        Amount = reader.GetInt64(1),
                        Category = reader.GetString(2),
                        UserId = reader.GetInt64(3),
                        label = reader.GetString(4),
                        sqlDateTime = reader.GetSqlDateTime(5)
                    });
                }

                sqlConnection.Close();
            }
            
            var tempModels = new Dictionary<string, long>();
            foreach (var variTransactionModel in TransactionModelsList)
            {
                var date =variTransactionModel.sqlDateTime.Value;
                var month = date.Month.ToString();
                if (tempModels.ContainsKey(month))
                {
                    tempModels[month] += variTransactionModel.Amount;
                }
                else
                {
                    tempModels.Add(month,variTransactionModel.Amount);
                }
            }

            var responseModel = new List<ExpenseResponseModel>();
            foreach (var temp in tempModels)
            {
                responseModel.Add(new ExpenseResponseModel()
                {
                    month = temp.Key,
                    Amount = temp.Value
                });
            }

            return responseModel;
        }

        [HttpGet]
        public async Task<IEnumerable<CategoryExpenseReponseModel>> GetCatgoryTransactions(int userId, DateTime startDate,DateTime endDate)
        {
            var query =
                "Select * from Transactions where UserId={userId} and TRANSACTIONDATE >={startDate} and TRANSACTIONDATE <= {endDate}";
            var sqlConnection = new SqlConnection(_sqlConnectionString);
            List<TransactionModel> TransactionModelsList = new List<TransactionModel>();
            using (sqlConnection)
            {
                sqlConnection.Open();
                var com = new SqlCommand(query, sqlConnection);
                var reader = await com.ExecuteReaderAsync();
                
                while (reader.Read())
                {
                    TransactionModelsList.Add(new TransactionModel()
                    {
                        transactionId = reader.GetString(0),
                        Amount = reader.GetInt64(1),
                        Category = reader.GetString(2),
                        UserId = reader.GetInt64(3),
                        label = reader.GetString(4),
                        sqlDateTime = reader.GetSqlDateTime(5)
                    });
                }

                sqlConnection.Close();
            }
            var tempModels = new Dictionary<string, long>();
            foreach (var variTransactionModel in TransactionModelsList)
            {
                if (tempModels.ContainsKey(variTransactionModel.Category))
                {
                    tempModels[variTransactionModel.Category] += variTransactionModel.Amount;
                }
                else
                {
                    tempModels.Add(variTransactionModel.Category,variTransactionModel.Amount);
                }
            } 
            var responseModel = new List<CategoryExpenseReponseModel>();
            foreach (var temp in tempModels)
            {
                responseModel.Add(new CategoryExpenseReponseModel()
                {
                    Category = temp.Key,
                    Amount = temp.Value,
                    Month = TransactionModelsList[0].sqlDateTime.Value.Month.ToString()
                });
            }

            return responseModel;
        }
        [HttpGet]
        public async Task<TaxExemptResponseModel> GetCatgeoryTaxExemption(string Category)
        {
            var section = _categoryTax.Where(x => x.category == Category).Select(r=>r.section);
            var response = _TaxRulesList.Where(s=>s.section.Equals(section)).FirstOrDefault();
            return response;
        }
        [HttpGet]
        public async Task<IEnumerable<ExpenseResponseModel>> GetExpenseForcast( int userId)
        {
            var LastYear = DateTime.Now.AddMonths(-1);
            var StartDate = new DateTime(LastYear.Year, LastYear.Month, 1);
            var endDate = new DateTime(LastYear.Year, LastYear.Month, 30);
            var SqlQuery = $"Select * from Transactions where UserId={userId} and TRANSACTIONDATE >={StartDate} and TRANSACTIONDATE <={endDate}";
            
            List<TransactionModel> TransactionModelsList = new List<TransactionModel>();
            var sqlConnection = new SqlConnection(_sqlConnectionString);
            using (sqlConnection)
            {
                sqlConnection.Open();
                var com = new SqlCommand(SqlQuery, sqlConnection);
                var reader = await com.ExecuteReaderAsync();
                
                while (reader.Read())
                {
                    TransactionModelsList.Add(new TransactionModel()
                    {
                        transactionId = reader.GetString(0),
                        Amount = reader.GetInt64(1),
                        Category = reader.GetString(2),
                        UserId = reader.GetInt64(3),
                        label = reader.GetString(4),
                        sqlDateTime = reader.GetSqlDateTime(5)
                    });
                }

                sqlConnection.Close();
            }
            
            var tempModels = new Dictionary<string, long>();
            foreach (var variTransactionModel in TransactionModelsList)
            {
                var date =variTransactionModel.sqlDateTime.Value;
                var month = date.Month.ToString();
                if (tempModels.ContainsKey(month))
                {
                    tempModels[month] += variTransactionModel.Amount;
                }
                else
                {
                    tempModels.Add(month,variTransactionModel.Amount);
                }
            }

            long amount = 0;
            foreach (var tempamount in tempModels)
            {
                amount = tempamount.Value;
            }

            var Month = DateTime.Now.Month;

            var responseModel = new List<ExpenseResponseModel>();
            responseModel.Add(new ExpenseResponseModel()
            {
                month = DateTime.Now.AddMonths(1).ToString(),
                Amount = amount*(1.03)
            });
            responseModel.Add(new ExpenseResponseModel()
            {
                month = DateTime.Now.AddMonths(2).ToString(),
                Amount = amount*(1.05)
            });
            responseModel.Add(new ExpenseResponseModel()
            {
                month = DateTime.Now.AddMonths(3).ToString(),
                Amount = amount * (1.07)
            });
            return responseModel;
        }
        
    }
}
