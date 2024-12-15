using System;
using System.DirectoryServices.AccountManagement;

namespace Banking_Application
{
    public static class AuthenticationHelper
    {
        private static int failedLoginAttempts = 0;
        private const int MaxFailedAttempts = 5;
        private static DateTime? lockoutUntil = null;

        /// <summary>
        /// Authenticates a user against Active Directory.
        /// Implements rate limiting to prevent brute-force attacks.
        /// </summary>
        public static bool AuthenticateUser(string username, string password)
        {
            if (lockoutUntil.HasValue && DateTime.Now < lockoutUntil)
            {
                Console.WriteLine($"Account is locked. Try again after {lockoutUntil.Value}.");
                return false;
            }

            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, "ITSLIGO.LAN"))
                {
                    bool isValid = context.ValidateCredentials(username, password);
                    if (!isValid)
                    {
                        Console.WriteLine("Invalid credentials.");
                        failedLoginAttempts++;
                        if (failedLoginAttempts >= MaxFailedAttempts)
                        {
                            lockoutUntil = DateTime.Now.AddMinutes(5); // Lock for 5 minutes
                            Console.WriteLine("Too many failed attempts. Account locked for 5 minutes.");
                        }
                        return false;
                    }

                    // Reset failed attempts on successful login
                    failedLoginAttempts = 0;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a user belongs to a specific Active Directory group.
        /// </summary>
        public static bool IsUserInGroup(string username, string groupName)
        {
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, "ITSLIGO.LAN"))
                using (var user = UserPrincipal.FindByIdentity(context, username))
                {
                    if (user == null)
                    {
                        Console.WriteLine($"User {username} not found in the domain.");
                        return false;
                    }

                    foreach (var group in user.GetAuthorizationGroups())
                    {
                        if (group.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
                Console.WriteLine($"User {username} is not a member of {groupName}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Group check failed: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Checks if a user is a Bank Teller or Administrator and returns appropriate access level.
        /// </summary>
        public static string GetUserAccessLevel(string username)
        {
            try
            {
                if (IsUserInGroup(username, "Bank Teller Administrator"))
                {
                    return "Administrator";
                }
                if (IsUserInGroup(username, "Bank Teller"))
                {
                    return "Teller";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error determining user access level: {ex.Message}");
            }
            return "None";
        }
    }
}
