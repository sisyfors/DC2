using System;

namespace WebApplication2.Models
{
    public class AccountSummaryDto
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public long BalanceInCents { get; set; }
        public bool IsActive { get; set; }
    }

    public class TransactionDto
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public long AmountInCents { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PageDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    public class HistoryResponseDto
    {
        public List<AccountSummaryDto> Summary { get; set; } = new List<AccountSummaryDto>();
        public List<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
        public PageDto Pagination { get; set; } = new PageDto();
    }

    public class HistoryViewModel
    {
        public string AccountNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string SortBy { get; set; } = "date";
        public string SortOrder { get; set; } = "desc";
        public HistoryResponseDto Data { get; set; } = new HistoryResponseDto();
    }
}
