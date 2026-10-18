using Bogus;
using WebApplication1.BankAPI.Models;
using WebApplication1.BankAPI.Services;

namespace WebApplication1.BankAPI.Data
{
    public static class DBSeeder
    {
        public static void Seed(BankDbContext dbContext)
        {
            if (!dbContext.Users.Any())
            {
                var rand = new Random();

                using var transaction = dbContext.Database.BeginTransaction();

                List<User> users = new List<User>();

                for (int i = 0; i < 20; i++)
                {
                    var userFaker = new Faker<User>()
                        .RuleFor(u => u.Name, f => f.Name.FullName())
                        .RuleFor(u => u.Username, f => f.Internet.UserName())
                        .RuleFor(u => u.Email, f => f.Internet.Email())
                        .RuleFor(u => u.Address, f => f.Address.FullAddress())
                        .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
                        .RuleFor(u => u.PathToPicture, f => f.Internet.Avatar())
                        .RuleFor(u => u.PasswordHash, f => PasswordService.HashPassword("password"))
                        .RuleFor(u => u.PasswordSalted, f => PasswordService.SaltPassword("password"))
                        .RuleFor(u => u.Active, true);
                    var user = userFaker.Generate();
                    users.Add(user);
                }

                dbContext.Users.AddRange(users);
                dbContext.SaveChanges();

                HashSet<string> usedNumbers = new HashSet<string>();
                List<BankAccount> accounts = new List<BankAccount>();

                for (int i = 0; i < 30; i++)
                {
                    string number;

                    do
                    {
                        number = rand.Next(10000000, 99999999).ToString();
                    } while (usedNumbers.Contains(number));

                    usedNumbers.Add(number);

                    var accountFaker = new Faker<BankAccount>()
                        .RuleFor(a => a.AccountNumber, number)
                        .RuleFor(a => a.AccountType, f => f.PickRandom(new[] { "Checking", "Savings" }))
                        .RuleFor(a => a.BalanceInCents, f => f.Random.Long(0, 10000000))
                        .RuleFor(a => a.IsActive, true)
                        .RuleFor(a => a.UserID, f => f.PickRandom(users).ID);
                }

                dbContext.BankAccounts.AddRange(accounts);
                dbContext.SaveChanges();

                Dictionary<int, long> balances = new Dictionary<int, long>();

                foreach (var account in accounts)
                {
                    balances.Add(account.ID, 0L);
                }

                List<BankTrans> transactions = new List<BankTrans>();
                DateTime start = new DateTime(2026, 1, 1, 0, 0, 0);
                
                foreach (var account in accounts)
                {
                    long amount = rand.Next(1000, 1000000);
                    start = start.AddMinutes(rand.Next(1, 30));

                    transactions.Add(new BankTrans
                    {
                        AccountID = account.ID,
                        TransactionType = TransactionTypes.Deposit,
                        AmountInCents = amount,
                        CreatedAtUtc = start
                    });

                    balances[account.ID] += amount;
                }

                while (transactions.Count < 500)
                {
                    start = start.AddMinutes(rand.Next(1, 120));

                    var account = accounts[rand.Next(accounts.Count)];

                    int transactionType = rand.Next(3);

                    if (transactionType == TransactionTypes.Deposit)
                    {
                        long amount = rand.Next(100, 100000);
                        transactions.Add(new BankTrans
                        {
                            AccountID = account.ID,
                            TransactionType = TransactionTypes.Deposit,
                            AmountInCents = amount,
                            CreatedAtUtc = start
                        });

                        balances[account.ID] += amount;
                    }
                    else if (transactionType == TransactionTypes.Withdrawal)
                    {
                        long amount = rand.Next(100, 100000);
                        if (balances[account.ID] >= amount)
                        {
                            transactions.Add(new BankTrans
                            {
                                AccountID = account.ID,
                                TransactionType = TransactionTypes.Withdrawal,
                                AmountInCents = amount,
                                CreatedAtUtc = start
                            });
                            balances[account.ID] -= amount;
                        }
                    }
                    else if (transactionType == TransactionTypes.Transfer)
                    {
                        var relatedAccount = accounts[rand.Next(accounts.Count)];
                        if (relatedAccount.ID != account.ID)
                        {
                            long amount = rand.Next(100, 100000);
                            if (balances[account.ID] >= amount)
                            {
                                var transferId = Guid.NewGuid();
                                transactions.Add(new BankTrans
                                {
                                    AccountID = account.ID,
                                    RelatedAccountID = relatedAccount.ID,
                                    TransactionType = TransactionTypes.Transfer,
                                    AmountInCents = amount,
                                    CreatedAtUtc = start,
                                    TransferID = transferId
                                });
                                transactions.Add(new BankTrans
                                {
                                    AccountID = relatedAccount.ID,
                                    RelatedAccountID = account.ID,
                                    TransactionType = TransactionTypes.Transfer,
                                    AmountInCents = amount,
                                    CreatedAtUtc = start,
                                    TransferID = transferId
                                });
                                balances[account.ID] -= amount;
                                balances[relatedAccount.ID] += amount;
                            }
                        }
                    }

                }

                dbContext.BankTrans.AddRange(transactions);

                foreach (var account in accounts)
                {
                    account.BalanceInCents = balances[account.ID];
                }
                
                dbContext.SaveChanges();
                transaction.Commit();
            }
        }
    }
}
