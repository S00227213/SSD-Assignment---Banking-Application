using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Microsoft.Data.Sqlite;

namespace Banking_Application
{
    public class Data_Access_Layer
    {
        private List<Bank_Account> accounts;
        public static String databaseName = "Banking Database.db";
        private static Data_Access_Layer instance = new Data_Access_Layer();

        private Data_Access_Layer()
        {
            accounts = new List<Bank_Account>();
        }

        public static Data_Access_Layer getInstance()
        {
            return instance;
        }

        private SqliteConnection getDatabaseConnection()
        {
            String databaseConnectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = Data_Access_Layer.databaseName,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

            return new SqliteConnection(databaseConnectionString);
        }

        private void initialiseDatabase()
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS Bank_Accounts(    
                        accountNo TEXT PRIMARY KEY,
                        name TEXT NOT NULL,
                        address_line_1 TEXT,
                        address_line_2 TEXT,
                        address_line_3 TEXT,
                        town TEXT NOT NULL,
                        balance REAL NOT NULL,
                        accountType INTEGER NOT NULL,
                        overdraftAmount REAL,
                        interestRate REAL
                    ) WITHOUT ROWID
                ";

                command.ExecuteNonQuery();
            }
        }

        public void loadBankAccounts()
        {
            if (!File.Exists(Data_Access_Layer.databaseName))
                initialiseDatabase();
            else
            {
                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM Bank_Accounts";
                    SqliteDataReader dr = command.ExecuteReader();

                    while (dr.Read())
                    {
                        int accountType = dr.GetInt16(7);

                        if (accountType == Account_Type.Current_Account)
                        {
                            Current_Account ca = new Current_Account();
                            ca.accountNo = dr.GetString(0);
                            ca.name = EncryptionHelper.Decrypt(dr.GetString(1)); // Decrypt PII
                            ca.address_line_1 = EncryptionHelper.Decrypt(dr.GetString(2));
                            ca.address_line_2 = EncryptionHelper.Decrypt(dr.GetString(3));
                            ca.address_line_3 = EncryptionHelper.Decrypt(dr.GetString(4));
                            ca.town = EncryptionHelper.Decrypt(dr.GetString(5));
                            ca.balance = dr.GetDouble(6);
                            ca.overdraftAmount = dr.GetDouble(8);
                            accounts.Add(ca);
                        }
                        else
                        {
                            Savings_Account sa = new Savings_Account();
                            sa.accountNo = dr.GetString(0);
                            sa.name = EncryptionHelper.Decrypt(dr.GetString(1)); // Decrypt PII
                            sa.address_line_1 = EncryptionHelper.Decrypt(dr.GetString(2));
                            sa.address_line_2 = EncryptionHelper.Decrypt(dr.GetString(3));
                            sa.address_line_3 = EncryptionHelper.Decrypt(dr.GetString(4));
                            sa.town = EncryptionHelper.Decrypt(dr.GetString(5));
                            sa.balance = dr.GetDouble(6);
                            sa.interestRate = dr.GetDouble(9);
                            accounts.Add(sa);
                        }
                    }
                }
            }
        }

        public String addBankAccount(Bank_Account ba)
        {
            accounts.Add(ba);

            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    INSERT INTO Bank_Accounts (
                        accountNo, name, address_line_1, address_line_2, 
                        address_line_3, town, balance, accountType, overdraftAmount, interestRate
                    ) VALUES (
                        @accountNo, @name, @addressLine1, @addressLine2, 
                        @addressLine3, @town, @balance, @accountType, @overdraftAmount, @interestRate
                    )";

                command.Parameters.AddWithValue("@accountNo", ba.accountNo);
                command.Parameters.AddWithValue("@name", EncryptionHelper.Encrypt(ba.name));
                command.Parameters.AddWithValue("@addressLine1", EncryptionHelper.Encrypt(ba.address_line_1));
                command.Parameters.AddWithValue("@addressLine2", EncryptionHelper.Encrypt(ba.address_line_2));
                command.Parameters.AddWithValue("@addressLine3", EncryptionHelper.Encrypt(ba.address_line_3));
                command.Parameters.AddWithValue("@town", EncryptionHelper.Encrypt(ba.town));
                command.Parameters.AddWithValue("@balance", ba.balance);
                command.Parameters.AddWithValue("@accountType", ba.GetType() == typeof(Current_Account) ? 1 : 2);
                command.Parameters.AddWithValue("@overdraftAmount", ba is Current_Account ? ((Current_Account)ba).overdraftAmount : (object)DBNull.Value);
                command.Parameters.AddWithValue("@interestRate", ba is Savings_Account ? ((Savings_Account)ba).interestRate : (object)DBNull.Value);

                command.ExecuteNonQuery();
            }

            LogTransaction("Account Creation", ba.accountNo, ba.name, "Account successfully created.");
            return ba.accountNo;
        }

        public Bank_Account findBankAccountByAccNo(String accNo)
        {
            foreach (Bank_Account ba in accounts)
            {
                if (ba.accountNo.Equals(accNo))
                {
                    return ba;
                }
            }
            return null;
        }

        public bool closeBankAccount(String accNo)
        {
            Bank_Account toRemove = null;

            foreach (Bank_Account ba in accounts)
            {
                if (ba.accountNo.Equals(accNo))
                {
                    toRemove = ba;
                    break;
                }
            }

            if (toRemove == null)
                return false;
            else
            {
                accounts.Remove(toRemove);

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM Bank_Accounts WHERE accountNo = @accountNo";
                    command.Parameters.AddWithValue("@accountNo", toRemove.accountNo);
                    command.ExecuteNonQuery();
                }

                LogTransaction("Account Closure", accNo, toRemove.name, "Account successfully closed.");
                return true;
            }
        }

        public bool lodge(String accNo, double amountToLodge)
        {
            Bank_Account toLodgeTo = null;

            foreach (Bank_Account ba in accounts)
            {
                if (ba.accountNo.Equals(accNo))
                {
                    ba.lodge(amountToLodge);
                    toLodgeTo = ba;
                    break;
                }
            }

            if (toLodgeTo == null)
                return false;
            else
            {
                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE Bank_Accounts SET balance = @balance WHERE accountNo = @accountNo";
                    command.Parameters.AddWithValue("@balance", toLodgeTo.balance);
                    command.Parameters.AddWithValue("@accountNo", toLodgeTo.accountNo);
                    command.ExecuteNonQuery();
                }

                LogTransaction("Lodgement", accNo, toLodgeTo.name, $"Amount lodged: €{amountToLodge}");
                return true;
            }
        }

        public bool withdraw(String accNo, double amountToWithdraw)
        {
            Bank_Account toWithdrawFrom = null;
            bool result = false;

            foreach (Bank_Account ba in accounts)
            {
                if (ba.accountNo.Equals(accNo))
                {
                    result = ba.withdraw(amountToWithdraw);
                    toWithdrawFrom = ba;
                    break;
                }
            }

            if (toWithdrawFrom == null || result == false)
                return false;
            else
            {
                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE Bank_Accounts SET balance = @balance WHERE accountNo = @accountNo";
                    command.Parameters.AddWithValue("@balance", toWithdrawFrom.balance);
                    command.Parameters.AddWithValue("@accountNo", toWithdrawFrom.accountNo);
                    command.ExecuteNonQuery();
                }

                LogTransaction("Withdrawal", accNo, toWithdrawFrom.name, $"Amount withdrawn: €{amountToWithdraw}");
                return true;
            }
        }

        private void LogTransaction(string transactionType, string accountNo, string accountName, string details)
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
    }
}
