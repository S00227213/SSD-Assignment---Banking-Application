using System;
using System.DirectoryServices.AccountManagement;

namespace Banking_Application
{
    public static class AuthenticationHelper
    {
        /// <summary>
        /// Authenticates a user against Active Directory.
        /// </summary>
        public static bool AuthenticateUser(string username, string password)
        {
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, "ITSLIGO.LAN"))
                {
                    bool isValid = context.ValidateCredentials(username, password);
                    if (!isValid)
                    {
                        Console.WriteLine("Invalid credentials.");
                    }
                    return isValid;
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
