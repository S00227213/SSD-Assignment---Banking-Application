using System;
using System.Diagnostics;

namespace Banking_Application
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Enter your username:");
            string username = Console.ReadLine();

            Console.WriteLine("Enter your password:");
            string password = Console.ReadLine();

            // Authenticate the user
            if (!AuthenticationHelper.AuthenticateUser(username, password))
            {
                Console.WriteLine("Authentication failed. Exiting application.");
                LogAuthenticationAttempt(username, success: false);
                return;
            }

            // Check if the user is in either the "Bank Teller" or "Bank Teller Administrator" group
            bool isTeller = AuthenticationHelper.IsUserInGroup(username, "Bank Teller");
            bool isAdmin = AuthenticationHelper.IsUserInGroup(username, "Bank Teller Administrator");

            if (!isTeller && !isAdmin)
            {
                Console.WriteLine("You do not have permission to access this application.");
                LogAuthenticationAttempt(username, success: false);
                return;
            }

            LogAuthenticationAttempt(username, success: true);

            // Log the user's access level
            if (isAdmin)
            {
                Console.WriteLine($"Welcome, {username}! You are logged in as an Administrator.");
            }
            else
            {
                Console.WriteLine($"Welcome, {username}! You are logged in as a Teller.");
            }

            Data_Access_Layer dal = Data_Access_Layer.getInstance();
            dal.loadBankAccounts();
            bool running = true;

            do
            {
                Console.WriteLine("");
                Console.WriteLine("***Banking Application Menu***");
                Console.WriteLine("1. Add Bank Account");
                Console.WriteLine("2. Close Bank Account");
                Console.WriteLine("3. View Account Information");
                Console.WriteLine("4. Make Lodgement");
                Console.WriteLine("5. Make Withdrawal");
                Console.WriteLine("6. Exit");
                Console.WriteLine("CHOOSE OPTION:");
                String option = Console.ReadLine();

                switch (option)
                {
                    case "1": // Add Bank Account
                        String accountType = "";
                        int loopCount = 0;

                        do
                        {
                            if (loopCount > 0)
                                Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");

                            Console.WriteLine("");
                            Console.WriteLine("***Account Types***:");
                            Console.WriteLine("1. Current Account.");
                            Console.WriteLine("2. Savings Account.");
                            Console.WriteLine("CHOOSE OPTION:");
                            accountType = Console.ReadLine();

                            loopCount++;
                        } while (!(accountType.Equals("1") || accountType.Equals("2")));

                        String name = "";
                        loopCount = 0;

                        do
                        {
                            if (loopCount > 0)
                                Console.WriteLine("INVALID NAME ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Name: ");
                            name = Console.ReadLine();

                            loopCount++;
                        } while (name.Equals(""));

                        String addressLine1 = "";
                        loopCount = 0;

                        do
                        {
                            if (loopCount > 0)
                                Console.WriteLine("INVALID ADDRESS LINE 1 ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Address Line 1: ");
                            addressLine1 = Console.ReadLine();

                            loopCount++;
                        } while (addressLine1.Equals(""));

                        Console.WriteLine("Enter Address Line 2: ");
                        String addressLine2 = Console.ReadLine();

                        Console.WriteLine("Enter Address Line 3: ");
                        String addressLine3 = Console.ReadLine();

                        String town = "";
                        loopCount = 0;

                        do
                        {
                            if (loopCount > 0)
                                Console.WriteLine("INVALID TOWN ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Town: ");
                            town = Console.ReadLine();

                            loopCount++;
                        } while (town.Equals(""));

                        double balance = -1;
                        loopCount = 0;

                        do
                        {
                            if (loopCount > 0)
                                Console.WriteLine("INVALID OPENING BALANCE ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Opening Balance: ");
                            String balanceString = Console.ReadLine();

                            try
                            {
                                balance = Convert.ToDouble(balanceString);
                            }
                            catch
                            {
                                loopCount++;
                            }
                        } while (balance < 0);

                        Bank_Account ba;

                        if (Convert.ToInt32(accountType) == Account_Type.Current_Account)
                        {
                            double overdraftAmount = -1;
                            loopCount = 0;

                            do
                            {
                                if (loopCount > 0)
                                    Console.WriteLine("INVALID OVERDRAFT AMOUNT ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Overdraft Amount: ");
                                String overdraftAmountString = Console.ReadLine();

                                try
                                {
                                    overdraftAmount = Convert.ToDouble(overdraftAmountString);
                                }
                                catch
                                {
                                    loopCount++;
                                }
                            } while (overdraftAmount < 0);

                            ba = new Current_Account(name, addressLine1, addressLine2, addressLine3, town, balance, overdraftAmount);
                        }
                        else
                        {
                            double interestRate = -1;
                            loopCount = 0;

                            do
                            {
                                if (loopCount > 0)
                                    Console.WriteLine("INVALID INTEREST RATE ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Interest Rate: ");
                                String interestRateString = Console.ReadLine();

                                try
                                {
                                    interestRate = Convert.ToDouble(interestRateString);
                                }
                                catch
                                {
                                    loopCount++;
                                }
                            } while (interestRate < 0);

                            ba = new Savings_Account(name, addressLine1, addressLine2, addressLine3, town, balance, interestRate);
                        }

                        String accNo = dal.addBankAccount(ba);
                        Console.WriteLine("New Account Number Is: " + accNo);

                        LogTransaction("Account Creation", accNo, name, "Account successfully created.");

                        break;

                    case "2": // Close Bank Account
                        // Check if the user is an administrator
                        if (!isAdmin)
                        {
                            Console.WriteLine("You do not have permission to close accounts.");
                            break;
                        }

                        Console.WriteLine("Enter Account Number: ");
                        accNo = Console.ReadLine();

                        ba = dal.findBankAccountByAccNo(accNo);

                        if (ba is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            Console.WriteLine(ba.ToString());

                            String ans = "";

                            do
                            {
                                Console.WriteLine("Proceed With Deletion (Y/N)?");
                                ans = Console.ReadLine();

                                switch (ans)
                                {
                                    case "Y":
                                    case "y":
                                        dal.closeBankAccount(accNo);
                                        LogTransaction("Account Closure", accNo, ba.name, "Account successfully closed.");
                                        break;
                                    case "N":
                                    case "n":
                                        break;
                                    default:
                                        Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");
                                        break;
                                }
                            } while (!(ans.Equals("Y") || ans.Equals("y") || ans.Equals("N") || ans.Equals("n")));
                        }
                        break;

                    case "3": // View Account Information
                        Console.WriteLine("Enter Account Number: ");
                        accNo = Console.ReadLine();

                        ba = dal.findBankAccountByAccNo(accNo);

                        if (ba is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            Console.WriteLine(ba.ToString());
                        }
                        break;

                    case "4": // Make Lodgement
                        Console.WriteLine("Enter Account Number: ");
                        accNo = Console.ReadLine();

                        ba = dal.findBankAccountByAccNo(accNo);

                        if (ba is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            double amountToLodge = -1;
                            loopCount = 0;

                            do
                            {
                                if (loopCount > 0)
                                    Console.WriteLine("INVALID AMOUNT ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Amount To Lodge: ");
                                String amountToLodgeString = Console.ReadLine();

                                try
                                {
                                    amountToLodge = Convert.ToDouble(amountToLodgeString);
                                }
                                catch
                                {
                                    loopCount++;
                                }
                            } while (amountToLodge < 0);

                            dal.lodge(accNo, amountToLodge);
                            LogTransaction("Lodgement", accNo, ba.name, $"Amount lodged: €{amountToLodge}");
                        }
                        break;

                    case "5": // Withdraw
                        Console.WriteLine("Enter Account Number: ");
                        accNo = Console.ReadLine();

                        ba = dal.findBankAccountByAccNo(accNo);

                        if (ba is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            double amountToWithdraw = -1;
                            loopCount = 0;

                            do
                            {
                                if (loopCount > 0)
                                    Console.WriteLine("INVALID AMOUNT ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Amount To Withdraw (€" + ba.getAvailableFunds() + " Available): ");
                                String amountToWithdrawString = Console.ReadLine();

                                try
                                {
                                    amountToWithdraw = Convert.ToDouble(amountToWithdrawString);
                                }
                                catch
                                {
                                    loopCount++;
                                }
                            } while (amountToWithdraw < 0);

                            bool withdrawalOK = dal.withdraw(accNo, amountToWithdraw);

                            if (withdrawalOK == false)
                            {
                                Console.WriteLine("Insufficient Funds Available.");
                            }
                            else
                            {
                                LogTransaction("Withdrawal", accNo, ba.name, $"Amount withdrawn: €{amountToWithdraw}");
                            }
                        }
                        break;

                    case "6": // Exit
                        running = false;
                        break;

                    default:
                        Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");
                        break;
                }
            } while (running != false);
        }

        private static void LogTransaction(string transactionType, string accountNo, string accountName, string details)
        {
            string logMessage = $"WHO: {Environment.UserName}, WHAT: {transactionType}, WHERE: {Environment.MachineName}, " +
                                $"WHEN: {DateTime.Now}, Account No: {accountNo}, Name: {accountName}, DETAILS: {details}";
            try
            {
                if (!EventLog.SourceExists("SSD Banking Application"))
                {
                    EventLog.CreateEventSource("SSD Banking Application", "Application");
                }

                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "SSD Banking Application";
                    eventLog.WriteEntry(logMessage, EventLogEntryType.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log: {ex.Message}");
            }
        }

        private static void LogAuthenticationAttempt(string username, bool success)
        {
            string logMessage = success
                ? $"Successful login for user {username}."
                : $"Failed login attempt for user {username}.";

            try
            {
                if (!EventLog.SourceExists("SSD Banking Application"))
                {
                    EventLog.CreateEventSource("SSD Banking Application", "Application");
                }

                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "SSD Banking Application";
                    eventLog.WriteEntry(logMessage, success ? EventLogEntryType.SuccessAudit : EventLogEntryType.FailureAudit);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing authentication log: {ex.Message}");
            }
        }
    }
}
