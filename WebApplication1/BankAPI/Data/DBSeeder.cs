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

                for (int i = 0; i < 25; i++)
                {
                    var userFaker = new Faker<User>()
                        .RuleFor(u => u.Name, f => f.Name.FullName())
                        .RuleFor(u => u.Username, f => f.Internet.UserName())
                        .RuleFor(u => u.UserAddress, f => f.Address.FullAddress())
                        .RuleFor(u => u.UserPhone, f => f.Phone.PhoneNumber())
                        .RuleFor(u => u.UserPfpPath, f => "default.png")
                        .RuleFor(u => u.UserActiveFlag, true)
                        .FinishWith((f, u) =>
                        {
                            u.Username = $"{u.Username}{i}".ToLower();
                            u.UserEmail = $"{u.Username}@example.com";

                            var password = "Password123!";
                            PasswordGen.HashPassword(password, out string hash, out string salted);
                            u.UserPasswordHash = hash;
                            u.UserPasswordSalt = salted;
                        });

                    var user = userFaker.Generate();
                    users.Add(user);
                }

                PasswordGen.HashPassword("AdminPassword123!", out string adminHash, out string adminSalt);
                User admin = new User
                {
                    Name = "Admin User",
                    Username = "admin",
                    UserEmail = "admin@example.com",
                    UserAddress = "123 Admin St",
                    UserPhone = "0401557881",
                    UserPfpPath = "default.png",
                    UserActiveFlag = true,
                    UserPasswordHash = adminHash,
                    UserPasswordSalt = adminSalt,
                    Role = "Administrator"
                };
                
                users.Add(admin);

                dbContext.Users.AddRange(users);
                dbContext.SaveChanges();

                HashSet<string> usedNumbers = new HashSet<string>();
                List<BankAccount> accounts = new List<BankAccount>();

                for (int i = 0; i < 35; i++)
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
                        .RuleFor(a => a.BalanceInCents, 0)
                        .RuleFor(a => a.IsActive, true)
                        .RuleFor(a => a.UserID, f => f.PickRandom(users).ID);

                    accounts.Add(accountFaker.Generate());
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

                while (transactions.Count < 505)
                {
                    start = start.AddMinutes(rand.Next(1, 120));

                    var account = accounts[rand.Next(accounts.Count)];
                    long amount = rand.Next(100, 100000);

                    int transactionType = rand.Next(3);

                    switch (transactionType)
                    {
                        case 0: // deposit
                            transactions.Add(new BankTrans
                            {
                                AccountID = account.ID,
                                TransactionType = TransactionTypes.Deposit,
                                AmountInCents = amount,
                                CreatedAtUtc = start
                            });

                            balances[account.ID] += amount;
                            break;
                        case 1: // withdrawal
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

                            break;
                        case 2: // transfer
                            var relatedAccount = accounts[rand.Next(accounts.Count)];
                            if (relatedAccount.ID != account.ID)
                            {
                                if (balances[account.ID] >= amount)
                                {
                                    var transferId = Guid.NewGuid();
                                    transactions.Add(new BankTrans
                                    {
                                        AccountID = account.ID,
                                        RelatedAccountID = relatedAccount.ID,
                                        TransactionType = TransactionTypes.TransferOut,
                                        AmountInCents = amount,
                                        CreatedAtUtc = start,
                                        TransferID = transferId
                                    });
                                    transactions.Add(new BankTrans
                                    {
                                        AccountID = relatedAccount.ID,
                                        RelatedAccountID = account.ID,
                                        TransactionType = TransactionTypes.TransferIn,
                                        AmountInCents = amount,
                                        CreatedAtUtc = start,
                                        TransferID = transferId
                                    });
                                    balances[account.ID] -= amount;
                                    balances[relatedAccount.ID] += amount;
                                }
                            }

                            break;
                    }
                }

                dbContext.BankTransactions.AddRange(transactions);

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
